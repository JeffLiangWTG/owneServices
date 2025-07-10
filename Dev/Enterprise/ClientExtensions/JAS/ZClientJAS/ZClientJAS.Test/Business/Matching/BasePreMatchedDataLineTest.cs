using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.JAS.Business.Testing;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Matching.Testing
{
	public abstract class BasePreMatchedDataLineTest : BaseDataLineTest
	{
		protected AccTransactionHeaderWithJobInfo GenerateNormalTransaction(ZString ledger)
		{
			CommonShipment testShipment = Generator.GenerateShipment("TESTHB", Constants.TransportModes.Air, "AUSYD", "IDJKT");
			ForwardingConsol testConsol = Generator.GenerateConsol("MBILL", Constants.TransportModes.Air);
			testShipment.Consols.Add(testConsol);
			JobHeader testJob = Generator.GenerateJob(testShipment.PK, "J001");
			RefCurrency currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			AssertNotNull("USD Currency exists", currency);
			AccTransactionHeader testTransaction = Generator.GenerateAccTransactionHeader(testJob.PK, ledger, TransactionTypes.CreditNote, new ZDateTime(2004, 12, 1), new ZDateTime(2004, 11, 1), "TRAN0012", "This is the description", currency.RX_Code, -450, -10, -460, 0.66M, CounterpartOrg.PK);
			Factory.Save();
			return GetRecordByTransactionNum(testTransaction.AH_TransactionNum);
		}

		protected AccTransactionHeaderWithJobInfo GetRecordByTransactionNum(string transactionNum)
		{
			return GetRecordByTransactionNum(transactionNum, 1)[0];
		}

		protected AccTransactionHeaderWithJobInfo[] GetRecordByTransactionNum(string transactionNum, int expectedNum)
		{
			ZQuery filter = new ZQuery(ClientAccTransactionHeaderWithJobInfoSchema.AH_TransactionNum, transactionNum);
			AccTransactionHeaderWithJobInfo[] transactions = (AccTransactionHeaderWithJobInfo[])Factory.Load(typeof(AccTransactionHeaderWithJobInfo), filter);
			AssertEquals(expectedNum, transactions.Length);
			return transactions;
		}

		protected abstract ZString Ledger { get; }

		protected abstract PreMatchedDataLine NewPreMatchedDataLine(AccTransactionHeaderWithJobInfo transaction);
		TestTransactionGenerator fGenerator;
		protected TestTransactionGenerator Generator
		{
			get
			{
				if (fGenerator == null)
				{
					fGenerator = new TestTransactionGenerator(Factory);
				}

				return fGenerator;
			}
		}

		OrgHeader fCounterpartOrg;
		protected OrgHeader CounterpartOrg
		{
			get
			{
				if (fCounterpartOrg == null)
				{
					fCounterpartOrg = Factory.NewWithValidTestData<OrgHeader>();
					SetNettingCode(fCounterpartOrg, "TSJAS");
				}

				return fCounterpartOrg;
			}
		}

		protected OrgHeader CurrentOrg
		{
			get
			{
				return GlbCompany.CurrentCompany.OrgProxy;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.RunClientDbCreateScripts();
			AccTransactionHeaderWithJobInfo transaction = GenerateNormalTransaction(Ledger);
			Line = NewPreMatchedDataLine(transaction);
			AUDCode = "AUD";
			InvoiceDate = new ZDateTime(2004, 1, 1);
			DueDate = new ZDateTime(2004, 1, 1);
			OldOrgProxy = TestUNCAndUOCSetter.SetCurrentOrgProxy(Factory, "CRJAS", "CRJAS", "CRJAS");
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = (OldOrgProxy == null) ? ZGuid.Empty : OldOrgProxy.PK;
		}

		protected OrgHeader OldOrgProxy;
		protected PreMatchedDataLine Line;
		protected ZString AUDCode;
		protected ZDateTime InvoiceDate;
		protected ZDateTime DueDate;
	}
}
