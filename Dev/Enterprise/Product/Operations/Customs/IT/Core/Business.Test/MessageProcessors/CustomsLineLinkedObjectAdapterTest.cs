using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

public abstract class CustomsLineLinkedObjectAdapterTest<TAdaptee> : CustomsLineLinkedObjectAdapterTestBase<TAdaptee, ISadCustomsLineLinkedObjectAdapter>
	where TAdaptee : BusinessObject
{
	public abstract void TestLineNo();
	public abstract void TestNBStatus();
	public abstract void TestSetNBStatus();
}

public abstract class CustomsLineLinkedObjectAdapterTestBase<TAdaptee, TAdapter> : TestCaseWithFactory
	where TAdaptee : BusinessObject
	where TAdapter : ISadCustomsLineLinkedObjectAdapter
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
