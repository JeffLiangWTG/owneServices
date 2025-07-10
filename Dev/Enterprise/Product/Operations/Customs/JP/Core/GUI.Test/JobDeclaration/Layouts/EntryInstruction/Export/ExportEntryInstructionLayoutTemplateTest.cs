using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Customs.JP.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(ExportEntryInstructionLayoutTemplate))]
	sealed class ExportEntryInstructionLayoutTemplateTest : TestCase
	{
		public void TestControls()
		{
			using var control = new ExportEntryInstructionLayoutTemplate();
			CombineAssertions("Visible", () =>
			{
				Assert("PreInspectedCargoDropEdit", control.FindSingle<ZDropEdit>("PreInspectedCargoDropEdit").Visible);
				Assert("LoadingConfirmationIsRequiredCheckBox", control.FindSingle<ZCheckBox>("LoadingConfirmationIsRequiredCheckBox").Visible);
				Assert("ExportControlNumberTextBox", control.FindSingle<ZTextBox>("ExportControlNumberTextBox").Visible);
				Assert("DeclarationCargoTypeDropEdit", control.FindSingle<ZDropEdit>("DeclarationCargoTypeDropEdit").Visible);
				Assert("GoodsDescriptionTextBox", control.FindSingle<ZTextBox>("GoodsDescriptionTextBox").Visible);
				Assert("AwbOrBillNumberTextBox", control.FindSingle<ZTextBox>("AwbOrBillNumberTextBox").Visible);
			});
		}

		public void TestVanningLocationsGrid()
		{
			using var control = new ExportEntryInstructionLayoutTemplate();
			CombineAssertions(() =>
			{
				var vanningLocationsGridGroupBox = control.FindSingle<ZGroupBox>("VanningLocationsGroupBox");
				AssertEquals("ExportEntryInstructionTemplate Must contain VanningLocationsGroupBox", true, control.Contains(vanningLocationsGridGroupBox));
				AssertEquals("English Caption", "Vanning Locations", vanningLocationsGridGroupBox?.CaptionResourceString.Caption);

				var grid = control.FindSingle<ZGrid>("VanningLocationsGrid");
				AssertEquals("Must be in Additional Details tab page", true, vanningLocationsGridGroupBox.Contains(grid));
				AssertEndsWith("Binds to correct field", "VanningLocations", control.BindingSource.GetBindingMember(grid));

				AssertEquals("MaximumRows of VanningLocationsGrid must be equal to VanningAddressCollection.MaxRowCount", VanningAddressCollection.MaxRowCount, grid.MaximumRows);
			});
		}

		public void TestVanningLocationsGridColumnsOrders()
		{
			using var control = new ExportEntryInstructionLayoutTemplate();
			var grid = control.FindSingle<ZGrid>("VanningLocationsGrid");
			var columnStyles = grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName);
			var expectedColumnsOrders = new[]
			{
					"E2_AddressSequence",
					"OrganisationPK",
					"E2_OA_Address",
					"E2_AddressOverride",
					"E2_GovRegNumType",
					"E2_GovRegNum",
					"E2_RN_NKCountryCode",
					"E2_CompanyName",
					"E2_State",
					"E2_City",
					"E2_Address1",
					"E2_AdditionalAddressInformation",
			};

			AssertContainsExactElementsInExactOrder(expectedColumnsOrders, columnStyles);
		}
	}
}
