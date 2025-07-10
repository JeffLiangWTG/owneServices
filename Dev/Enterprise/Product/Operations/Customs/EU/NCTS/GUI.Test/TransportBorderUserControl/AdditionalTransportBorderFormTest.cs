using System.Data;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using static Enterprise.Customs.Business.AutoCusTransportMeans.Schema;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(AdditionalTransportBorderForm))]
	sealed class AdditionalTransportBorderFormTest : ZFormBasherTest
	{
		public void TestCloseButton_ItemUpdated()
		{
			CombineAssertions(() =>
			{
				var movementHeader = PrepareDataForTest();
				var item = movementHeader.AdditionalTransportAtBorderList.AddNew();
				item.TPM_TypeOfIdentification = NctsTransportTypeOfIdList.Codes._10;
				item.TPM_IdentificationNumber = "ABCD";
				using (var form = new AdditionalTransportBorderForm(movementHeader))
				{
					form.Show();
					AssertEquals("Caption", "Cancel", form.CloseButton.CaptionResourceString.Caption);
					AssertEquals("DialogResult", DialogResult.Cancel, form.CloseButton.DialogResult);

					movementHeader.AdditionalTransportAtBorderList[0].TPM_IdentificationNumber = "XYZ";
					form.CloseButton.PerformClick();
					AssertEquals("Form is closed after clicking button", false, form.Visible);
					AssertEquals("No change", "ABCD", movementHeader.AdditionalTransportAtBorderList[0].TPM_IdentificationNumber);
				}
			});
		}

		public void TestCloseButton_ItemsAddedAndRemoved()
		{
			CombineAssertions(() =>
			{
				var movementHeader = PrepareDataForTest();
				var item = movementHeader.AdditionalTransportAtBorderList.AddNew();
				item.TPM_TypeOfIdentification = NctsTransportTypeOfIdList.Codes._10;
				item.TPM_IdentificationNumber = "ABCD";
				using (var form = new AdditionalTransportBorderForm(movementHeader))
				{
					form.Show();
					AssertEquals("Caption", "Cancel", form.CloseButton.CaptionResourceString.Caption);
					AssertEquals("DialogResult", DialogResult.Cancel, form.CloseButton.DialogResult);

					movementHeader.AdditionalTransportAtBorderList.RemoveAndDeleteAll();
					var newItem = movementHeader.AdditionalTransportAtBorderList.AddNew();
					newItem.TPM_TypeOfIdentification = NctsTransportTypeOfIdList.Codes._21;
					newItem.TPM_IdentificationNumber = "XYZ";
					form.CloseButton.PerformClick();
					AssertEquals("Form is closed after clicking button", false, form.Visible);
					AssertEquals("No change", "ABCD", movementHeader.AdditionalTransportAtBorderList[0].TPM_IdentificationNumber);
				}
			});
		}

		public void TestCloseButton_NoChanges()
		{
			var movementHeader = PrepareDataForTest();
			var item = movementHeader.AdditionalTransportAtBorderList.AddNew();
			item.TPM_TypeOfIdentification = NctsTransportTypeOfIdList.Codes._10;
			item.TPM_IdentificationNumber = "ABCD";
			Factory.Save();

			using (var form = new AdditionalTransportBorderForm(movementHeader))
			{
				form.Show();
				form.CloseButton.PerformClick();
				AssertEquals(false, movementHeader.AdditionalTransportAtBorderList.HasChanges);
			}
		}

		public void TestOKButton()
		{
			CombineAssertions(() =>
			{
				var movementHeader = PrepareDataForTest();
				var item = movementHeader.AdditionalTransportAtBorderList.AddNew();
				item.TPM_TypeOfIdentification = NctsTransportTypeOfIdList.Codes._10;
				item.TPM_IdentificationNumber = "ABCD";
				using (var form = new AdditionalTransportBorderForm(movementHeader))
				{
					form.Show();
					AssertEquals("Caption", "OK", form.OKButton.CaptionResourceString.Caption);
					AssertEquals("DialogResult", DialogResult.OK, form.OKButton.DialogResult);

					movementHeader.AdditionalTransportAtBorderList[0].TPM_IdentificationNumber = "XYZ";
					form.OKButton.PerformClick();
					AssertEquals("Form is closed after clicking button", false, form.Visible);
					AssertEquals("Was changed", "XYZ", movementHeader.AdditionalTransportAtBorderList[0].TPM_IdentificationNumber);
				}
			});
		}

		public void TestText()
		{
			using (var form = GetFormToBashCore())
			{
				AssertEquals("Transport Border", ((ZForm)form).CaptionResourceString.Caption);
			}
		}

		public void TestOKButton_NoErrorExists()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var movementHeader = PrepareDataForTest();
			using (var form = new AdditionalTransportBorderForm(movementHeader))
			{
				form.Show();
				form.OKButton.PerformClick();
				AssertEquals("Form is closed after clicking button", false, form.Visible);
			}
		}

		[RequiresSTA]
		public void TestOKButton_MessageErrorExists()
		{
			var movementHeader = PrepareDataForTest();
			using (var form = new AdditionalTransportBorderForm(movementHeader))
			{
				form.Show();
				movementHeader.AdditionalTransportAtBorderList.AddNew();
				form.OKButton.PerformClick();
				AssertEquals("Form is closed after clicking button", false, form.Visible);
			}
		}

		public void TestOKButton_ErrorExists()
		{
			CombineAssertions(() =>
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var movementHeader = PrepareDataForTest();
				using (var form = new AdditionalTransportBorderForm(movementHeader))
				{
					form.Show();

					var item = movementHeader.AdditionalTransportAtBorderList.AddNew();
					item.TPM_TypeOfIdentification = "XX";
					form.OKButton.PerformClick();
					AssertEquals("Pop-up window shows", "The form has errors. Please fix them before continuing.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Form isn't closed after clicking button", true, form.Visible);

					form.Close();
					AssertEquals("Collection was reverted", 0, movementHeader.AdditionalTransportAtBorderList.Count);
				}
			});
		}

		[RequiresSTA]
		public void TestValidateAdditionalTransportAtBorderListWhenFormIsClosed()
		{
			var provider = Factory.New<AdditionalTransportMeansProviderForTest>();
			AssertEquals("PRE-CONDITION: ValidateAdditionalTransportAtBorderListCountIsCalled", false, provider.ValidateAdditionalTransportAtBorderListCountIsCalled);

			using (var form = new AdditionalTransportBorderForm(provider))
			{
				form.Show();
				form.OKButton.PerformClick();
			}

			AssertEquals("POST-CONDITION: ValidateAdditionalTransportAtBorderListCountIsCalled", true, provider.ValidateAdditionalTransportAtBorderListCountIsCalled);
		}

		public void TestGridColumns()
		{
			using (var form = (AdditionalTransportBorderForm)GetFormToBashCore())
			{
				AssertSequencesEqual("Columns", new[] { TPM_TypeOfIdentification, TPM_IdentificationNumber, TPM_RN_NKTransportNationality, TPM_ReferenceNumber, TPM_CustomsOffice, nameof(DepartureCusTransportMeans.CustomsOfficeDescription) },
					form.AdditionalTransportBorderGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
			}
		}

		public void TestGridColumnsWidth()
		{
			using (var form = (AdditionalTransportBorderForm)GetFormToBashCore())
			{
				var grid = form.AdditionalTransportBorderGrid;
				CombineAssertions(() =>
				{
					AssertEquals("TPM_TypeOfIdentification", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80), grid.GetColumnStyle(TPM_TypeOfIdentification).Width);
					AssertEquals("TPM_IdentificationNumber", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120), grid.GetColumnStyle(TPM_IdentificationNumber).Width);
					AssertEquals("TPM_RN_NKTransportNationality", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60), grid.GetColumnStyle(TPM_RN_NKTransportNationality).Width);
					AssertEquals("TPM_ReferenceNumber", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120), grid.GetColumnStyle(TPM_ReferenceNumber).Width);
					AssertEquals("TPM_CustomsOffice", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80), grid.GetColumnStyle(TPM_CustomsOffice).Width);
					AssertEquals("CustomsOfficeDescription", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120), grid.GetColumnStyle(nameof(DepartureCusTransportMeans.CustomsOfficeDescription)).Width);
				});
			}
		}

		public void TestGridColumnType()
		{
			using (var form = (AdditionalTransportBorderForm)GetFormToBashCore())
			{
				var grid = form.AdditionalTransportBorderGrid;
				CombineAssertions(() =>
				{
					AssertType<ZDropEditColumnStyleInfo>(grid.GetColumnStyle(TPM_TypeOfIdentification));
					AssertType<ZTextBoxColumnStyleInfo>(grid.GetColumnStyle(TPM_IdentificationNumber));
					AssertType<ZCodeFindBoxColumnStyleInfo>(grid.GetColumnStyle(TPM_RN_NKTransportNationality));
					AssertType<ZTextBoxColumnStyleInfo>(grid.GetColumnStyle(TPM_ReferenceNumber));
					AssertType<ZDropEditColumnStyleInfo>(grid.GetColumnStyle(TPM_CustomsOffice));
					AssertType<ZTextBoxColumnStyleInfo>(grid.GetColumnStyle(nameof(DepartureCusTransportMeans.CustomsOfficeDescription)));
				});
			}
		}

		protected override Form GetFormToBashCore()
		{
			var movementHeader = PrepareDataForTest();
			return new AdditionalTransportBorderForm(movementHeader);
		}

		NctsDepartureMovementHeader PrepareDataForTest()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = header.MovementHeader;
			movementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._2_RailTransport;
			movementHeader.BM_ActiveBorderIdentificationType = NctsTransportTypeOfIdList.Codes._21;
			movementHeader.BM_TOLCarrierID = "ABCD";
			return movementHeader;
		}

		sealed class AdditionalTransportMeansProviderForTest : DummyBaseBusinessObject, IAdditionalTransportMeansProvider
		{
			public AdditionalTransportMeansProviderForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
				var header = Factory.New<NctsHeader>();
				header.SetMovementType(NctsMovementType.Codes.Departure);
				movementHeader = header.MovementHeader;
			}

			IDepartureCusTransportMeansCollection<DepartureCusTransportMeans> IAdditionalTransportMeansProvider.AdditionalTransportAtBorderList => movementHeader.AdditionalTransportAtBorderList;

			void IAdditionalTransportMeansProvider.ValidateAdditionalTransportAtBorderListCount()
			{
				ValidateAdditionalTransportAtBorderListCountIsCalled = true;
			}

			public bool ValidateAdditionalTransportAtBorderListCountIsCalled { get; set; }

			readonly NctsDepartureMovementHeader movementHeader;
		}
	}
}
