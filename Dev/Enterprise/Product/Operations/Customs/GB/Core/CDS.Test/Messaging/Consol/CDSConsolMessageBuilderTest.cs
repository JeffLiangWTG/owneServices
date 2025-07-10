using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.CDS.Messaging;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;
using Enterprise.Freight.Forwarding.Business;
using static Enterprise.Customs.GB.Business.GbDes242MessageFunction;

namespace Enterprise.Customs.GB.CDS.Testing.Messaging
{
	sealed class CDSConsolMessageBuilderTest : TestCaseWithFactory
	{
		public void TestGetApplicationCode()
		{
			var consol = Factory.New<ForwardingConsol>();
			var wrapper = new CustomsExportConsolIntegrationWrapperForTest(consol);
			var builder = new CDSConsolMessageBuilderForTest(wrapper);

			CombineAssertions(() =>
			{
				wrapper.IsProfileForCCSUKForTest = false;
				AssertEquals("CDS - CDS", "CDS", builder.ApplicationCode);

				wrapper.IsProfileForCCSUKForTest = true;
				AssertEquals("CCSUK - CVC", "CVC", builder.ApplicationCode);
			});
		}

		class CDSConsolMessageBuilderForTest : CDSConsolMessageBuilder
		{
			public CDSConsolMessageBuilderForTest(CustomsExportConsolIntegrationWrapper consolWrapper) : base(consolWrapper, new MucrAssociate())
			{
			}

			public ZString ApplicationCode => GetApplicationCode();
		}

		class CustomsExportConsolIntegrationWrapperForTest : CustomsExportConsolIntegrationWrapper
		{
			public CustomsExportConsolIntegrationWrapperForTest(ForwardingConsol consol) : base(consol, null)
			{
			}

			public bool IsProfileForCCSUKForTest { get; set; }
			protected override bool IsProfileForCCSUKCore => IsProfileForCCSUKForTest;
		}
	}
}
