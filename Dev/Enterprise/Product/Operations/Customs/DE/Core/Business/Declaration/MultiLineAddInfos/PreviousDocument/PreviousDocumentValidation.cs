using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class PreviousDocumentValidation : EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentValidation
	{
		public PreviousDocumentValidation(PreviousDocument parent)
			: base(parent)
		{
		}

		protected new PreviousDocument Parent => (PreviousDocument)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateAuthorizationNumber();
			ValidateCSI_ItemNumberString();
		}

		public void ValidateCSI_ItemNumberString()
		{
			ValidateCalculatedProperty(Parent.CSI_ItemNumberStringInfo);
		}

		protected void CheckCSI_ItemNumberString()
		{
			if (IsAvailable(PreviousDocument.Schema.CSI_ItemNumberString))
			{
				CheckCSI_ItemNumberStringCore();
			}
		}

		protected virtual void CheckCSI_ItemNumberStringCore()
		{
		}

		public void ValidateAuthorizationNumber()
		{
			ValidateCalculatedProperty(Parent.AuthorizationNumberInfo);
		}

		protected void CheckAuthorizationNumber()
		{
			if (IsAvailable(PreviousDocument.Schema.AuthorizationNumber))
			{
				CheckAuthorizationNumberCore();
			}
		}

		protected virtual void CheckAuthorizationNumberCore()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.AuthorizationNumberInfo, Res.GetString("22357066-1411-4c42-a6e8-6b8811af5bca", "Authorization Number"));
		}

		protected override sealed void CheckCSI_ReferenceNumber()
		{
			if (IsAvailable(PreviousDocument.Schema.CSI_ReferenceNumber))
			{
				CheckCSI_ReferenceNumberCore();
			}
		}

		protected virtual void CheckCSI_ReferenceNumberCore()
		{
		}

		protected override sealed void CheckCSI_ReferenceNumber2()
		{
			if (IsAvailable(PreviousDocument.Schema.CSI_ReferenceNumber2))
			{
				CheckCSI_ReferenceNumber2Core();
			}
		}

		protected virtual void CheckCSI_ReferenceNumber2Core()
		{
		}

		protected override sealed void CheckCSI_Code()
		{
			if (IsAvailable(PreviousDocument.Schema.CSI_Code))
			{
				CheckCSI_CodeCore();
			}
		}

		protected virtual void CheckCSI_CodeCore()
		{
			base.CheckCSI_Code();
		}

		protected override sealed void CheckCSI_DateOfIssue()
		{
			if (IsAvailable(PreviousDocument.Schema.CSI_DateOfIssue))
			{
				CheckCSI_DateOfIssueCore();
			}
		}

		protected virtual void CheckCSI_DateOfIssueCore()
		{
			base.CheckCSI_DateOfIssue();
		}

		protected override sealed void CheckCSI_SubType()
		{
			if (IsAvailable(PreviousDocument.Schema.CSI_SubType))
			{
				CheckCSI_SubTypeCore();
			}
		}

		protected virtual void CheckCSI_SubTypeCore()
		{
			base.CheckCSI_SubType();
		}

		protected override sealed void CheckCSI_CustomsOffice()
		{
			if (IsAvailable(PreviousDocument.Schema.CSI_CustomsOffice))
			{
				CheckCSI_CustomsOfficeCore();
			}
		}

		protected virtual void CheckCSI_CustomsOfficeCore()
		{
			base.CheckCSI_CustomsOffice();
		}

		protected override sealed void CheckCSI_Status()
		{
			if (IsAvailable(PreviousDocument.Schema.CSI_Status) || Parent.IsProcedureATAV || Parent.IsProcedureATZL)
			{
				CheckCSI_StatusCore();
			}
		}

		protected virtual void CheckCSI_StatusCore()
		{
			base.CheckCSI_Status();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_StatusInfo);
		}

		protected override sealed void CheckCSI_LineNo()
		{
			if (IsAvailable(PreviousDocument.Schema.CSI_LineNo))
			{
				CheckCSI_LineNoCore();
			}
		}

		protected virtual void CheckCSI_LineNoCore()
		{
			base.CheckCSI_LineNo();
		}

		protected override sealed void CheckCSI_Description()
		{
			if (IsAvailable(PreviousDocument.Schema.CSI_Description))
			{
				CheckCSI_DescriptionCore();
			}
		}

		protected virtual void CheckCSI_DescriptionCore()
		{
			base.CheckCSI_Description();
			if (Parent.IsProcedureATAV)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_DescriptionInfo, Res.GetString("ff88cd3b-ed25-4f0c-b368-dc8b9ad6b4a5", "Goods Related Information"));
			}
		}

		protected override sealed void CheckCSI_Tariff()
		{
			if (IsAvailable(PreviousDocument.Schema.CSI_Tariff))
			{
				CheckCSI_TariffCore();
			}
		}

		protected virtual void CheckCSI_TariffCore()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_TariffInfo, Res.GetString("ae8f3fda-a5f4-43cf-833e-d3019e35cd1e", "Commodity Code"));

			var tariff = Parent.CSI_Tariff;
			if (!tariff.IsEmpty && (tariff.Length != 11 || !tariff.IsNumbersOnlyOrEmpty || IsUnknownTariffCode()))
			{
				Parent.CSI_TariffInfo.AddMessageError(Res.GetString("70AC8DC3-2B4B-46D9-BD03-061BE4021919", "The entered Commodity Code is not valid."));
			}
			bool IsUnknownTariffCode() => new TariffView.Loader(Parent.Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.Germany, Constants.TariffTypes.Import, tariff, ZDateTime.Now) == null;
		}

		protected override sealed void CheckCSI_UnitOfQuantity()
		{
			if (IsAvailable(PreviousDocument.Schema.CSI_UnitOfQuantity))
			{
				CheckCSI_UnitOfQuantityCore();
			}
		}

		protected virtual void CheckCSI_UnitOfQuantityCore()
		{
		}

		protected override sealed void CheckCSI_UnitOfQuantity2()
		{
			if (IsAvailable(PreviousDocument.Schema.CSI_UnitOfQuantity2))
			{
				CheckCSI_UnitOfQuantity2Core();
			}
		}

		protected virtual void CheckCSI_UnitOfQuantity2Core()
		{
		}

		protected override sealed void CheckCSI_Quantity()
		{
			base.CheckCSI_Quantity();

			if (IsAvailable(PreviousDocument.Schema.CSI_Quantity))
			{
				CheckCSI_QuantityCore();
			}
		}

		protected virtual void CheckCSI_QuantityCore()
		{
			if (Parent.UsualProcessingFlag && Parent.IsProcedureATZL)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_QuantityInfo, Res.GetString("de75f15c-b380-41f5-a491-67e61681f6e3", "Commercial Qty"));
			}
		}

		protected override sealed void CheckCSI_Quantity2()
		{
			if (IsAvailable(PreviousDocument.Schema.CSI_Quantity2))
			{
				CheckCSI_Quantity2Core();
			}
		}
		protected virtual void CheckCSI_Quantity2Core()
		{
		}

		protected virtual bool IsAvailable(string propertyName) //Checks whether the given property is shown in the grid, (i.e. whether it should be validated or not)
			=> new PreviousDocumentConfiguration().GetAvailableColumnsFromProcedureCode(Parent.IsImport, Parent.CSI_Procedure, Parent.CusEntryInstructionParentStyle).Select(x => x.ColumnName).Contains(propertyName);
	}
}
