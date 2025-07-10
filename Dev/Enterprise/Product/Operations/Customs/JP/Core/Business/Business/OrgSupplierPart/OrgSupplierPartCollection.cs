using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.Business
{
	public class OrgSupplierPartCollection : Customs.Business.OrgSupplierPartCollection
	{
		public OrgSupplierPartCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgSupplierPartCollection(BusinessObjectFactory factory, JobComInvoiceLine invoiceLine, bool isExport)
			: base(factory, invoiceLine, isExport)
		{
		}

		public OrgSupplierPartCollection(BusinessObjectFactory factory, BaseJobComInvoiceLine invoiceLine, OrgHeader supplier, OrgHeader owner, bool isExport)
			: base(factory, invoiceLine, supplier, owner, isExport)
		{
		}

		protected override void AddPivotWithAdditionalLineDetailsCore(Customs.Business.OrgSupplierPart part, BaseJobComInvoiceLine invoiceLine)
		{
			base.AddPivotWithAdditionalLineDetailsCore(part, invoiceLine);
			var orgSupplierPart = (OrgSupplierPart)part;
			var jobComInvoiceLine = (JobComInvoiceLine)invoiceLine;
			var pivot = orgSupplierPart.PivotsForBinding.Cast<CusClassPartPivot>().FirstOrDefault();
			if (pivot != null)
			{
				CopyValuesFromInvoiceLine(pivot, jobComInvoiceLine);
			}

			void CopyValuesFromInvoiceLine(CusClassPartPivot pivot, JobComInvoiceLine invoiceLine)
			{
				pivot.CI_Description = invoiceLine.JI_Description;
				pivot.CI_RN_NKCountryOfOrigin = invoiceLine.JI_CountryOfOrigin;
				pivot.CI_PrimaryPreference = invoiceLine.JI_PrimaryPreference;
				pivot.CI_SecondaryPreference = invoiceLine.JI_SecondaryPreference;
				pivot.CI_TradeControlOrderAppendix = invoiceLine.JI_TradeControlOrderAppendix;
				pivot.CI_FEFTAArticle48 = invoiceLine.JI_FEFTAArticle48;
				pivot.CI_StorageType = invoiceLine.JI_StorageType;
				pivot.CI_AdvanceRulingOnClassification = invoiceLine.JI_AdvanceRulingOnClassification;
				pivot.CI_AdvanceRulingOnOrigin = invoiceLine.JI_AdvanceRulingOnOrigin;
				pivot.CI_DutyReductionExemptionRefundCode = invoiceLine.JI_DutyReductionExemptionRefundCode;
				pivot.CI_DomesticConsumptionTaxExemptionCode = invoiceLine.JI_DomesticConsumptionTaxExemptionCode;
				pivot.CI_DomesticConsumptionTaxExemptionIsPartial = invoiceLine.JI_DomesticConsumptionTaxExemptionIsPartial;
				pivot.CI_DutyReductionAmount = invoiceLine.JI_DutyReductionAmount;
			}
		}
	}
}
