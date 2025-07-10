using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.Business.Testing;

public abstract class XmlCustomsLinkedObjectAdapterTest<TAdaptee> : CustomsLinkedObjectAdapterAbstractTest<TAdaptee, IXmlCustomsLinkedObjectAdapter>
	where TAdaptee : BusinessObject
{
	public abstract void TestGetAllRelatedEntryNumbers();

	public abstract void TestCustomsOfficeOfPresentation();

	public abstract void TestGetAllEntryLines();

	public abstract void TestIsAwaitingMessage();

	public abstract void TestIsDeposited();

	public abstract void TestSetStatusAsError();

	public abstract void TestSetStatusAsAcknowledged();

	public abstract void TestSetStatusAsFailedForTransmission();

	public abstract void TestSetStatusAsRegistered();

	public abstract void TestSetStatusAsCleared();

	public abstract void TestSetStatusAsCancelled();

	public abstract void TestSetStatusAsDeposited();

	public abstract void TestSetStatusAsUnderControl();

	public abstract void TestSetStatusAsExitCompleted();

	public abstract void TestSetStatusAsGoodsWrittenOffClosed();

	public abstract void TestGetAllPaymentInfo();

	public abstract void TestSetStatusAsAmended();

	public abstract void TestUpdateOrInsertEntryNumber();

	public abstract void TestAddA93Number();

	public abstract void TestGetAllFees();

	public abstract void TestGetFeesForLine();

	public abstract void TestSetCustomsChannel();

	public abstract void TestGetUniqueTransactionIdentifierRequestContext();

	public abstract void TestGetLastSuccessfullySentMessageForDepositedStatus();

	public abstract void TestSetStatusAsAcceptedBySystem();

	public abstract void TestUpdateOrInsertIvistoEntryNumber();

	public abstract void TestUpdateOrInsertIrildesEntryNumber();

	public abstract void TestGetOriginalSentMessageByUniqueTransactionIdentifier();

	public abstract void TestAddEDoc();

	public abstract void TestCustomsProfile();

	public abstract void TestCreateIvistoRequestMessage();

	public abstract void TestCreateIrildesRequestMessage();
}
