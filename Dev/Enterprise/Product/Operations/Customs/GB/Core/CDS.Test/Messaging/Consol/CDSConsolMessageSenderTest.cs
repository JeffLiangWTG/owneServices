using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.GB.CDS.Messaging.Testing
{
	sealed class CDSConsolMessageSenderTest : TestCaseWithFactory
	{
		public void TestGetCodesOfRelevantServiceTasksThatShouldBeCheckedBeforeSending_CDS()
		{
			var wrapper = new CustomsExportConsolIntegrationWrapper(consol, null);
			wrapper.MawbExportHelper.ME_Profile = $"GB123456789.CDS";
			var sender = new CDSConsolMessageSender(wrapper);

			AssertArrayEqualsByElements(new[] { "EHO", "EHI", "CDS", "CDR" }, sender.GetCodesOfRelevantServiceTasksThatShouldBeCheckedBeforeSending());
		}

		public void TestGetCodesOfRelevantServiceTasksThatShouldBeCheckedBeforeSending_CCSUK()
		{
			var wrapper = new CustomsExportConsolIntegrationWrapper(consol, null);
			wrapper.MawbExportHelper.ME_Profile = $"GB123456789.CSK";
			var sender = new CDSConsolMessageSender(wrapper);

			AssertArrayEqualsByElements(new[] { "CUK", "GCI", "CDR" }, sender.GetCodesOfRelevantServiceTasksThatShouldBeCheckedBeforeSending());
		}

		protected override void SetUp()
		{
			base.SetUp();

			consol = Factory.New<ForwardingConsol>();

			CreateBadge("CDS");
			CreateBadge("CSK");

			var settingCollection = new BadgeCodeSettingCollection();
			settingCollection.Add(new BadgeCodeSetting(Factory) { BadgeCode = "CDS", CSPCode = "CDS", ApplicationCode = "CDS" });
			settingCollection.Add(new BadgeCodeSetting(Factory) { BadgeCode = "CSK", CSPCode = "CCSUK", ApplicationCode = "CDS" });

			registrySetting = GBCustomsDataRegistry.Instance.BadgeCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, settingCollection);
		}

		protected override void TearDown()
		{
			registrySetting?.Dispose();
			base.TearDown();
		}

		void CreateBadge(string badgeCode)
		{
			var extPass = Factory.New<GlbExternalPassword_GB>();
			extPass.Badge = badgeCode;
			extPass.EORI = "GB123456789";
		}

		ForwardingConsol consol;
		IDisposable registrySetting;
	}
}
