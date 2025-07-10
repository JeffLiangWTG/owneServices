using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business
{
	public class EPaymentBeneficiaryRequest : AccEPaymentBeneficiaryRequest, IAccountingNumberFountainDataSource
	{
		public EPaymentBeneficiaryRequest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZDateTime PostDate => ABR_SystemCreateTimeUtc;

		public GlbBranch Branch => GlbBranch.CurrentBranch;

		public GlbDepartment Department => GlbDepartment.CurrentDepartment;

		IDbConnected IAccountingNumberFountainDataSource.Factory => Factory;

		public override void OnSaving()
		{
			if (!IsInDatabase)
			{
				ABR_InternalReference = AccountingNumberFountainWrapperFactory.Instance.EPaymentBeneficiaryRequestInternalRef.Generate(this);
			}
			base.OnSaving();
		}
	}
}
