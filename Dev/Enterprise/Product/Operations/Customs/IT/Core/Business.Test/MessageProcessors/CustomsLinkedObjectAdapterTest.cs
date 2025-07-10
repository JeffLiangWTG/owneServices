using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

public abstract class CustomsLinkedObjectAdapterTest<TAdaptee> : CustomsLinkedObjectAdapterAbstractTest<TAdaptee, ICustomsLinkedObjectAdapter>
	where TAdaptee : BusinessObject
{
	public void TestPK()
	{
		AssertEquals(Adaptee.PK, Adapter.PK);
	}

	public abstract void TestAddMessage();
	public abstract void TestGetLastSuccessfullySentMessage();
	public abstract void TestEntryReferenceNumber();
	public abstract void TestJobReferenceNumber();
	public abstract void TestFactory();
	public abstract void TestCustomsProfile();
}

public abstract class CustomsLinkedObjectAdapterAbstractTest<TAdaptee, TAdapter> : TestCaseWithFactory
	where TAdaptee : BusinessObject
	where TAdapter : ICustomsLinkedObjectAdapter
{
	protected abstract TAdaptee GetAdaptee();

	protected TAdaptee Adaptee => adaptee ?? (adaptee = GetAdaptee());
	TAdaptee adaptee;

	protected abstract TAdapter GetAdapter();

	protected TAdapter Adapter
	{
		get
		{
			if (adapter == null)
			{
				adapter = GetAdapter();
			}
			return adapter;
		}
	}
	TAdapter adapter;
}
