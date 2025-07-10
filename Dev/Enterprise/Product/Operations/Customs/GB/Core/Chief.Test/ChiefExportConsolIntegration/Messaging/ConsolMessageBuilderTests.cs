using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Registry;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.Testing
{
	sealed class ConsolMessageBuilderTests : TestCaseWithFactory
	{
		public void TestPopulateMessages()
		{
			wrapper.MawbExportHelper.ME_Profile = "ZPE";

			var builder = new ConsolMessageBuilder(wrapper, new GbDes242MessageFunction.MucrClose());
			var messageBuilderResult = builder.PopulateMessages();
			var builderResult = messageBuilderResult.GetBuilderResults().SingleOrDefault();

			AssertNotNull("BuilderResult", builderResult);
			AssertEquals("BuilderResult.Errors.Length", 0, builderResult.Errors.Length);
		}

		public void TestPopulateMessages_WarningOnProfile()
		{
			using (Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.CDS_ILE_PHASE2, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Now, value: true))
			{
				wrapper.MawbExportHelper.ME_Profile = "ZPE";
				AssertHasWarningContaining("Pre-requisite: Profile should have a warning", wrapper.MawbExportHelper.ME_ProfileInfo, "CDS");

				var builder = new ConsolMessageBuilder(wrapper, new GbDes242MessageFunction.MucrClose());
				var messageBuilderResult = builder.PopulateMessages();
				var builderResult = messageBuilderResult.GetBuilderResults().SingleOrDefault();

				AssertNotNull("BuilderResult", builderResult);
				AssertEquals("BuilderResult.Errors.Length", 0, builderResult.Errors.Length);
			}
		}

		public void TestPopulateMessages_NoProfile()
		{
			wrapper.MawbExportHelper.ME_Profile = ZString.Empty;

			var builder = new ConsolMessageBuilder(wrapper, new GbDes242MessageFunction.MucrClose());
			var messageBuilderResult = builder.PopulateMessages();
			var builderResult = messageBuilderResult.GetBuilderResults().SingleOrDefault();

			AssertEquals("BuilderResult.Errors.Length", 1, builderResult.Errors.Length);
			AssertContains("Profile", builderResult.Errors[0]);
		}

		protected override void SetUp()
		{
			base.SetUp();
			MawbTestHelper.MakeBadge("ZPE", GatewayList.Codes.CCSUKviaNTMsgGW, "CUKFFW98000", true, "", false, false, BadgeDirectionList.Codes.EXP, "GBLHR");

			consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "GBLHR";
			consol.JK_RL_NKDischargePort = "AUSYD";

			wrapper = new CustomsExportConsolIntegrationWrapper(consol, new SendsMessagesToCustomsShutterUpperer());
			wrapper.MawbExportHelper.ME_MasterUCR = "X";
		}

		ForwardingConsol consol;
		CustomsExportConsolIntegrationWrapper wrapper;
	}
}
