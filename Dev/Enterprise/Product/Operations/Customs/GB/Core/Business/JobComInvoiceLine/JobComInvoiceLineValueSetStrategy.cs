using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using SupportingDocument = Enterprise.Customs.GB.Business.Declaration.SupportingDocument;

namespace Enterprise.Customs.GB.Business
{
	public abstract class JobComInvoiceLineValueSetStrategy : EU.Business.Declaration.JobComInvoiceLineValueSetStrategy
	{
		protected JobComInvoiceLineValueSetStrategy(Declaration.JobComInvoiceLine invoiceLine)
			: base()
		{
			this.invoiceLine = invoiceLine;
		}

		const string Doc9200 = "9200";
		protected string doc9WKS = "9WKS";

		public override void HandleSettingOfNewValuationMethod()
		{
			// See tariff V3 page 3-10. 
			base.HandleSettingOfNewValuationMethod();
			switch (invoiceLine.JI_ValuationCode)
			{
				case ValuationMethodList.Codes._1:
					CreateSupportingDocumentForValueMethodOne();
					DeleteSupportingDocument(Doc9200);
					DeleteSupportingDocument(doc9WKS);
					break;
				case ValuationMethodList.Codes._2:
				case ValuationMethodList.Codes._3:
					CreateSupportingDocument(Doc9200);
					DeleteSupportingDocument(doc9WKS);
					DeleteSupportingDocument(SupportingDocumentTypeForValueMethodOne);
					break;
				case ValuationMethodList.Codes._4:
				case ValuationMethodList.Codes._5:
				case ValuationMethodList.Codes._6:
					CreateSupportingDocument(Doc9200);
					CreateSupportingDocument(doc9WKS, invoiceLine.Declaration.JE_DeclarationReference, string.Format("SEE ATTACHED WORKSHEET {0}", invoiceLine.Declaration.JE_DeclarationReference));
					DeleteSupportingDocument(SupportingDocumentTypeForValueMethodOne);
					break;

				default:
					DeleteSupportingDocument(SupportingDocumentTypeForValueMethodOne);
					DeleteSupportingDocument(Doc9200);
					DeleteSupportingDocument(doc9WKS);
					break;
			}
		}

		protected void CreateSupportingDocument(string sdType, string sdReferenceNumber = "", string sdReasonDescription = "")
		{
			SupportingDocument suppDoc = null;
			MeasuresToTaxAndDocsHelper.FindExistingOrAddNewSupportingDocument(sdType, invoiceLine, out suppDoc);
			if (!string.IsNullOrEmpty(sdReferenceNumber))
			{
				suppDoc.CSI_ReferenceNumber = sdReferenceNumber;
			}

			if (!string.IsNullOrEmpty(sdReasonDescription))
			{
				suppDoc.CSI_Description = sdReasonDescription;
			}
		}

		void DeleteSupportingDocument(string sdType)
		{
			MeasuresToTaxAndDocsHelper.FindExistingSupportingDocument(sdType, invoiceLine, out SupportingDocument sd);
			sd?.Delete();
		}

		protected readonly Declaration.JobComInvoiceLine invoiceLine;

		protected abstract void CreateSupportingDocumentForValueMethodOne();

		protected abstract string SupportingDocumentTypeForValueMethodOne { get; }

		#region IValueSetStrategy Members

		protected override void ValueSetCore(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			base.ValueSetCore(valueThatHasChanged, oldValue);
			switch (valueThatHasChanged.Name)
			{
				case JobComInvoiceLine.Schema.JI_CountryOfOrigin:
					ValidateRelatedFECChallenge(FECChallengeFields.Codes.JI_ORG);
					break;
				case JobComInvoiceLine.Schema.JI_CustomsSecondQuantity:
					ValidateRelatedFECChallenge(FECChallengeFields.Codes.JI_Supp);
					break;
				case JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty:
					ValidateRelatedFECChallenge(FECChallengeFields.Codes.JI_SuppUQ);
					break;
				case JobComInvoiceLine.Schema.JI_CustomsQuantity:
					ValidateRelatedFECChallenge(FECChallengeFields.Codes.JI_NettMass);
					break;
				case JobComInvoiceLine.Schema.JI_LinePrice:
					ValidateRelatedFECChallenge(FECChallengeFields.Codes.JI_Price);
					break;
				case JobComInvoiceLine.Schema.JI_CustomsUnitQty:
					ValidateRelatedFECChallenge(FECChallengeFields.Codes.JI_NettMassUQ);
					break;
			}
		}

		void ValidateRelatedFECChallenge(ZString fecFieldCode)
		{
			var entryLine = this.invoiceLine.CusEntryLine;
			entryLine?.Header?.FECChallenges.Find(x => x.CY_Code == fecFieldCode && x.CY_ParentID == entryLine.PK).ForEach(x => x.Validation.ValidateCY_IsOverridden());
		}

		#endregion
	}
}
