using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI.Testing
{
	class CustomsOfficesUserControlTest : TestCaseWithFactory
	{
		public void TestPurposeDescription()
		{
			using (var control = new CustomsOfficesUserControl())
			{
				var customsOfficesGrid = control.FindSingleOrDefault<ZGrid>("CustomsOfficesGrid");
				AssertEquals("Purpose description is visible", true, customsOfficesGrid.GetColumnStyle(OfficeCode.Schema.CY_CodeDescription).IsVisible);
			}
		}
		public void TestTimeColumnIsUnavailable()
		{
			using (var control = new CustomsOfficesUserControl())
			{
				var customsOfficesGrid = control.FindSingleOrDefault<ZGrid>("CustomsOfficesGrid");
				AssertEquals("Time column is unavailable", true, customsOfficesGrid.GetColumnStyle(OfficeCode.Schema.CY_Date).IsUnavailable);
			}
		}
		public void TestCustomsOfficesGrid()
		{
			using (var control = new CustomsOfficesUserControl())
			{
				AssertEquals("CustomsOfficesGrid.Visible", true, control.FindSingleOrDefault<ZGrid>("CustomsOfficesGrid").Visible);
			}
		}

		public void TestCY_OfficeDescriptionWidth()
		{
			using (var control = new CustomsOfficesUserControl())
			{
				var customsOfficesGrid = control.FindSingleOrDefault<ZGrid>("CustomsOfficesGrid");
				AssertEquals("CY_OfficeDescription.Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(156), customsOfficesGrid.GetColumnWidth(OfficeCode.Schema.CY_OfficeDescription));
			}
		}

		public void TestCustomsOfficeFindBox()
		{
			using (var control = new CustomsOfficesUserControl())
			{
				var customsOfficeFindBox = control.FindSingleOrDefault<ZCodeFindBox>("CustomsOfficeFindBox");
				AssertEquals("Visible", true, customsOfficeFindBox.Visible);
				AssertEquals("CaptionResourceString should be empty; use property resource string instead", CargoWiseOne.ResourceStrings.ResourceStringData.Empty, customsOfficeFindBox.CaptionResourceString);
			}
		}

		public void TestCaptions()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new ZForm(declaration))
			using (var control = new CustomsOfficesUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var customsOfficeFindBox = control.FindSingle<ZCodeFindBox>("CustomsOfficeFindBox");
				var labelCaptionRenderer = customsOfficeFindBox.Extensions.Get<ILabelCaptionRenderer>();
				AssertEquals("Caption of CustomsOfficeFindBox  for IMP.", "Office of Lodgement", labelCaptionRenderer.Caption);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("Caption of CustomsOfficeFindBox  for EXP.", "Office of Export", labelCaptionRenderer.Caption);

				declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
				AssertEquals("Caption of CustomsOfficeFindBox  for EXS.", "Office of Lodgement", labelCaptionRenderer.Caption);
			}
		}

		public void TestCaptionsOnShipmentForm()
		{
			var shipment = Factory.New<ForwardingShipment>();
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			using (var form = new ShipmentForm(shipment))
			{
				form.Show();
				Application.DoEvents();

				form.PlugIns.SelectPlugInTabPage(ZArchitecture.Modules.ControllerIDs.Customs.JobDeclaration);

				var plugin = (BrokeragePlugIn)form.PlugIns.GetPlugIn(ZArchitecture.Modules.ControllerIDs.Customs.JobDeclaration);
				plugin.OnGUIShown();

				var brokerageUserControl = (CustomsBrokerageUserControl)plugin.UserControl;
				var control = brokerageUserControl.FindSingle<CustomsOfficesUserControl>();
				var customsOfficeFindBox = control.FindSingle<ZCodeFindBox>("CustomsOfficeFindBox");
				var labelCaptionRenderer = customsOfficeFindBox.Extensions.Get<ILabelCaptionRenderer>();
				AssertEquals("Caption of CustomsOfficeFindBox  for IMP.", "Office of Lodgement", labelCaptionRenderer.Caption);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("Caption of CustomsOfficeFindBox  for EXP.", "Office of Export", labelCaptionRenderer.Caption);

				declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
				AssertEquals("Caption of CustomsOfficeFindBox  for EXS.", "Office of Lodgement", labelCaptionRenderer.Caption);
			}
		}
	}
}
