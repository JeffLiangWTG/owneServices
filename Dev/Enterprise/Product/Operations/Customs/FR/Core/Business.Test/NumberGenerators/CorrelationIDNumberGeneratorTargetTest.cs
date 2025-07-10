using System;
using Enterprise.Customs.FR.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.FR.Business.Testing
{
	public class CorrelationIDNumberGeneratorTargetTest : NumberGeneratorTargetTest
	{
		public void TestOverrides()
		{
			var customisation = new CorrelationIDCustomisation();
			FRCustomsDataRegistry.Instance.CorrelationIDCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customisation);

			var target = new CorrelationIDNumberGeneratorTarget { Context = new NumberGeneratorContext() };
			AssertCustomisation("Should find the customisation", "", target.NumberCustomisation);
			AssertLocation(FRCustomsDataRegistry.Instance.CorrelationIDCustomisation, target.NumberCustomisationLocation);
			AssertEquals(10, target.MaxLength);
			AssertEquals("CorellationID", target.Name);
		}
	}
}
