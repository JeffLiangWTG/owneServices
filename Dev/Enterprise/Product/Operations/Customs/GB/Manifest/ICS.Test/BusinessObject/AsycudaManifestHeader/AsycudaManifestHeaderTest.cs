using System;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using AsycudaContainer = Enterprise.Customs.EU.Manifest.Business.AsycudaContainer;

namespace Enterprise.Customs.GB.ICS.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeader))]
	public class AsycudaManifestHeaderTest : AsycudaManifestHeaderBaseTest
	{
		protected override AsycudaManifestHeaderBase GetManifestHeader() => Factory.NewWithValidTestData<AsycudaManifestHeader>();

		public void TestHumanReadableNamePrefix()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			AssertEquals(Constants.MessageSubTypePreFixes.ICS, header.HumanReadableNamePrefix);
		}

		public void TestGetExtraMessageSendingNotification()
		{
			var header = GetManifestHeader();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				AssertEquals("Incorrect Current country message", "To create a message for UK you must be logged-in under a UK company.", header.MessageSendingNotificationHelper.GetNotifications());
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				using (GBCustomsDataRegistry.Instance.ICSUsername.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ""))
				using (GBCustomsDataRegistry.Instance.ICSPasssword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ""))
				{
					AssertEquals("Empty ICSUsername/ICSPasssword message", "ICS Username and Password must be entered. \r\nSee System -> Registry -> Customs -> Country or Region Specific -> United Kingdom -> ICS.", header.MessageSendingNotificationHelper.GetNotifications());
				}

				using (GBCustomsDataRegistry.Instance.ICSUsername.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "ICSUsername"))
				using (GBCustomsDataRegistry.Instance.ICSPasssword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "ICSPasssword"))
				{
					AssertEquals("Empty EORI message", "EORI must exist under the currently logged in branch's Organization Proxy. \r\nConfig code: EOR.", header.MessageSendingNotificationHelper.GetNotifications());
				}
			}
		}

		protected override Type ExpectedTypeOfContainer => typeof(AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeaderBase>);
	}
}
