using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Testing
{
	[TestedType(typeof(AccTransactionHeaderWithJobInfo))]
	public class AccTransactionHeaderWithJobInfoTest : EnterpriseBusinessObjectTestCase
	{
		public void TestOSOutstandingAmount()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.DataType.SuspendValidation())
			{
				AssertOSOutstandingAmount(true, true);
				AssertOSOutstandingAmount(true, false);
				AssertOSOutstandingAmount(false, true);
				AssertOSOutstandingAmount(false, false);
			}

			void AssertOSOutstandingAmount(bool registryValue, bool isApplicable)
			{
				TransactionHeader.AH_IsOSOutstandingAmountApplicable = isApplicable;
				if (isApplicable)
				{
					AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
					TransactionHeader.AH_OSOutstandingAmount = 77m;
					Factory.Save();
				}

				AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

				var expected = Factory.Load<TransactionHeader>(TransactionHeader.PK);

				var filter = new ZQuery(ClientAccTransactionHeaderWithJobInfoSchema.AH_TransactionNum, "tTR001");
				var transactions = (AccTransactionHeaderWithJobInfo[])Factory.Load(typeof(AccTransactionHeaderWithJobInfo), filter);

				AssertEquals("Ensure only one transaction with this id exist", 1, transactions.Length);
				AssertEquals("Ensure OSOutstandingAmount in Accounting is the same as in this BizO", 77m, transactions[0].AH_OSOutstandingAmount);
				AssertEquals(77m, expected.AH_Calc_OSOutstandingAmount);
			}
		}

		public void TestDelete()
		{
			AccTransactionHeaderWithJobInfo transaction = Factory.New<AccTransactionHeaderWithJobInfo>();
			transaction.Delete();
			AssertEquals("JASAccTransactionHeaderWithJobInfoDeleteIsNotSupported", ErrorReporter.LastKeyReported);
			AssertEquals("Delete is not supported for AccTransactionHeaderWithJobInfo BizO", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		protected override bool IsDeleteSupported()
		{
			return false;
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.RunClientDbCreateScripts();
			TestTransactionGenerator generator = new TestTransactionGenerator(Factory);
			TransactionHeader = generator.GenerateAccTransactionHeader(ZGuid.Empty, ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Invoice, new ZDateTime(2001, 1, 1), new ZDateTime(2001, 2, 1), "tTR001", "Desc1", "AUD", 100, 10, 110, 0.7M, ZGuid.Empty);
			Factory.Save();
		}

		protected AccTransactionHeader TransactionHeader;
	}
}
