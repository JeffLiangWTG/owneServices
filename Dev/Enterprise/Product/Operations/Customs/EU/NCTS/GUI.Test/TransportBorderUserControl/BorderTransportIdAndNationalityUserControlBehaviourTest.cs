using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class BorderTransportIdAndNationalityUserControlBehaviourTest : TestCaseWithFactory
	{
		public void TestCharacterCasingBehaviour()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var departureMovement = header.MovementHeader;
			departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._2_RailTransport;

			using (var borderTransportIdAndNationalityUserControl = new BorderTransportIdAndNationalityUserControl())
			{
				var behaviour = new BorderTransportIdAndNationalityUserControlBehaviour();

				using (var ruleTestContext = Business.Testing.ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
				{
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleR0076Active));
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleB1811Active));

					CombineAssertions("When R0076 is enabled", () =>
					{
						departureMovement.BM_ActiveBorderIdentificationType = "";
						behaviour.UpdateBehaviour(borderTransportIdAndNationalityUserControl, departureMovement);
						AssertEquals("When BorderTransportTypeOfId is empty, ZTextBox has normal character casing",
							CharacterCasing.Normal,
							borderTransportIdAndNationalityUserControl.BorderTransportIdTextBox.CharacterCasing);

						departureMovement.BM_ActiveBorderIdentificationType = "10";
						behaviour.UpdateBehaviour(borderTransportIdAndNationalityUserControl, departureMovement);
						AssertEquals("When BorderTransportTypeOfId is 10, ZTextBox has normal character casing",
							CharacterCasing.Upper,
							borderTransportIdAndNationalityUserControl.BorderTransportIdTextBox.CharacterCasing);
					});
				}
			}
		}

		public void TestVisiblityAndCaptionBehaviour()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var departureMovement = header.MovementHeader;

			using (var borderTransportIdAndNationalityUserControl = new BorderTransportIdAndNationalityUserControl())
			{
				borderTransportIdAndNationalityUserControl.SetDataBinding(departureMovement, "");
				var behaviour = new BorderTransportIdAndNationalityUserControlBehaviour();

				CombineAssertions(() =>
				{
					departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
					departureMovement.BM_ActiveBorderIdentificationType = "10";
					behaviour.UpdateBehaviour(borderTransportIdAndNationalityUserControl, departureMovement);
					AssertEquals("BorderTransportIdTextBox is not Visible", false, borderTransportIdAndNationalityUserControl.BorderTransportIdTextBox.Visible);
					AssertEquals("VesselCodeFindBox is Visible", true, borderTransportIdAndNationalityUserControl.VesselCodeFindBox.Visible);
					AssertEquals("Caption", "Lloyds Number", borderTransportIdAndNationalityUserControl.VesselCodeFindBox.Extensions.Get<ILabelCaptionRenderer>().Caption);

					departureMovement.BM_ActiveBorderIdentificationType = "11";
					behaviour.UpdateBehaviour(borderTransportIdAndNationalityUserControl, departureMovement);
					AssertEquals("Caption", "Vessel Name", borderTransportIdAndNationalityUserControl.VesselCodeFindBox.Extensions.Get<ILabelCaptionRenderer>().Caption);

					departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._2_RailTransport;
					departureMovement.BM_ActiveBorderIdentificationType = "20";
					behaviour.UpdateBehaviour(borderTransportIdAndNationalityUserControl, departureMovement);
					AssertEquals("BorderTransportIdTextBox is Visible", true, borderTransportIdAndNationalityUserControl.BorderTransportIdTextBox.Visible);
					AssertEquals("VesselCodeFindBox is not Visible", false, borderTransportIdAndNationalityUserControl.VesselCodeFindBox.Visible);
					AssertEquals("Caption", "Wagon Number", borderTransportIdAndNationalityUserControl.BorderTransportIdTextBox.Extensions.Get<ILabelCaptionRenderer>().Caption);

					departureMovement.BM_ActiveBorderIdentificationType = "21";
					behaviour.UpdateBehaviour(borderTransportIdAndNationalityUserControl, departureMovement);
					AssertEquals("Caption", "Train Number", borderTransportIdAndNationalityUserControl.BorderTransportIdTextBox.Extensions.Get<ILabelCaptionRenderer>().Caption);

					departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._4_AirTransport;
					departureMovement.BM_ActiveBorderIdentificationType = "40";
					behaviour.UpdateBehaviour(borderTransportIdAndNationalityUserControl, departureMovement);
					AssertEquals("Caption", "Flight Number", borderTransportIdAndNationalityUserControl.BorderTransportIdTextBox.Extensions.Get<ILabelCaptionRenderer>().Caption);

					departureMovement.BM_ActiveBorderIdentificationType = "41";
					behaviour.UpdateBehaviour(borderTransportIdAndNationalityUserControl, departureMovement);
					AssertEquals("Caption", "Registration Number", borderTransportIdAndNationalityUserControl.BorderTransportIdTextBox.Extensions.Get<ILabelCaptionRenderer>().Caption);

					departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
					departureMovement.BM_ActiveBorderIdentificationType = "80";
					behaviour.UpdateBehaviour(borderTransportIdAndNationalityUserControl, departureMovement);
					AssertEquals("Caption", "ENI Code", borderTransportIdAndNationalityUserControl.BorderTransportIdTextBox.Extensions.Get<ILabelCaptionRenderer>().Caption);

					departureMovement.BM_ActiveBorderIdentificationType = "81";
					behaviour.UpdateBehaviour(borderTransportIdAndNationalityUserControl, departureMovement);
					AssertEquals("Caption", "Vessel Name", borderTransportIdAndNationalityUserControl.BorderTransportIdTextBox.Extensions.Get<ILabelCaptionRenderer>().Caption);
				});
			}
		}
	}
}
