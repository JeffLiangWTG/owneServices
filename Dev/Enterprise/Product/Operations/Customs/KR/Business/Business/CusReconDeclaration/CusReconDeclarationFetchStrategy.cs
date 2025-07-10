using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public class CusReconDeclarationFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public CusReconDeclarationFetchStrategy(CusReconDeclaration cusReconDeclaration)
			: base(cusReconDeclaration)
		{
		}

		CusReconDeclaration ReconDeclaration
		{
			get { return (CusReconDeclaration)base.BusinessObject; }
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			Factory.AddFetchHint(CusReconEntrySchema.CRE_CRD, ReconDeclaration.PK);
			Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, ReconDeclaration.PK);
			foreach (var reconEntry in ReconDeclaration.CusReconEntries)
			{
				Factory.AddFetchHint(CusReconEntryLineSchema.CRL_CRE, reconEntry.PK);
			}
			foreach (var reconEntryLine in ReconDeclaration.CusReconEntryLines)
			{
				Factory.AddFetchHint(CusReconCustomsChargeSchema.CRC_CRL_Line, reconEntryLine.PK);
			}
			Factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, ReconDeclaration.PK);
			Factory.AddFetchHint(OrgAddressSchema.PK, ReconDeclaration.CRD_OA_DeclarantAddress);
		}
	}
}
