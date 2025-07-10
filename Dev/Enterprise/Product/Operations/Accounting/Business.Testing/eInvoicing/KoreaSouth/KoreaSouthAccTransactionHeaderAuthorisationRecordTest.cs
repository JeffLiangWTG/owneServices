using Enterprise.Accounting.Business.EInvoicing.Testing;
using Enterprise.MasterFiles.Business.CountryCompliance;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.EInvoicing.KoreaSouth.Testing
{
	[TestedType(typeof(KoreaSouthAccTransactionHeaderAuthorisationRecord))]
	public class KoreaSouthAccTransactionHeaderAuthorisationRecordTest : AccTransactionHeaderAuthorisationRecordTest
	{
		public void TestSetDefaultValues()
		{
			var record = (KoreaSouthAccTransactionHeaderAuthorisationRecord)GetNewBusinessObject();
			AssertEquals(AccTransactionHeaderAuthorisationRecordTypes.KoreaSouth, record.AHF_RecordType);
		}
	}
}
