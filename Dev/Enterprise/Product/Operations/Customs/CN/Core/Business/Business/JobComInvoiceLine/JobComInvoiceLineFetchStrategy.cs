using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.Business.FetchStrategies
{
	public class JobComInvoiceLineFetchStrategy : Customs.Business.FetchStrategies.JobComInvoiceLineFetchStrategy
	{
		public JobComInvoiceLineFetchStrategy(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		protected new JobComInvoiceLine BusinessObject => (JobComInvoiceLine)base.BusinessObject;

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			AddFetchHints(Factory);
			Factory.AddFetchHint(StmNoteSchema.ST_ParentID, BusinessObject.PK);
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			AddFetchHints(Factory);

			var effectiveDate = BusinessObject.EffectiveAssessmentDate;
			Factory.AddRefCusCodeListFetchHintIfNotEmpty(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DistrictCode, BusinessObject.JI_DestinationDistrict, effectiveDate);
			Factory.AddRefCusCodeListFetchHintIfNotEmpty(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DistrictCode, BusinessObject.JI_OriginDistrict, effectiveDate);
			Factory.AddRefCusCodeListFetchHintIfNotEmpty(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNCIQDistricts, BusinessObject.JI_DestinationRegion, effectiveDate);
			Factory.AddRefCusCodeListFetchHintIfNotEmpty(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNCIQDistricts, BusinessObject.JI_OriginRegion, effectiveDate);
			Factory.AddRefCusCodeListFetchHintIfNotEmpty(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNCIQStates, BusinessObject.JI_CIQOriginState, effectiveDate);
			Factory.AddFetchHint(StmNoteSchema.ST_ParentID, BusinessObject.PK);
		}

		void AddFetchHints(BusinessObjectFactory factory)
		{
			factory.AddFetchHint(CusCodeDataSchema.CY_ParentID, BusinessObject.PK);
			factory.AddFetchHint(CusAddInfoSchema.B7_ParentID, BusinessObject.PK);
			factory.AddFetchHint(CusSupportingInfoSchema.CSI_ParentID, BusinessObject.PK);
			factory.AddFetchHint(UNDGDataItemSchema.DI_ParentID, BusinessObject.PK);
			factory.AddFetchHint(JobComInvLineRefsSchema.JG_JI, BusinessObject.PK);
		}
	}
}
