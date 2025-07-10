using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class SupportingDocumentValueSetStrategy : IValueSetStrategy
	{
		public SupportingDocumentValueSetStrategy(SupportingDocument supportingDocument)
		{
			this.supportingDocument = supportingDocument;
		}

		readonly SupportingDocument supportingDocument;

		public void ValueSet(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			switch (valueThatHasChanged.Name)
			{
				case SupportingDocument.Schema.CSI_Code:
					HandleSettingOfCSI_Code();
					HandleSettingOfCSI_DateOfExpiry();
					HandleSettingOfCSI_IsDTP();
					HandleSettingOfCSI_UnitOfQuantity();
					break;
				case SupportingDocument.Schema.CSI_Quantity3:
				case SupportingDocument.Schema.CSI_DateOfIssue:
					HandleSettingOfCSI_DateOfExpiry();
					break;
			}
		}

		void HandleSettingOfCSI_Code()
		{
			DefaultReferenceIfNecessary();
			DefaultDateOfIssueIfNecessary();
		}

		void DefaultReferenceIfNecessary()
		{
			if (supportingDocument.CSI_Code == VATDeferStrategyCodeList.Codes.IdentifiedVATNumber)
			{
				if (supportingDocument.Declaration is JobDeclaration declaration && declaration.Importer is OrgHeader importer)
				{
					var vatDeferNumber = declaration.VATNumberSupporter.GetVATDeferNumberForAutoliquidation(importer);
					supportingDocument.CSI_ReferenceNumber = vatDeferNumber;
				}
			}
			else if (supportingDocument.CSI_Code == VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber)
			{
				supportingDocument.CSI_ReferenceNumber = (NoResString)"Redevable non identifié à la TVA en France";
			}
			else if (supportingDocument.CSI_Code == VATDeferStrategyCodeList.Codes.VisaFreeAi2VatOnlyAdditionalCode)
			{
				supportingDocument.CSI_ReferenceNumber = supportingDocument.Declaration?.Ai2Permit?.CPH_Number ?? ZString.Empty;
			}
		}

		void DefaultDateOfIssueIfNecessary()
		{
			switch (supportingDocument.CSI_Code)
			{
				case VATDeferStrategyCodeList.Codes.VisaFreeAi2VatOnlyAdditionalCode:
					supportingDocument.CSI_DateOfIssue = supportingDocument.Declaration?.Ai2Permit?.CPH_StartDate ?? ZDateTime.Now;
					break;
				case VATDeferStrategyCodeList.Codes.IdentifiedVATNumber:
				case VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber:
					if (supportingDocument.CSI_DateOfIssue.IsEmpty)
					{
						supportingDocument.CSI_DateOfIssue = ZDateTime.Now;
					}
					break;
				default:
					break;
			}
		}

		void HandleSettingOfCSI_UnitOfQuantity()
		{
			if (supportingDocument.ShowDropEditForUnitOfQuantity)
			{
				var list = supportingDocument.Lookups.UnitOfQuantityList;
				if (list.Count == 1)
				{
					var unitOfQuantityToSet = list[0].Code;
					supportingDocument.CSI_UnitOfQuantity = unitOfQuantityToSet;
				}
			}
		}

		void HandleSettingOfCSI_IsDTP()
		{
			supportingDocument.CSI_IsDTP = supportingDocument.IsDTP;
		}

		void HandleSettingOfCSI_DateOfExpiry()
		{
			if (supportingDocument.IsD48)
			{
				if (supportingDocument.CSI_Quantity3 == 0 || !supportingDocument.CSI_DateOfIssue.IsValid)
				{
					supportingDocument.CSI_DateOfExpiry = ZDateTime.Empty;
				}
				else
				{
					supportingDocument.CSI_DateOfExpiry = supportingDocument.CSI_DateOfIssue.AddMonths(supportingDocument.CSI_Quantity3.ToZInt());
				}
			}
		}
	}
}
