using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class ArrivalReportBuilderAbstractTest : TestCaseWithFactory
	{
		public void TestDocumentNameCode()
		{
			AssertEquals("DocumentNameCode", Edifact.D99B.Elements.DocumentNameCodeList.ArrivalInformation, Builder.DocumentNameCode);
		}

		public void TestResponsiblePartyClientID()
		{
			Assert("ResponsiblePartyClientID Included", GeneratedMessage.Contains("RFF+ABP:41065894724'"));
		}

		public void TestPortOfArrival()
		{
			Assert("PortOfArrival", GeneratedMessage.Contains("LOC+60+AUSYD::6'"));
		}

		public abstract void TestTransportDetails();

		public abstract void TestDocumentName();

		protected abstract ZString DateTimeCodeQualifier { get; }

		protected abstract ArrivalReportBuilder Builder { get; }

		protected ZString GeneratedMessage => Builder.GeneratedMessageStrings[0];

		protected override void SetUp()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			base.SetUp();
		}

		protected Common.MessageBuilders.MessageSubTypes messageSubType = Common.MessageBuilders.MessageSubTypes.Create;
	}
}
