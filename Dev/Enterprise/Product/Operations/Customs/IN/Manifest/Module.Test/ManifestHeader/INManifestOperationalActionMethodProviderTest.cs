using System.Linq;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Module.Testing;

[TestedType(typeof(INManifestOperationalActionMethodProvider))]
sealed class INManifestOperationalActionMethodProviderTest : OperationalActionMethodProviderTest
{
	public void TestNewMethods()
	{
		var methods = Provider.NewMethods(null);
		AssertType<MessageSendingOperationalActionMethod>(methods.Single());
	}

	protected override ActionMethodProviderID ID => ActionMethodProviderIDs.INManifest;
}
