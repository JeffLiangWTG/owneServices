using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.Business.Testing;

public abstract class SingleWindowCustomsLinkedObjectAdapterTest<TAdaptee> : CustomsLinkedObjectAdapterAbstractTest<TAdaptee, ISingleWindowCustomsLinkedObjectAdapter>
	where TAdaptee : BusinessObject
{
	public abstract void TestDocManagerInfo();
	public abstract void TestAddLog();
	public abstract void TestSetEntryCustomsChannel();
	public abstract void TestSetEntryAsCleared();
	public abstract void TestInsertOrUpdateReleaseCode();
	public abstract void TestIsEntryCleared();
}
