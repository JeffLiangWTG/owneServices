using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Module.Testing;

[TestedType(typeof(MessageSendingOperationalActionMethod))]
sealed class MessageSendingOperationalActionMethodTest : OperationalActionMethodTest<MessageSendingOperationalActionMethod>
{
	public void TestProperties()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Name", "Send EDI message", Method.Name);
			AssertEquals("Description", "Sign and Send EDI message", Method.Description);
		});
	}

	public void TestMethodID()
	{
		AssertEquals(new ZGuid("119894D9-1EBA-4115-8F1D-95898F42B515"), Method.MethodID);
	}

	public void TestNewApplicatorType()
	{
		AssertType<MessageSendingOperationalActionMethodApplicator>(Method.NewApplicator(null, null));
	}

	public void TestGetFilterRequirements()
	{
		var filterRequirements = Method.GetFilterRequirements();
		AssertContainsExactElementsInAnyOrder(
			"Consol General Manifest enabled for filter constraints",
			new List<string>
			{
				$"{new FilterIsConsolGeneralManifestEnabledRegistryConstraint().Name}:Y"
			},
			filterRequirements.Select(s => $"{s.ConstraintName}:{string.Join(",", s.Select(v => v))}"));
	}

	protected override MessageSendingOperationalActionMethod NewMethod() => new MessageSendingOperationalActionMethod();
}
