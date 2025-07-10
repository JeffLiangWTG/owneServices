using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Billing;
using Enterprise.Registry.Business;
using Moq;

namespace Enterprise.Billing.StlCollector.Retriever.Testing.Scripts
{
	public class HighWaterMarkTest : TestCaseWithFactory
	{
		public void TestHighWaterMark()
		{
			var dummyScriptToTest = MockScript("test", "test");
			var startWaterMark = new DateTime(2009, 5, 22);
			var script = dummyScriptToTest;
			SystemDataRegistry.Instance.GetStlCollectorHighWaterMark(script.Code, script.Feature).SetValue(Guid.Empty, Guid.Empty, Guid.Empty, startWaterMark);
			var scriptLoader = new ScriptLoader(new ScriptFactoryForTest(script));
			var scriptWithConfig = scriptLoader.Load(Factory).Single();
			AssertEquals("HighWaterMark Initialisation", startWaterMark, scriptWithConfig.HighWaterMarkSettings.HighWaterMark);
			var newWaterMark = startWaterMark.AddDays(1);
			scriptWithConfig.HighWaterMarkSettings.HighWaterMark = newWaterMark;
			Factory.Save();
			AssertEquals("HighWaterMark updated", newWaterMark, SystemDataRegistry.Instance.GetStlCollectorHighWaterMark(script.Code, script.Feature).Value);
		}

		IStlScript MockScript(string code, string feature)
		{
			var script = new Mock<IStlScript>();
			script.Setup(script => script.Code).Returns(code);
			script.Setup(script => script.Feature).Returns(feature);
			return script.Object;
		}
	}
}
