using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class PreviousDocumentValidation : EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentValidation
	{
		public PreviousDocumentValidation(PreviousDocument parent)
			: base(parent)
		{
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();

			var parent = Parent;
			if (!parent.CSI_Code.IsEmpty || !parent.CSI_SubType.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.CSI_ReferenceNumberInfo);
			}
			if (parent.CSI_Code == PreviousDocumentCodeList.Codes.IST && !parent.CSI_ReferenceNumber.IsEmpty)
			{
				var header = ComplementaryJobISTFinder.FindFromReferenceNumber(parent.Factory, parent.CSI_ReferenceNumber, parent.Declaration?.CountryCode ?? Core.Constants.CountryCodes.France);
				if (header is null)
				{
					parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("AC8CC1B9-2DF9-4630-897B-D5E3989335EF", "Could not find matching temporary storage register using reference {0}.", parent.CSI_ReferenceNumber));
				}
			}
			else if (parent.CSI_Code == UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage && !parent.CSI_ReferenceNumber.IsEmpty)
			{
				if (ISTRegHeader is null)
				{
					parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("80EF599B-ED56-4E86-8F9C-AF46F7E7966C", "Could not find matching temporary storage register using reference {0}.", parent.CSI_ReferenceNumber));
				}
			}
		}

		protected override void CheckCSI_PackType()
		{
			base.CheckCSI_PackType();
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_PackTypeInfo);

			var parent = Parent;
			if (parent.CSI_Code == UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage
				&& RegisterMatchingLine is CusTempStorageRegLine matchingLine
				&& matchingLine.SRL_PackageType != Parent.CSI_PackType)
			{
				parent.CSI_PackTypeInfo.AddMessageError(Res.GetString("BB18D337-97EC-47E0-B3BF-6B39D62DBDB3", "The entered package type does not equal the package type for this line on the IST."));
			}
		}

		protected override void CheckCSI_PackQty()
		{
			base.CheckCSI_PackQty();

			var parent = Parent;
			if (parent.CSI_Code == UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage
				&& RegisterMatchingLine is CusTempStorageRegLine matchingLine
				&& matchingLine.SRL_PackagesRemaining < Parent.CSI_PackQty)
			{
				parent.CSI_PackQtyInfo.AddMessageError(Res.GetString("DF7E85B3-DE19-4780-8AAE-D904220E92DB", "The entered package quantity should be less than or equal to the corresponding line in the IST."));
			}
		}

		protected override void CheckCSI_UnitOfQuantity()
		{
			base.CheckCSI_UnitOfQuantity();
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_UnitOfQuantityInfo);
			var parent = Parent;
			if (parent.CSI_Code == UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage
				&& RegisterMatchingLine is CusTempStorageRegLine matchingLine
				&& matchingLine.SRL_GrossWeightUQ != Parent.CSI_UnitOfQuantity)
			{
				parent.CSI_UnitOfQuantityInfo.AddMessageError(Res.GetString("B42AF78A-777F-4139-BBB3-37AFE307A53C", "The entered measurement unit does not equal the unit for this line on the IST."));
			}
		}

		protected override void CheckCSI_ItemNumber()
		{
			base.CheckCSI_ItemNumber();
			var parent = Parent;
			if (parent.CSI_Code == UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage
				&& ISTRegHeader is not null
				&& RegisterMatchingLine is null)
			{
				parent.CSI_ItemNumberInfo.AddMessageError(Res.GetString("F87A9FA8-7B89-48F9-A3CD-DB36DD03C93C", "The entered Goods Item Identifier does not exist for this IST."));
			}
		}

		protected override void CheckCSI_Quantity()
		{
			base.CheckCSI_Quantity();
			var parent = Parent;
			if (parent.CSI_Code == UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage
				&& RegisterMatchingLine is CusTempStorageRegLine matchingLine
				&& matchingLine.GrossWeightRemainingCalculated < Parent.CSI_Quantity)
			{
				parent.CSI_QuantityInfo.AddMessageError(Res.GetString("C0935248-CB8D-411A-A6BB-CBFEF2DE6277", "The entered quantity should be less than or equal to the corresponding line in the IST."));
			}
		}

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();
			CheckSingleNMRNInDeclarationLevelOnly();
		}

		protected override void CheckCSI_DateOfIssue()
		{
			base.CheckCSI_DateOfIssue();
			var parent = Parent;
			if (parent.Parent is JobDeclaration declaration
				&& declaration is IPreviousDocumentsProviderWithValidationDecider provider
				&& provider.ValidationDecider is IPreviousDocumentValidationDecider validationDecider
				&& declaration.IsDeclarationStandard
				&& parent.CSI_Code == UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.NMRN)
			{
				if (validationDecider.IsRuleNAT_130Active && parent.CSI_DateOfIssue.IsEmpty)
				{
					parent.CSI_DateOfIssueInfo.AddMessageError(Res.GetString("CCEF8967-F6DD-44E4-9B2B-4AE6766E2CB4", "[NAT_130] When declaration has E0001 Additional Information, the NMRN document must have a Date of Issue."));
				}
				if (validationDecider.ISRuleNAT_259Active && parent.CSI_DateOfIssue > ZDateTime.UtcNow)
				{
					parent.CSI_DateOfIssueInfo.AddMessageError(Res.GetString("603CA876-971D-4675-A315-17AA7D0382FE", "[NAT_259] When declaration has E0001 Additional Information, the NMRN is document Date of Issue (i.e the status date of IE429 response)."));
				}
			}
		}

		void CheckSingleNMRNInDeclarationLevelOnly()
		{
			var parent = Parent;
			if (parent.Parent is IPreviousDocumentsProviderWithValidationDecider provider
				&& provider.ValidationDecider is IPreviousDocumentValidationDecider validationDecider
				&& validationDecider.IsRuleNAT_088Active
				&& (parent.Declaration as JobDeclaration).IsDeclarationStandard
				&& parent.CSI_Code == UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.NMRN)
			{
				if (parent.Parent is JobDeclaration declaration
					&& declaration.PreviousDocuments.OfType<PreviousDocument>().Count(info => info.CSI_Code == UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.NMRN) > 1)
				{
					parent.CSI_CodeInfo.AddMessageError(Res.GetString("C82C867C-482A-4C67-8DDA-5726115AA4A3", "[NAT_088] There can be only one NMRN document when declaration has E0001 Additional Information."));
				}
				else if (parent.Parent is not JobDeclaration)
				{
					parent.CSI_CodeInfo.AddMessageError(Res.GetString("381352C3-B857-48D4-84CE-5B1CDBE2F5AA", "[NAT_088] NMRN is only allowed at Declaration level when Declaration has E0001 Additional Information."));
				}
			}
		}

		CusTempStorageRegHeader ISTRegHeader
		{
			get
			{
				var parent = Parent;
				return parent.Factory.GetCachedValue(parent.CSI_ReferenceNumber + parent.CSI_Code, () =>
				{
					return GetMatchingRegister(Parent.Factory, parent.CSI_ReferenceNumber);
				});
			}
		}

		static CusTempStorageRegHeader GetMatchingRegister(BusinessObjectFactory factory, ZString referenceNumber)
		{
			CusTempStorageRegHeader result = null;
			if (!referenceNumber.IsEmpty)
			{
				result = factory.LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_Reference, referenceNumber));
			}
			return result;
		}

		CusTempStorageRegLine RegisterMatchingLine => ISTRegHeader?.CusTempStorageRegLines.Cast<CusTempStorageRegLine>().FirstOrDefault(x => x.SRL_LineNumber == Parent.CSI_ItemNumber);

		protected new PreviousDocument Parent => (PreviousDocument)base.Parent;
	}
}
