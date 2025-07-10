using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.MFI.Testing
{
	using System;
	using CargoWise.Application;
	using CargoWise.Schema;
	using CargoWise.Types;
	using Enterprise.Accounting.Business.JobInvoicing;
	using Enterprise.Environment;
	using Enterprise.Freight.Business;
	using Enterprise.Freight.Forwarding.Business;
	using Enterprise.MasterFiles.Business;

	internal class TestHelper
	{
		public TestHelper(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}

		#region GetTransactionHeader

		public AccTransactionHeader GetTransactionHeader(ZGuid jobPK, ZDecimal amount, ZString reference, ZString ledgerType, OrgHeader billTo, ZString transType)
		{
			AccTransactionHeader result = Factory.New<AccTransactionHeader>();
			result.AH_ConsolidatedInvoiceRef = reference;
			result.AH_TransactionNum = reference;
			result.AH_OH = billTo.PK;
			result.AH_GB = GlbBranch.CurrentBranch.PK;
			result.AH_JH = jobPK;
			result.AH_GE = GlbDepartment.CurrentDepartment.PK;
			result.AH_Ledger = ledgerType;
			result.AH_TransactionType = transType;
			result.AH_InvoiceAmount = amount;
			result.AH_OutstandingAmount = amount;
			result.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.LocalCurrency.RX_Code;
			result.AH_InvoiceDate = ZDateTime.Now;
			result.AH_PostDate = ZDateTime.Now;
			return result;
		}

		#endregion

		#region GetTransactionHeaderWithDept

		public AccTransactionHeader GetTransactionHeaderWithDept(ZGuid dept, ZGuid jobPK, ZDecimal amount, ZString reference, ZString ledgerType, OrgHeader billTo)
		{
			AccTransactionHeader aRInvoice = GetTransactionHeader(jobPK, amount, reference, ledgerType, billTo, "INV");
			aRInvoice.AH_GE = dept;

			return aRInvoice;
		}
		#endregion

		#region GetTransactionLines

		public AccTransactionLines GetTransactionLines(ZGuid headerPK, ZGuid jobPK, ZString transType, ZDecimal amount, GlbBranch brh, GlbDepartment dept, Guid chargePK)
		{
			AccTransactionLines result = Factory.New<AccTransactionLines>();
			result.AL_LineType = transType;
			result.AL_AH = headerPK;
			result.AL_JH = jobPK;
			result.AL_PostDate = ZDateTime.Now;
			result.AL_LineAmount = amount;
			if (transType == TransactionLineTypes.Revenue || transType == TransactionLineTypes.Cost)
			{
				result.AL_ReverseDate = ZDateTime.Now;

				JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
				charge.JR_JH = jobPK;
				if (transType == TransactionLineTypes.Revenue)
				{
					charge.JR_AL_ARLine = result.PK;
				}
				else
				{
					charge.JR_AL_APLine = result.PK;
				}
				charge.SetAmountsFromLinkedLinesForTests();
			}
			result.AL_GB = (brh == null) ? Guid.Empty : brh.PK;
			result.AL_GE = dept.PK;
			result.AL_AC = chargePK;
			result.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;

			return result;
		}

		#endregion

		#region SetupConsol

		public ForwardingConsol SetupConsol(ZString transportMode, ZString packingMode, ZString agentType, ZString load, ZString disch,
									ZDateTime eTD, ZDateTime eTA)
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = transportMode;
			consol.JK_ConsolMode = packingMode;
			consol.JK_AgentType = agentType;
			consol.JK_RL_NKDischargePort = load;
			consol.JK_RL_NKLoadPort = disch;

			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = load;
			transport.JW_RL_NKDiscPort = disch;
			transport.JW_ETD = eTD;
			transport.JW_ETA = eTA;
			transport.JW_TransportType = transportMode;

			Factory.Save();

			return consol;
		}

		#endregion

		#region GetJobHeader

		public Job GetJobHeader(ZGuid foreignKey, ZString tableName, ZString jobNumber, OrgHeader billTo,
			GlbStaff op, GlbStaff salesRep)
		{
			Job result = Factory.NewJobForTesting<Job>();
			result.JH_ParentTableCode = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(tableName);
			result.LocalChargesPK = billTo.PK;
			result.JH_ParentID = foreignKey;
			result.JH_JobNum = jobNumber;
			result.JH_GE = GlbDepartment.CurrentDepartment.PK;
			result.JH_GS_NKRepSales = (salesRep == null) ? ZString.Empty : salesRep.GS_Code;
			result.JH_GS_NKRepOps = op.GS_Code;
			return result;
		}

		#endregion

		#region GetCharge

		public Charge GetCharge(Job jobHeader1, ZDecimal amount)
		{
			Charge result = jobHeader1.Charges.AddNew();
			result.JR_AC = Env.Registry.FreightChargeCode;
			result.JR_LocalCostAmt = amount;
			return result;
		}

		#endregion

		readonly BusinessObjectFactory Factory;
	}
}
