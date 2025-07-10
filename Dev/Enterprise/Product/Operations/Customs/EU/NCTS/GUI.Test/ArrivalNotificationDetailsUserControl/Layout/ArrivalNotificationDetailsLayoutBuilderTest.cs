using System.Windows.Forms;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(ArrivalNotificationDetailsLayoutBuilder<NctsHeader>))]
	class ArrivalNotificationDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<ArrivalNotificationDetailsLayoutBuilder<NctsHeader>, NctsHeader, ArrivalNotificationDetailsControlBag>
	{
		protected override int ExpectedMaxColumns => 2;

		protected override ArrivalNotificationDetailsLayoutBuilder<NctsHeader> GetColumnLayoutBuilderForTesting() => new ArrivalNotificationDetailsLayoutBuilder<NctsHeader>();

		public void TestOverrideFreightDetailsCheckBoxVisibility()
		{
			CombineAssertions(() =>
			{
				var layout = ((IPanelLayoutProvider)new Phase5ArrivalNotificationDetailsLayout()).Layout;
				AssertEquals("IsPluggedIn: false", false, layout.IsVisible(ArrivalNotificationDetailsControlBag.Instance.OverrideFreightDetailsCheckBox, nctsHeader));

				var shipment = Factory.New<ForwardingShipment>();
				nctsHeader.BH_ParentID = shipment.PK;
				nctsHeader.BH_ParentTableCode = shipment.TablePrefix;
				AssertEquals("IsPluggedIn - shipment: true", true, layout.IsVisible(ArrivalNotificationDetailsControlBag.Instance.OverrideFreightDetailsCheckBox, nctsHeader));

				var consol = Factory.New<ForwardingConsol>();
				nctsHeader.BH_ParentID = consol.PK;
				nctsHeader.BH_ParentTableCode = consol.TablePrefix;
				AssertEquals("IsPluggedIn - consol: true", true, layout.IsVisible(ArrivalNotificationDetailsControlBag.Instance.OverrideFreightDetailsCheckBox, nctsHeader));
			});
		}

		public void TestAuthorisationNumberCasingBehaviour()
		{
			CombineAssertions(() =>
			{
				var layout = ((IPanelLayoutProvider)new Phase5ArrivalNotificationDetailsLayout()).Layout;
				var behaviour = layout.GetControlBehaviour<ArrivalAuthorisationNumberCasingBehaviour>(ArrivalNotificationDetailsControlBag.Instance.NumberCodeFindBox);
				AssertNotNull(behaviour);

				using (NctsConfigurationTestHelper.TemporarilySetNctsHeaderConfigurationAllowMixedCaseAuthorisationNumbers(Factory, false))
				using (var codeFindBox = new ZCodeFindBox())
				{
					behaviour.UpdateBehaviour(codeFindBox, nctsHeader);
					AssertEquals("Authorisation Number - Upper Case", CharacterCasing.Upper, codeFindBox.CodeBox.CharacterCasing);
				}

				using (NctsConfigurationTestHelper.TemporarilySetNctsHeaderConfigurationAllowMixedCaseAuthorisationNumbers(Factory, true))
				using (var codeFindBox = new ZCodeFindBox())
				{
					behaviour.UpdateBehaviour(codeFindBox, nctsHeader);
					AssertEquals("Authorisation Number - Normal Case", CharacterCasing.Normal, codeFindBox.CodeBox.CharacterCasing);
				}
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		}
		NctsHeader nctsHeader;
	}
}
