using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.Testing
{
	public class TestTransactionGenerator
	{
		public TestTransactionGenerator(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}

		public JASForwardingShipment GenerateShipment(ZString houseBillNum, ZString transportMode, ZString origin, ZString dest)
		{
			JASForwardingShipment testShipment = Factory.NewWithValidTestData<JASForwardingShipment>();
			testShipment.JS_E_DEP = ZDateTime.BrettsBirthday;
			testShipment.JS_HouseBill = houseBillNum;
			testShipment.JS_TransportMode = transportMode;
			testShipment.JS_RL_NKOrigin = origin;
			testShipment.JS_RL_NKDestination = dest;
			testShipment.JS_UniqueConsignRef = houseBillNum;
			return testShipment;
		}

		public JASForwardingConsol GenerateConsol(ZString masterBillNum, ZString agentType, ZString portOfLoading, ZString portOfDischarge, ZString transportMode, ZString voyageFlightNum, ZDateTime departure, ZDateTime arrival)
		{
			JASForwardingConsol testConsol = GenerateConsol(masterBillNum, agentType, portOfLoading, transportMode);
			VoyageOrigin voyOrigin = Factory.New<VoyageOrigin>();
			voyOrigin.JA_A_DEP = departure;
			voyOrigin.JA_RL_NKPortOfLoading = portOfLoading;
			VoyageDestination voyDes = Factory.New<VoyageDestination>();
			voyDes.JB_A_ARV = arrival;
			voyDes.JB_RL_NKPortOfDischarge = portOfDischarge;
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = voyageFlightNum;
			voyage.Origins.Add(voyOrigin);
			voyage.Destinations.Add(voyDes);
			voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
			voyage.GenerateSailings();
			Transport transport = testConsol.Transports[0];
			transport.JW_JX = voyage.Sailings[0].PK;
			testConsol.JK_MasterBillNum = masterBillNum;
			return testConsol;
		}

		public JASForwardingConsol GenerateConsol(ZString masterBillNumber, ZString transportMode)
		{
			return GenerateConsol(masterBillNumber, Constants.AgentType.Agent, "AUSYD", transportMode);
		}

		public JASForwardingConsol GenerateConsol(ZString masterBillNum, ZString agentType, ZString portOfLoading, ZString transportMode)
		{
			JASForwardingConsol testConsol = Factory.NewWithValidTestData<JASForwardingConsol>();
			testConsol.JK_IsNeutralMaster = false;
			testConsol.JK_AgentType = agentType;
			testConsol.JK_TransportMode = transportMode;
			Transport transport = testConsol.Transports[0];
			transport.JW_RL_NKLoadPort = portOfLoading;
			testConsol.JK_MasterBillNum = masterBillNum;
			testConsol.JK_UniqueConsignRef = masterBillNum;
			return testConsol;
		}

		public JobHeader GenerateJob(ZGuid shipmentGuid, ZString jobNum)
		{
			JobHeader job = Factory.NewJobForTesting<JobHeader>();
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_Address1 = "Address";
			job.JH_JobNum = jobNum;
			job.JH_ParentID = shipmentGuid;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.AgentCollectPK = org.PK;
			return job;
		}

		public AccTransactionHeader GenerateAccTransactionHeader(ZGuid jobGuid, ZString ledger, ZString transactionType, ZDateTime dueDate, ZDateTime invoiceDate, ZString transactionNum, ZString desc, ZString currencyNK, ZDecimal amount, ZDecimal gSTAmount, ZDecimal outstanding, ZDecimal exchangeRate, ZGuid orgPK)
		{
			return GenerateAccTransactionHeader(jobGuid, ZString.Empty, ledger, transactionType, dueDate, invoiceDate, transactionNum, desc, currencyNK, amount, gSTAmount, outstanding, exchangeRate, orgPK);
		}

		public AccTransactionHeader GenerateAccTransactionHeader(ZGuid jobGuid, ZString ledger, ZString transactionType, ZDateTime dueDate, ZDateTime invoiceDate, ZString transactionNum, ZString desc, ZString currencyNK, ZDecimal amount, ZGuid orgPK)
		{
			return GenerateAccTransactionHeader(jobGuid, ZString.Empty, ledger, transactionType, dueDate, invoiceDate, transactionNum, desc, currencyNK, amount, 0, amount, 1M, orgPK);
		}

		public AccTransactionHeader GenerateAccTransactionHeader(ZString consolidatedInvoiceRef, ZString ledger, ZString transactionType, ZDateTime dueDate, ZDateTime invoiceDate, ZString transactionNum, ZString desc, ZString currencyNK, ZDecimal amount, ZDecimal gSTAmount, ZDecimal outstanding, ZDecimal exchangeRate, ZGuid orgPK)
		{
			return GenerateAccTransactionHeader(ZGuid.Empty, consolidatedInvoiceRef, ledger, transactionType, dueDate, invoiceDate, transactionNum, desc, currencyNK, amount, gSTAmount, outstanding, exchangeRate, orgPK);
		}

		public AccTransactionHeader GenerateAccTransactionHeader(ZString consolidatedInvoiceRef, ZString ledger, ZString transactionType, ZDateTime dueDate, ZDateTime invoiceDate, ZString transactionNum, ZString desc, ZString currencyNK, ZDecimal amount, ZGuid orgPK)
		{
			return GenerateAccTransactionHeader(ZGuid.Empty, consolidatedInvoiceRef, ledger, transactionType, dueDate, invoiceDate, transactionNum, desc, currencyNK, amount, 0, amount, 1M, orgPK);
		}

		protected AccTransactionHeader GenerateAccTransactionHeader(ZGuid jobGuid, ZString consolidatedInvoiceRef, ZString ledger, ZString transactionType, ZDateTime dueDate, ZDateTime invoiceDate, ZString transactionNum, ZString desc, ZString currencyNK, ZDecimal amount, ZDecimal gSTAmount, ZDecimal outstanding, ZDecimal exchangeRate, ZGuid orgPK)
		{
			AccTransactionHeader transaction = Factory.New<AccTransactionHeader>();
			transaction.AH_TransactionType = transactionType;
			transaction.AH_OH = orgPK;
			transaction.AH_Ledger = ledger;
			transaction.AH_InvoiceAmount = amount;
			transaction.AH_OutstandingAmount = outstanding;
			transaction.AH_ExchangeRate = exchangeRate;
			transaction.AH_GSTAmount = gSTAmount;
			transaction.AH_OSTotal = (ZDecimal)Env.CurrentCompany.ExchangeRate.LocalToForeign(amount + gSTAmount, exchangeRate, currencyNK);
			transaction.AH_DueDate = dueDate;
			transaction.AH_InvoiceDate = invoiceDate;
			transaction.AH_TransactionNum = transactionNum;
			transaction.AH_Desc = desc;
			transaction.AH_RX_NKTransactionCurrency = currencyNK;
			transaction.AH_JH = jobGuid;
			transaction.AH_ConsolidatedInvoiceRef = consolidatedInvoiceRef;
			transaction.AH_GB = GlbBranch.CurrentBranch.PK;
			transaction.AH_GE = GlbDepartment.CurrentDepartment.PK;
			return transaction;
		}

		protected BusinessObjectFactory Factory;
	}
}
