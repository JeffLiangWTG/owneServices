using CargoWise.Data.Testing;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Module.Testing;

[TestedType(typeof(EUH7ActionMethodProvider))]
[UseSnapshotProtection]
public class EUH7ActionMethodProviderTest : OperationalActionMethodProviderTest
{
	public void TestEUH7ActionMethodProvider_NewMethods()
	{
		var provider = new EUH7ActionMethodProvider();
		var operationalActionMethods = provider.NewMethods(new EUH7OperationalActionSupporter());
		AssertNotNull(operationalActionMethods);
	}

	protected override ActionMethodProviderID ID => ActionMethodProviderIDs.EUH7;
}
