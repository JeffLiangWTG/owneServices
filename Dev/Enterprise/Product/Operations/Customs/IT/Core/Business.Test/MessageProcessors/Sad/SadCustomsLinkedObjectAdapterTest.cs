using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.Business.Testing;

public abstract class SadCustomsLinkedObjectAdapterTest<TAdaptee> : CustomsLinkedObjectAdapterAbstractTest<TAdaptee, ISadCustomsLinkedObjectAdapter>
	where TAdaptee : BusinessObject
{
	public abstract void TestFactory();
	public abstract void TestMessageStatus();
	public abstract void TestEntryCustomsStatus();
	public abstract void TestStatusProvider();
	public abstract void TestIsImport();
	public abstract void TestIsExport();
	public abstract void TestIsEntryRegisteredOrNbRejected();
	public abstract void TestCustomsLines();
	public abstract void TestSingleWindowRequestDataProvider();
	public abstract void TestMrn();
	public abstract void TestIrildesCusEntryNum();
	public abstract void TestIvistoCusEntryNum();
	public abstract void TestGetEntryNumbers();
	public abstract void TestGetNewCusEntryNumber();
	public abstract void TestInsertOrUpdateA93Numbers();
	public abstract void TestIsIncomingMessageAlreadyLinked();
	public abstract void TestSetEntryCustomsStatus();
	public abstract void TestSetMessageStatus();
	public abstract void TestSetEntryReleaseDate();
	public abstract void TestUpdatePendingGuaranteeTransactions();
	public abstract void TestWriteOffGuarantee();
}
