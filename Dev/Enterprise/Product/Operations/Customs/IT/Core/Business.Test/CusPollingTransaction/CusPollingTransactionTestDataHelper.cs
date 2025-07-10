using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

public sealed class CusPollingTransactionTestDataHelper(BusinessObjectFactory factory)
{
	public EDIInterchange CreateIvistoRequestInterchange()
	{
		var interchange = factory.New<EDIInterchange>();
		interchange.EI_ApplicationCode = "ITH";
		interchange.EI_InterchangeType = "IVI";
		interchange.IsTransmitInterchange = true;
		interchange.EI_SessionGUID = ZGuid.NewZGuid();
		interchange.EI_Status = "SNT";
		interchange.NumberStrategy = new FixedMessageNumberStrategy(ZGuid.NewZGuid().ToString());
		return interchange;
	}

	public EDIInterchange CreateIrildesRequestInterchange()
	{
		var interchange = factory.New<EDIInterchange>();
		interchange.EI_ApplicationCode = "ITH";
		interchange.EI_InterchangeType = "IRI";
		interchange.IsTransmitInterchange = true;
		interchange.EI_SessionGUID = ZGuid.NewZGuid();
		interchange.EI_Status = "SNT";
		interchange.NumberStrategy = new FixedMessageNumberStrategy(ZGuid.NewZGuid().ToString());
		return interchange;
	}

	public CusPollingTransaction CreateIvistoPollingTransaction(ZString transactionStatus, ZDateTime earliestTimeOfNextAttemptUtc, EDIInterchange parentInterchage, ZDateTime systemCreateTimeUtc)
		=> CreatePollingTransaction(transactionStatus, earliestTimeOfNextAttemptUtc, parentInterchage, "IVI", systemCreateTimeUtc);

	public CusPollingTransaction CreateIrildesPollingTransaction(ZString transactionStatus, ZDateTime earliestTimeOfNextAttemptUtc, EDIInterchange parentInterchage, ZDateTime systemCreateTimeUtc)
		=> CreatePollingTransaction(transactionStatus, earliestTimeOfNextAttemptUtc, parentInterchage, "IRI", systemCreateTimeUtc);

	CusPollingTransaction CreatePollingTransaction(ZString transactionStatus, ZDateTime earliestTimeOfNextAttemptUtc, EDIInterchange parentInterchage, ZString transactionType, ZDateTime systemCreateTimeUtc)
	{
		var transaction = factory.New<CusPollingTransaction>();
		transaction.CPT_ApplicationCode = "ITH";
		transaction.CPT_Status = transactionStatus;
		transaction.CPT_EarliestTimeOfNextAttemptUtc = earliestTimeOfNextAttemptUtc;
		transaction.CPT_ParentID = parentInterchage.PK;
		transaction.CPT_ParentTableCode = parentInterchage.TablePrefix;
		transaction.CPT_Type = transactionType;
		transaction.CPT_SystemCreateTimeUtc = systemCreateTimeUtc;
		transaction.CPT_TransactionID = parentInterchage.EI_SessionGUID.ToString();
		return transaction;
	}
}
