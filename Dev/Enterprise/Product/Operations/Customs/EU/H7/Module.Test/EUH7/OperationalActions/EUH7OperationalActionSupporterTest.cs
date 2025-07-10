using CargoWise.Definitions;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Environment;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Module.Testing;

[TestedType(typeof(EUH7OperationalActionSupporter))]
public class EUH7OperationalActionSupporterTest : OperationalActionSupporterTest<EUH7OperationalActionSupporter>
{
	public void TestEUH7OperationalActionSupporter_OverridenProperties()
	{
		var supporter = new EUH7OperationalActionSupporter();
		CombineAssertions("Test overriden properties", () =>
		{
			AssertNotNull(supporter.RootType);
			Assert(supporter.RootType == typeof(AsycudaManifestHeader));
			Assert(supporter.BusinessContext == BusinessContext.AsycudaManifest);
			Assert(supporter.BaseCheckpoint == Env.Security.EuH7);
		});
	}

	public void TestElementNoun()
	{
		var supporter = new EUH7OperationalActionSupporter();
		CombineAssertions("Test element noun", () =>
		{
			AssertEquals("job", supporter.SingularElementNoun);
			AssertEquals("jobs", supporter.PluralElementNoun);
		});
	}

	protected override ModuleIdentifier ModuleID => ModuleIDs.Customs.EU.EUH7;
}
