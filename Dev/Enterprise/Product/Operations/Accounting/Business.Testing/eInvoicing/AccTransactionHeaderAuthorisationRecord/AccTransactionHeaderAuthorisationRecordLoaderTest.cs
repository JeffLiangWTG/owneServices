using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing.Fiji;
using Enterprise.Accounting.Business.EInvoicing.India;
using Enterprise.Accounting.Business.EInvoicing.Samoa;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.EInvoicing.Testing
{
	public class AccTransactionHeaderAuthorisationRecordLoaderTest : TestCaseWithFactory
	{
		public void TestLoadByParentID()
		{
			var mapping = new[]
			{
				new { CountryCode = "", RecordType = typeof(AccTransactionHeaderAuthorisationRecord) },
				new { CountryCode = Enterprise.Core.Constants.CountryCodes.Fiji, RecordType = typeof(FijiAccTransactionHeaderAuthorisationRecord) },
				new { CountryCode = Enterprise.Core.Constants.CountryCodes.WesternSamoa, RecordType = typeof(SamoaAccTransactionHeaderAuthorisationRecord) },
				new { CountryCode = Enterprise.Core.Constants.CountryCodes.India, RecordType = typeof(IndiaAccTransactionHeaderAuthorisationRecord) },
			};

			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			charge.JR_AT_SellGSTRate = TestObjectCreator.GSTFREE1.PK;
			Factory.Save();

			var originalTransaction = TestObjectCreator.CreateARInvoice<ARInvoice>("AP001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
			originalTransaction.Lines.Add(TestObjectCreator.CreateRevenueLine(charge, originalTransaction.PK));
			originalTransaction.Lines[0].AL_AT = TestObjectCreator.GSTFREE1.PK;
			originalTransaction.AH_TransactionReference = "REF001";
			originalTransaction.AH_AgreedPaymentMethodOverride = OrgConstants.CreditAgreedPaymentMethods.Code.CashAndBankCheck;
			Factory.Save();

			var authorizationRecord = Factory.NewWithValidTestData<AccTransactionHeaderAuthorisationRecord>();
			authorizationRecord.AHF_ParentId = originalTransaction.PK;
			authorizationRecord.AHF_ParentTableCode = "AH";
			Factory.Save();

			foreach (var item in mapping)
			{
				var loadedRecord = AccTransactionHeaderAuthorisationRecordLoader.LoadByParentID(Factory, originalTransaction.PK, item.CountryCode);
				AssertType(item.RecordType, loadedRecord);
			}
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}
