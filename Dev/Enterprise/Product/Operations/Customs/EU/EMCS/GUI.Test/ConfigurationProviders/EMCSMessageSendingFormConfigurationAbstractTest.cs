using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.GUI.Testing
{
	[TestsSubclassesOf(typeof(IEMCSMessageSendingFormConfiguration))]
	public abstract class EMCSMessageSendingFormConfigurationAbstractTest<T> : TestCaseWithFactory where T : IEMCSMessageSendingFormConfiguration
	{
		public abstract void TestIsOKToSend();

		public void TestEMCSTopeMenuProviderType()
		{
			AssertType<T>(provider);
		}

		protected abstract string CountryOrGroupingCode { get; }

		protected override void SetUp()
		{
			base.SetUp();
			provider = EMCSMessageSendingFormConfiguration.GetConfiguration(CountryOrGroupingCode);
		}

		protected IEMCSMessageSendingFormConfiguration Provider => provider;
		IEMCSMessageSendingFormConfiguration provider;
	}
}
