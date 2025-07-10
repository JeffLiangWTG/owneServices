using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.CountryCompliance;

namespace Enterprise.Accounting.Business.EInvoicing.KoreaSouth
{
	public class KoreaSouthAccTransactionHeaderAuthorisationRecord : AccTransactionHeaderAuthorisationRecord
	{
		public KoreaSouthAccTransactionHeaderAuthorisationRecord(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AHF_RecordType = AccTransactionHeaderAuthorisationRecordTypes.KoreaSouth;
		}
	}
}
