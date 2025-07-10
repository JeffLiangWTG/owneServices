using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.EU.ExitControl.GUI;
using Enterprise.Customs.IE.ExitControl.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.ExitControl.GUI.Testing
{
	[TestedType(typeof(ReportsGridFieldsLayout))]
	class ReportsGridFieldsLayoutTest : LayoutsAbstractTest
	{
		[RequiresSTA]
		public void TestConsignmentGuidDropEditVisibility()
		{
			TestLayoutVisibility("ConsignmentGuidDropEdit", "related to CER_Status", (report) => report.CER_Status = "", (report) => report.CER_Status = "200");
			TestLayoutVisibility("ConsignmentGuidDropEdit", "related to CER_MessageStatus", (report) => report.CER_MessageStatus = "", (report) => report.CER_MessageStatus = "SNT");
		}

		public void TestTypeOfLocationDropEditVisibility()
		{
			TestLayoutVisibility("TypeOfLocationDropEdit", "related to CER_Type", (report) => report.CER_Type = "PRE", (report) => report.CER_Type = "EXT");
		}

		public void TestFormattedDateTimeDateEditVisibility()
		{
			TestLayoutVisibility("FormattedDateTimeDateEdit", "related to CER_Type", (report) => report.CER_Type = "EXT", (report) => report.CER_Type = "PRE");
			TestLayoutVisibility("LongFormattedDateTimeDateEdit", "related to CER_Type", (report) => report.CER_Type = "PRE", (report) => report.CER_Type = "EXT");
		}

		public void TestOfficeOfExportCodeFindBoxVisibility()
		{
			TestLayoutVisibility("OfficeOfExportCodeFindBox", "related to CER_Type", (report) => report.CER_Type = "ALT", (report) => report.CER_Type = "EXT");
		}

		[RequiresSTA]
		public void TestUNLOCOCodeFindBoxVisibility()
		{
			TestLayoutVisibility("UNLOCOCodeFindBox", "related to CER_Type", (report) => report.CER_Type = "PRE", (report) => report.CER_Type = "EXT");
		}

		[RequiresSTA]
		public void TestEnquiryInformationCodeDropEditVisibility()
		{
			TestLayoutVisibility("EnquiryInformationCodeDropEdit", "related to CER_Type", (report) => report.CER_Type = "ALT", (report) => report.CER_Type = "EXT");
		}

		[RequiresSTA]
		public void TestAdditionalDeclarationTypeDropEditVisibility()
		{
			TestLayoutVisibility("AdditionalDeclarationTypeDropEdit", "related to CER_Type", (report) => report.CER_Type = "EXT", (report) => report.CER_Type = "ALT");
		}

		[RequiresSTA]
		public void TestLocationTextBoxVisibility()
		{
			TestLayoutVisibility("LocationTextBox", "related to CER_Type", (report) => report.CER_Type = "PRE", (report) => report.CER_Type = "EXT");
		}

		public void TestDiscrepanciesCheckBoxVisibility()
		{
			TestLayoutVisibility("DiscrepanciesCheckBox", "related to CER_Type", (report) => report.CER_Type = "EXT", (report) => report.CER_Type = "ALT");
		}

		[RequiresSTA]
		public void TestTransportVisiblities()
		{
			TestLayoutVisibility("TransportIDTextBox", "related to CER_Type and CER_Calc_Discrepancies", SetReportPresentationAndDiscrepancies, SetReportNonPresentationAndDiscrepancies);
			TestLayoutVisibility("TransportIDTextBox", "related to CER_Type and CER_Calc_Discrepancies", SetReportPresentationAndDiscrepancies, SetReportNonPresentationAndDiscrepancies);
			TestLayoutVisibility("TransportIDTextBox", "related to CER_Type and CER_Calc_Discrepancies", SetReportPresentationAndNoDiscrepancies, SetReportNonPresentationAndDiscrepancies);
			TestLayoutVisibility("TransportNationalityDropEdit", "related to CER_Type and CER_Calc_Discrepancies", SetReportPresentationAndDiscrepancies, SetReportNonPresentationAndDiscrepancies);
			TestLayoutVisibility("TransportNationalityDropEdit", "related to CER_Type and CER_Calc_Discrepancies", SetReportPresentationAndDiscrepancies, SetReportNonPresentationAndDiscrepancies);
			TestLayoutVisibility("TransportNationalityDropEdit", "related to CER_Type and CER_Calc_Discrepancies", SetReportPresentationAndNoDiscrepancies, SetReportNonPresentationAndDiscrepancies);
			TestLayoutVisibility("TransportTypeDropEdit", "related to CER_Type and CER_Calc_Discrepancies", SetReportPresentationAndDiscrepancies, SetReportNonPresentationAndDiscrepancies);
			TestLayoutVisibility("TransportTypeDropEdit", "related to CER_Type and CER_Calc_Discrepancies", SetReportPresentationAndDiscrepancies, SetReportNonPresentationAndDiscrepancies);
			TestLayoutVisibility("TransportTypeDropEdit", "related to CER_Type and CER_Calc_Discrepancies", SetReportPresentationAndNoDiscrepancies, SetReportNonPresentationAndDiscrepancies);
		}

		public void TestOrganisationVisiblities()
		{
			TestLayoutVisibility("RepresentativeAddressDropEdit", "related to CER_Type", (report) => report.CER_Type = "ALT", (report) => report.CER_Type = "EXT");
			TestLayoutVisibility("DeclarantAddressDropEdit", "related to CER_Type", (report) => report.CER_Type = "ALT", (report) => report.CER_Type = "EXT");
			TestLayoutVisibility("DeclarantTypeDropEdit", "related to CER_Type", (report) => report.CER_Type = "ALT", (report) => report.CER_Type = "EXT");
		}

		void TestLayoutVisibility(string controlToFind, string failMessage, Action<CusExitReport> visibleFunc, Action<CusExitReport> invisibleFunc)
		{
			var report = Factory.New<CusExitReport>();
			using (var form = new ZForm())
			using (var userControl = new ReportsGridFieldsLayoutUserControl())
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(report, "");
				form.Show();

				var consignmentGuidDropEdit = userControl.Controls.Find(controlToFind, true).FirstOrDefault();

				CombineAssertions(() =>
				{
					AssertNotNull("Can not find target userControl", consignmentGuidDropEdit);

					visibleFunc(report);
					Assert("Should be Visible, " + failMessage, consignmentGuidDropEdit.Visible);

					invisibleFunc(report);
					Assert("Should not be Visible, " + failMessage, !consignmentGuidDropEdit.Visible);
				});
			}
		}

		void SetReportPresentationAndDiscrepancies(CusExitReport cusExitReport)
		{
			cusExitReport.CER_Type = "PRE";
			cusExitReport.CER_Behavior = "DIS";
		}

		void SetReportPresentationAndNoDiscrepancies(CusExitReport cusExitReport)
		{
			cusExitReport.CER_Type = "PRE";
			cusExitReport.CER_Behavior = "STD";
		}

		void SetReportNonPresentationAndDiscrepancies(CusExitReport cusExitReport)
		{
			cusExitReport.CER_Type = "ALT";
			cusExitReport.CER_Behavior = "DIS";
		}

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				var euBag = EU.ExitControl.GUI.ReportsGridFieldsControlBag.Instance;
				var ieBag = ReportsGridFieldsControlBag.Instance;

				yield return new List<(ControlReference, ControlWidthClass)>
				{
					(euBag.ConsignmentGuidDropEdit, ControlWidthClass.Long),
					(euBag.OfficeOfExitCodeFindBox, ControlWidthClass.Long),
					(ieBag.OfficeOfExportCodeFindBox, ControlWidthClass.Long),
					(ieBag.FormattedDateTimeDateEdit, ControlWidthClass.Medium),
					(ieBag.LongFormattedDateTimeDateEdit, ControlWidthClass.Medium),
					(ieBag.DiscrepanciesCheckBox, ControlWidthClass.Auto)
				};

				yield return new List<(ControlReference, ControlWidthClass)>
				{
					(ieBag.TypeOfLocationDropEdit, ControlWidthClass.Long),
					(ieBag.UNLOCOCodeFindBox, ControlWidthClass.Long),
					(ieBag.EnquiryInformationCodeDropEdit, ControlWidthClass.Auto),
					(ieBag.DeclarantAddressDropEdit, ControlWidthClass.Long),
					(ieBag.RepresentativeAddressDropEdit, ControlWidthClass.Long),
					(ieBag.DeclarantTypeDropEdit, ControlWidthClass.Auto),
					(ieBag.AdditionalDeclarationTypeDropEdit, ControlWidthClass.Long),
					(ieBag.LocationTextBox, ControlWidthClass.Long)
				};

				yield return new List<(ControlReference, ControlWidthClass)>
				{
					(ieBag.TransportModeDropEdit, ControlWidthClass.Long),
					(euBag.TransportIDTextBox, ControlWidthClass.Long),
					(euBag.TransportNationalityDropEdit, ControlWidthClass.Long),
					(euBag.TransportTypeDropEdit, ControlWidthClass.Long)
				};
			}
		}

		protected override int ControlBagCount => 2;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ReportsGridFieldsLayoutBuilder<CusExitReport>();
	}
}
