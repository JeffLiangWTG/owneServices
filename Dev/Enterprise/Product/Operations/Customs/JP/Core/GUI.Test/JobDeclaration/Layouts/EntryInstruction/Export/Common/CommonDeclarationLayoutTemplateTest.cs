using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(CommonDeclarationLayoutTemplate))]
	sealed class CommonDeclarationLayoutTemplateTest : TestCase
	{
		public void TestControls()
		{
			using var control = new CommonDeclarationLayoutTemplate();
			CombineAssertions("Visible", () =>
			{
				Assert("ExportCodeTextBox", control.FindSingle<ZTextBox>("ExportCodeTextBox").Visible);
				Assert("ExportNameTextBox", control.FindSingle<ZTextBox>("ExportNameTextBox").Visible);
				Assert("DeclarantCodeTextBox", control.FindSingle<ZTextBox>("DeclarantCodeTextBox").Visible);
				Assert("CarrierCodeCodeFindBox", control.FindSingle<ZCodeFindBox>("CarrierCodeCodeFindBox").Visible);
				Assert("VesselCodeFindBox", control.FindSingle<ZCodeFindBox>("VesselCodeFindBox").Visible);
				Assert("VesselNameCodeFindBox", control.FindSingle<ZCodeFindBox>("VesselNameCodeFindBox").Visible);
				Assert("DeclarationReferenceTextBox", control.FindSingle<ZTextBox>("DeclarationReferenceTextBox").Visible);
				Assert("FinalDestinationCodeFindBox", control.FindSingle<ZCodeFindBox>("FinalDestinationCodeFindBox").Visible);
				Assert("ExportControlNumberUserControl", control.FindSingle<ExportControlNumberUserControl>("ExportControlNumberUserControl").Visible);
				Assert("VoyageFlightNoBoundTextBox", control.FindSingle<ZTextBox>("VoyageFlightNoBoundTextBox").Visible);
				Assert("DateOfArrivalBoundDateEdit", control.FindSingle<ZDateEdit>("DateOfArrivalBoundDateEdit").Visible);
				Assert("PortOfLoadingCodeFindBox", control.FindSingle<ZCodeFindBox>("PortOfLoadingCodeFindBox").Visible);
				Assert("ExportDateBoundDateEdit", control.FindSingle<ZDateEdit>("ExportDateBoundDateEdit").Visible);
				Assert("PortOfDischargeCodeFindBox", control.FindSingle<ZCodeFindBox>("PortOfDischargeCodeFindBox").Visible);
				Assert("ReceiptModeDropEdit", control.FindSingle<ZDropEdit>("ReceiptModeDropEdit").Visible);
				Assert("DeliveryModeDropEdit", control.FindSingle<ZDropEdit>("DeliveryModeDropEdit").Visible);
				Assert("BookingNumberTextBox", control.FindSingle<ZTextBox>("BookingNumberTextBox").Visible);
				Assert("AllEntryInsSeparatorUserControl", control.FindSingle<SeparatorUserControl>("AllEntryInsSeparatorUserControl").Visible);
			});
		}
	}
}
