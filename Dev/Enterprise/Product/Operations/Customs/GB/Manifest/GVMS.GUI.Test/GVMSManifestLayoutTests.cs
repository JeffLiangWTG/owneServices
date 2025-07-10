using System.Collections.Generic;
using System.Linq;
using CargoWise.Windows.UI;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GVMS.GUI.Testing
{
	[TestedType(typeof(GVMSManifestLayout))]
	sealed class GVMSManifestLayoutTests : LayoutsAbstractTest
	{
		public void TestTabsVisibility()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				manifest.AMA_TransportMode = "ROA";
				AssertNull(asycudaManifestUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "personsTabPage"));
				AssertNull(asycudaManifestUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "billsTabPage"));
			}
		}

		public void TestFieldsVisibility()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			using (var form = new ManifestForm(manifest))
			{
				form.Show();

				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var mainTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "mainTabPage");
				mainTabControl.SelectedTab = mainTabPage;

				var carrierCodeZText = asycudaManifestUserControl.Controls.Find("CarrierCode", true).FirstOrDefault();
				var carrierCodeDropEdit = asycudaManifestUserControl.Controls.Find("CarrierCodeDropEdit", true).FirstOrDefault();
				var routeIdDropEdit = asycudaManifestUserControl.Controls.Find("RouteIdDropEdit", true).FirstOrDefault();
				var isUnaccompaniedCheckBox = asycudaManifestUserControl.Controls.Find("IsUnaccompaniedCheckBox", true).FirstOrDefault();
				var emptyVehicleDropEdit = asycudaManifestUserControl.Controls.Find("EmptyVehicleDropEdit", true).FirstOrDefault();
				var inspectionRequiredCheckBox = asycudaManifestUserControl.Controls.Find("InspectionRequiredCheckBox", true).FirstOrDefault();

				var customsReferencesGroupBox = asycudaManifestUserControl.Controls.Find("CustomsReferencesGroupBox", true).FirstOrDefault();
				var transitReferencesGroupBox = asycudaManifestUserControl.Controls.Find("TransitReferencesGroupBox", true).FirstOrDefault();
				var otherReferencesGroupBox = asycudaManifestUserControl.Controls.Find("OtherReferencesGroupBox", true).FirstOrDefault();
				var inspectionLocationsGroupBox = asycudaManifestUserControl.Controls.Find("InspectionLocationsGroupBox", true).FirstOrDefault();

				AssertNull(carrierCodeZText);
				AssertEquals(true, carrierCodeDropEdit.Visible);
				AssertEquals(true, routeIdDropEdit.Visible);
				AssertEquals(true, isUnaccompaniedCheckBox.Visible);
				AssertEquals(true, emptyVehicleDropEdit.Visible);
				AssertEquals(true, inspectionRequiredCheckBox.Visible);

				AssertEquals(true, customsReferencesGroupBox.Visible);
				AssertEquals(true, transitReferencesGroupBox.Visible);
				AssertEquals(true, otherReferencesGroupBox.Visible);
				AssertEquals(true, inspectionLocationsGroupBox.Visible);
			}
		}

		public void TestGVMSGridsCaptionsAndColumnOrder()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");

				AssertGridColumnsAndCaptions(asycudaManifestUserControl, "CustomsReferencesGroupBox", "Customs References - Customs Declarations", "CustomsReferencesGrid", GetCustomsReferencesGridColumns());
				AssertGridColumnsAndCaptions(asycudaManifestUserControl, "TransitReferencesGroupBox", "Customs References - Transit Declarations", "TransitReferencesGrid", GetTransitReferencesGridColumn());
				AssertGridColumnsAndCaptions(asycudaManifestUserControl, "OtherReferencesGroupBox", "Customs References - Other", "OtherReferencesGrid", GetOtherReferencesGridColumns());
				AssertGridColumnsAndCaptions(asycudaManifestUserControl, "InspectionLocationsGroupBox", "Inspection(s) Details", "InspectionLocationsGrid", GetInspectionLocationsGridColumns());
			}
		}

		protected override int ControlBagCount => 2;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
				yield return ThirdColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CommonManifestControlBag.Instance.CountryTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.RegistrationDateEdit, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.RegistrationNumberTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.TransportModeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.ManifestTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.NatureDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.AgentTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.ContainerModeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.BuyersConsolidationCheckBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.VesselCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.VoyageFlightTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.LloydsNumberTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.RadioCallSignTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.ConveyanceCountryCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.MastersNameTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.VehicleRegistrationTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.Trailer1RegNoTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.Trailer1RegCountryCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.Trailer2RegNoTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.Trailer2RegCountryCodeFindBox, ControlWidthClass.Long);
				yield return (GVMSManifestControlBag.Instance.IsUnaccompaniedCheckBox, ControlWidthClass.Long);
				yield return (GVMSManifestControlBag.Instance.EmptyVehicleDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.PortOfLoadingCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.EstDepartureDateEdit, ControlWidthClass.Auto);
				yield return (CommonManifestControlBag.Instance.PortOfFirstArrivalCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.PortOfDischargeCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.EstArrivalDateEdit, ControlWidthClass.Auto);
				yield return (GVMSManifestControlBag.Instance.HaulierTypeDropEdit, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CommonManifestControlBag.Instance.JobReferenceTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.MessageStatusDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.CustomsStatusDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.CarrierAddressControl, ControlWidthClass.Long);
				yield return (GVMSManifestControlBag.Instance.CarrierCodeDropEdit, ControlWidthClass.Long);
				yield return (GVMSManifestControlBag.Instance.RouteIdDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.ShippingAgentAddressControl, ControlWidthClass.Long);
				yield return (GVMSManifestControlBag.Instance.CustomsReferencesGroupBox, ControlWidthClass.Long);
				yield return (GVMSManifestControlBag.Instance.TransitReferencesGroupBox, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (GVMSManifestControlBag.Instance.InspectionRequiredCheckBox, ControlWidthClass.Long);
				yield return (GVMSManifestControlBag.Instance.InspectionLocationsGroupBox, ControlWidthClass.Long);
				yield return (GVMSManifestControlBag.Instance.OtherReferencesGroupBox, ControlWidthClass.Long);
			}
		}

		public void TestGVMSManifestRemovedUserControl()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var messageStatusTextBox = asycudaManifestUserControl.Controls.Find("MessageStatusTextBox", true).FirstOrDefault();
				var manifestNumberFromMasterBillTextBox = asycudaManifestUserControl.Controls.Find("ManifestNumberFromMasterBillTextBox", true).FirstOrDefault();
				var masterBOLTextBox = asycudaManifestUserControl.Controls.Find("MasterBOLTextBox", true).FirstOrDefault();
				var issueDateDateEdit = asycudaManifestUserControl.Controls.Find("IssueDateDateEdit", true).FirstOrDefault();

				AssertNull(messageStatusTextBox);
				AssertNull(manifestNumberFromMasterBillTextBox);
				AssertNull(masterBOLTextBox);
				AssertNull(issueDateDateEdit);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new GVMSManifestLayoutBuilder<AsycudaManifestHeader>();

		void AssertGridColumnsAndCaptions(AsycudaManifestUserControl asycudaManifestUserControl, string groupBoxName, string groupBoxCaption, string gridToTest, string[] expectedColumnNames)
		{
			var groupBox = asycudaManifestUserControl.Controls.Find(groupBoxName, true).FirstOrDefault();
			var grid = (ZGrid)groupBox.Controls.Find(gridToTest, true).FirstOrDefault();
			AssertEquals(gridToTest + " Expected Group Box Caption", groupBoxCaption, groupBox.GetExtension<ILabelCaptionRenderer>().Caption);
			var columns = grid.Columns;
			AssertEquals(gridToTest + " Expected Column Count", expectedColumnNames.Length, columns.Count);

			for (int i = 0; i < expectedColumnNames.Length; i++)
			{
				var column = columns[i];
				AssertNotNull(column);
				AssertEquals(gridToTest + ": Caption for column " + i.ToString(), expectedColumnNames[i], column.ColumnStyle.HeaderText);
			}
		}

		string[] GetCustomsReferencesGridColumns() => new string[] { "Type", "Reference No.", "S&S Reference", "Date of Issue", "Country Code" };
		string[] GetTransitReferencesGridColumn() => new string[] { "Type", "Reference No.", "S&S Reference", "Date of Issue", "Country Code", "TSAD" };
		string[] GetOtherReferencesGridColumns() => new string[] { "Type", "EORI/Reference", "S&S Reference", "Date of Issue", "Country Code", "LRN/Local Reference Number", "Procedure" };
		string[] GetInspectionLocationsGridColumns() => new string[] { "Inspection Type", "Inspection Location" };
	}
}
