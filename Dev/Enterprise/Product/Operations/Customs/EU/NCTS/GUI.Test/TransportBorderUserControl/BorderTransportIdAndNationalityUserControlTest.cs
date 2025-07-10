using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class BorderTransportIdAndNationalityUserControlTest : TestCaseWithFactory
	{
		public void TestISupportMultipleResourceStringDataSupporterMembers()
		{
			using (var control = new BorderTransportIdAndNationalityUserControl())
			{
				ISupportMultipleResourceStringDataSupporter supporter = control;
				AssertNull("When CurrentDataItem is null", supporter.SupportMultipleResourceStringData);
				var header = Factory.New<NctsHeader>();
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				header.SetMovementType(NctsMovementType.Codes.Departure);
				control.SetDataBinding(header.MovementHeader, "");
				AssertSequencesEqual("Same as header.MultipleKeysToUse", header.MultipleKeysToUse, supporter.SupportMultipleResourceStringData.MultipleKeysToUse);
				var movementHeader = header.MovementHeader;
				movementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
				movementHeader.BM_ActiveBorderIdentificationType = NctsTransportTypeOfIdList.Codes._10;
				var headerMultipleKeysToUse = header.MultipleKeysToUse;
				var multipleKeysToUse = supporter.SupportMultipleResourceStringData.MultipleKeysToUse;
				AssertEquals("shoud have less", true, headerMultipleKeysToUse.Count < multipleKeysToUse.Count);
				AssertSequencesEqual("Same as movementHeader.MultipleKeysToUse", movementHeader.MultipleKeysToUse, multipleKeysToUse);

				const string assertionMessage =
					"After deleting the 'MovementHeader', accessing 'SupportMultipleResourceStringData.MultipleKeysToUse' would result in a DeveloperNotificationException." +
					"Hence 'SupportMultipleResourceStringData' should be null.";
				movementHeader.Delete();
				AssertNull(assertionMessage, supporter.SupportMultipleResourceStringData);
			}
		}

		public void TestBindingSourceDataSourceType()
		{
			using (var control = new BorderTransportIdAndNationalityUserControl())
			{
				AssertEquals("DataSourceType", typeof(NctsDepartureMovementHeader), control.BindingSource.DataSourceType);
			}
		}

		public void TestCaptionRenderingEnabled()
		{
			using (var control = new BorderTransportIdAndNationalityUserControl())
			{
				AssertEquals("CaptionRenderingEnabled", true, control.CaptionRenderingEnabled);
			}
		}

		public void TestBorderTransportIdTextBox()
		{
			using (var control = new BorderTransportIdAndNationalityUserControl())
			{
				var transportIdTextBox = control.BorderTransportIdTextBox;
				CombineAssertions(() =>
				{
					AssertType<ZTextBox>("Type", transportIdTextBox);
					AssertEquals("BindTo", nameof(NctsDepartureMovementHeader.BM_TOLCarrierID), transportIdTextBox.BindTo);
					AssertEquals("CharacterCasing", System.Windows.Forms.CharacterCasing.Normal, transportIdTextBox.CharacterCasing);
				});
			}
		}

		public void TestVesselCodeFindBox()
		{
			using (var control = new BorderTransportIdAndNationalityUserControl())
			{
				var vesselCodeFindBox = control.VesselCodeFindBox;
				CombineAssertions(() =>
				{
					AssertType<ZCodeFindBox>("Type", vesselCodeFindBox);
					AssertEquals("BindTo", nameof(NctsDepartureMovementHeader.BM_TOLCarrierID), vesselCodeFindBox.BindTo);
					AssertEquals("Location of vesselCodeFindBox should same with BorderTransportIdTextBox", control.BorderTransportIdTextBox.Location, vesselCodeFindBox.Location);
				});
			}
		}

		public void TestBorderTransportNationalityCodeFindBox()
		{
			using (var control = new BorderTransportIdAndNationalityUserControl())
			{
				var transportNationalityCodeFindBox = control.BorderTransportNationalityCodeFindBox;
				CombineAssertions(() =>
				{
					AssertType<ZCodeFindBox>("Type", transportNationalityCodeFindBox);
					AssertEquals("BindTo", nameof(NctsDepartureMovementHeader.BM_RN_NKTOLCarrierNationality), transportNationalityCodeFindBox.BindTo);
				});
			}
		}
	}
}
