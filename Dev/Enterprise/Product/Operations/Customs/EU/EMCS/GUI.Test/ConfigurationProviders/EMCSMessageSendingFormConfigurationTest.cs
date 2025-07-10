using Enterprise.Customs.EU.EMCS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.GUI.Testing
{
	[TestedType(typeof(EMCSMessageSendingFormConfiguration))]
	sealed class EMCSMessageSendingFormConfigurationTest : EMCSMessageSendingFormConfigurationAbstractTest<EMCSMessageSendingFormConfiguration>
	{
		public override void TestIsOKToSend()
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			var sendingActionParent = new MinimalSendingActionParent(declaration);
			AssertEquals(true, Provider.IsOKToSend(sendingActionParent));
		}

		protected override string CountryOrGroupingCode => Core.Constants.CountryCodes.Latvia;
	}
}
