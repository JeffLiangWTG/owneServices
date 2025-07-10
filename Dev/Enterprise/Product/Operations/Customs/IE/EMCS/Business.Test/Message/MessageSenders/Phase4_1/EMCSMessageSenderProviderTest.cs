using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.EMCS.Business.Testing.Phase4_1
{
	sealed class EMCSMessageSenderProviderTest : EMCSMessageSenderProviderAbstractTest
	{
		protected override string ResourcePathIE819Sample => "Enterprise.Customs.IE.EMCS.Business.Testing.Message.MessageSenders.Phase4_1.TestFiles.IE819MessageSample.txt";

		protected override string MessageTextVersion => "V3.13";

		protected override void SetUp()
		{
			base.SetUp();
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.EMCS_Phase4_1, Core.Constants.CountryCodes.Ireland, ZDateTime.Today, true);
			var systemCode = GlbCompany.CurrentCompany.LicenceEnterpriseCode + GlbCompany.CurrentCompany.LicenceServerID;
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionalityAttribute(Constants.FunctionalityTypes.EMCS_Phase4_1,
				Core.Constants.CountryCodes.Ireland,
				ZDateTime.Today,
				Core.Constants.Customs.Universal.RefCusCodeList.Attributes.System,
				systemCode);
		}

		protected override void TearDown()
		{
			base.TearDown();
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.EMCS_Phase4_1, Core.Constants.CountryCodes.Ireland, ZDateTime.Today, false);
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionalityAttribute(Constants.FunctionalityTypes.EMCS_Phase4_1,
				Core.Constants.CountryCodes.Ireland,
				ZDateTime.Today,
				Core.Constants.Customs.Universal.RefCusCodeList.Attributes.System,
				string.Empty);
		}
	}
}
