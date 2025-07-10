using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Module.Testing
{
	class NctsMovementFilterControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestHeaderGridColumnWidths()
		{
			using (var filterControl = new NctsMovementFilterControl(new NctsHeaderCollection(Factory), new NctsMovementFilterStripBusinessObject()))
			{
				var grid = filterControl.Grid;
				CombineAssertions(() =>
				{
					AssertEquals("JobReferenceNumber", 90, grid.GetColumnStyle(NctsHeader.Schema.JobReferenceNumber).Width);
					AssertEquals("MovementReferenceNumber", 120, grid.GetColumnStyle(NctsHeader.Schema.MovementReferenceNumber).Width);
					AssertEquals("LocalReferenceNumberForDisplay", 163, grid.GetColumnStyle(NctsHeader.Schema.LocalReferenceNumberForDisplay).Width);
					AssertEquals("BH_HeaderType", 47, grid.GetColumnStyle(NctsHeader.Schema.BH_HeaderType).Width);
					AssertEquals("DestinationCustomsOfficeCodeForArrival", 115, grid.GetColumnStyle(NctsHeader.Schema.DestinationCustomsOfficeCodeForArrival).Width);
					AssertEquals("BH_FTZMove", 67, grid.GetColumnStyle(NctsHeader.Schema.BH_FTZMove).Width);
					AssertEquals("CountryOfDispatch", 47, grid.GetColumnStyle(NctsHeader.Schema.CountryOfDispatch).Width);
					AssertEquals("PlaceOfUnloading", 90, grid.GetColumnStyle(NctsHeader.Schema.PlaceOfUnloading).Width);
					AssertEquals("DeclarationPlace", 73, grid.GetColumnStyle(NctsHeader.Schema.DeclarationPlace).Width);
					AssertEquals("TotalNumberOfItems", 54, grid.GetColumnStyle(NctsHeader.Schema.TotalNumberOfItems).Width);
					AssertEquals("TotalNumberOfPackages", 54, grid.GetColumnStyle(NctsHeader.Schema.TotalNumberOfPackages).Width);
					AssertEquals("TotalGrossMassInKilograms", 81, grid.GetColumnStyle(NctsHeader.Schema.TotalGrossMassInKilograms).Width);
					AssertEquals("BH_ApplicationCode", 80, grid.GetColumnStyle(NctsHeader.Schema.BH_ApplicationCode).Width);
					AssertEquals("ReleaseDate", 109, grid.GetColumnStyle(NctsHeader.Schema.MovementReferenceIssueDate).Width);
				});
			}
		}

		[RequiresSTA]
		public void TestDepartureMovementGridColumnWidths()
		{
			using (var filterControl = new NctsMovementFilterControl(new NctsHeaderCollection(Factory), new NctsMovementFilterStripBusinessObject()))
			{
				var grid = filterControl.Grid;
				CombineAssertions(() =>
				{
					AssertEquals("BM_CustomsStatus", 62, grid.GetColumnStyle(ConcatMovementHeaderWithColumnName(NctsDepartureMovementHeader.Schema.BM_CustomsStatus)).Width);
					AssertEquals("DepartureStatusDescription", 163, grid.GetColumnStyle(ConcatMovementHeaderWithColumnName(NctsDepartureMovementHeader.Schema.CustomsStatusDescription)).Width);
					AssertEquals("IsSimplifiedNctsProcedure", 127, grid.GetColumnStyle(ConcatMovementHeaderWithColumnName(NctsDepartureMovementHeader.Schema.IsSimplifiedNctsProcedure)).Width);
					AssertEquals("CountryOfDestination", 48, grid.GetColumnStyle(ConcatMovementHeaderWithColumnName(NctsDepartureMovementHeader.Schema.BM_RL_NKDestinationPort)).Width);
					AssertEquals("DeclarationType", 63, grid.GetColumnStyle(ConcatMovementHeaderWithColumnName(NctsDepartureMovementHeader.Schema.BM_InBondEntryType)).Width);
					AssertEquals("AgreedLocationOfGoodsCode", 91, grid.GetColumnStyle(ConcatMovementHeaderWithColumnName(NctsDepartureMovementHeader.Schema.BM_LocationOfGoodsCode)).Width);
					AssertEquals("AgreedLocationOfGoods", 80, grid.GetColumnStyle(ConcatMovementHeaderWithColumnName(NctsDepartureMovementHeader.Schema.BM_LocationOfGoods)).Width);
					AssertEquals("MeansOfTransportAtDepartureIdentity", 104, grid.GetColumnStyle(ConcatMovementHeaderWithColumnName(NctsDepartureMovementHeader.Schema.BM_TransportAtDeparture)).Width);
					AssertEquals("PlaceOfLoading", 98, grid.GetColumnStyle(ConcatMovementHeaderWithColumnName(NctsDepartureMovementHeader.Schema.PlaceOfLoading)).Width);
					AssertEquals("CustomsSubPlace", 70, grid.GetColumnStyle(ConcatMovementHeaderWithColumnName(NctsDepartureMovementHeader.Schema.BM_CustomsSubPlace)).Width);
					AssertEquals("InlandTransportMode", 117, grid.GetColumnStyle(ConcatMovementHeaderWithColumnName(NctsDepartureMovementHeader.Schema.BM_InlandTransportMode)).Width);
					AssertEquals("TransportModeAtBorder", 130, grid.GetColumnStyle(ConcatMovementHeaderWithColumnName(NctsDepartureMovementHeader.Schema.BM_ExportTransportMode)).Width);
					AssertEquals("MeansOfTransportAtDepartureNationality", 73, grid.GetColumnStyle(ConcatMovementHeaderWithColumnName(NctsDepartureMovementHeader.Schema.BM_RN_NKTransportAtDepartureCountry)).Width);
					AssertEquals("MeansOfTransportCrossingBorderIdentity", 140, grid.GetColumnStyle(ConcatMovementHeaderWithColumnName(NctsDepartureMovementHeader.Schema.BM_TOLCarrierID)).Width);
					AssertEquals("MeansOfTransportCrossingBorderNationality", 84, grid.GetColumnStyle(ConcatMovementHeaderWithColumnName(NctsDepartureMovementHeader.Schema.BM_TOLCarrierCode)).Width);
					AssertEquals("SpecificCircumstanceIndicator", 74, grid.GetColumnStyle(ConcatMovementHeaderWithColumnName(NctsDepartureMovementHeader.Schema.BM_BTAIndicator)).Width);
					AssertEquals("TransportChargesMoP", 47, grid.GetColumnStyle(ConcatMovementHeaderWithColumnName(NctsDepartureMovementHeader.Schema.BM_MethodOfPayment)).Width);
					AssertEquals("CommercialReferenceNumber", 100, grid.GetColumnStyle(ConcatMovementHeaderWithColumnName(NctsDepartureMovementHeader.Schema.BM_AdditionalText)).Width);
					AssertEquals("ConveyanceReferenceNumber", 93, grid.GetColumnStyle(ConcatMovementHeaderWithColumnName(NctsDepartureMovementHeader.Schema.BM_ConveyanceNumber)).Width);
					AssertEquals("ControlDateLimit", 117, grid.GetColumnStyle(ConcatMovementHeaderWithColumnName(NctsDepartureMovementHeader.Schema.BM_ExportDate)).Width);
					AssertEquals("RepresentativeCode", 95, grid.GetColumnStyle(ConcatMovementHeaderWithColumnName(NctsDepartureMovementHeader.Schema.BM_GS_NKCusAgent)).Width);
					AssertEquals("IsContainerised", 89, grid.GetColumnStyle(ConcatMovementHeaderWithColumnName(NctsDepartureMovementHeader.Schema.IsContainerised)).Width);
					AssertEquals("DepartureCustomsOfficeCodeForModuleGrid", 103, grid.GetColumnStyle(ConcatMovementHeaderWithColumnName(NctsDepartureMovementHeader.Schema.DepartureCustomsOfficeCodeForModuleGrid)).Width);
					AssertEquals("DestinationCustomsOfficeCodeForDepartureForModuleGrid", 115, grid.GetColumnStyle(ConcatMovementHeaderWithColumnName(NctsDepartureMovementHeader.Schema.DestinationCustomsOfficeCodeForDepartureForModuleGrid)).Width);
					AssertEquals("UniqueConsignmentReferenceNumber", 208, grid.GetColumnStyle(ConcatMovementHeaderWithColumnName(NctsDepartureMovementHeader.Schema.BM_UniqueConsignmentReference)).Width);
					AssertEquals("AdditionalDeclarationType", 153, grid.GetColumnStyle(ConcatMovementHeaderWithColumnName(NctsDepartureMovementHeader.Schema.BM_AdditionalDeclarationType)).Width);
					AssertEquals("DepartureAtTransportID", 137, grid.GetColumnStyle(ConcatMovementHeaderWithColumnName(nameof(NctsDepartureMovementHeader.TransportAtDeparture))).Width);
					AssertEquals("TypeOfSecurity", 136, grid.GetColumnStyle(ConcatMovementHeaderWithColumnName(NctsDepartureMovementHeader.Schema.BM_TypeOfSecurity)).Width);
				});
			}

			string ConcatMovementHeaderWithColumnName(ZString columnName) => nameof(NctsHeader.MovementHeader) + "+" + columnName;
		}

		[RequiresSTA]
		public void TestArrivalMovementGridColumnWidths()
		{
			using (var filterControl = new NctsMovementFilterControl(new NctsHeaderCollection(Factory), new NctsMovementFilterStripBusinessObject()))
			{
				var grid = filterControl.Grid;
				CombineAssertions(() =>
				{
					AssertEquals("ArrivalStatus", 57, grid.GetColumnStyle(ConcatMovementHeaderWithColumnName(NctsArrivalMovementHeader.Schema.BM_CustomsStatus)).Width);
					AssertEquals("ArrivalStatusDescription", 155, grid.GetColumnStyle(ConcatMovementHeaderWithColumnName(NctsArrivalMovementHeader.Schema.ArrivalStatusDescription)).Width);
					AssertEquals("ArrivalDate", 80, grid.GetColumnStyle(ConcatMovementHeaderWithColumnName(NctsArrivalMovementHeader.Schema.BM_ArrivalDate)).Width);
				});
			}

			string ConcatMovementHeaderWithColumnName(ZString columnName) => nameof(NctsHeader.ArrivalMovementHeader) + "+" + columnName;
		}

		[RequiresSTA]
		public void TestCommonMovementGridColumnWidths()
		{
			using (var filterControl = new NctsMovementFilterControl(new NctsHeaderCollection(Factory), new NctsMovementFilterStripBusinessObject()))
			{
				var grid = filterControl.Grid;
				CombineAssertions(() =>
				{
					AssertEquals("PhaseStatus", 57, grid.GetColumnStyle(ConcatMovementHeaderWithColumnName(NctsArrivalMovementHeader.Schema.BM_Phase)).Width);
					AssertEquals("PhaseStatusDescription", 155, grid.GetColumnStyle(ConcatMovementHeaderWithColumnName(NctsCommonMovementHeader.Schema.PhaseStatusDescription)).Width);
					AssertEquals("AcceptanceDate", 109, grid.GetColumnStyle(ConcatMovementHeaderWithColumnName(NctsCommonMovementHeader.Schema.BM_EntryDate)).Width);
				});
			}

			string ConcatMovementHeaderWithColumnName(ZString columnName) => nameof(NctsHeader.CommonMovementHeader) + "+" + columnName;
		}

		[RequiresSTA]
		public void TestJobDocAddressAndOrgHeaderGridColumnWidths()
		{
			using (var filterControl = new NctsMovementFilterControl(new NctsHeaderCollection(Factory), new NctsMovementFilterStripBusinessObject()))
			{
				var grid = filterControl.Grid;
				CombineAssertions(() =>
				{
					AssertEquals("Consignee.CompanyName", 73, grid.GetColumnStyle($"{nameof(NctsHeader.Consignee)}+{nameof(NctsHeader.Consignee.CompanyName)}").Width);
					AssertEquals("Consignee.Organisation.OH_Code", 101, grid.GetColumnStyle($"{nameof(NctsHeader.Consignee)}+{nameof(NctsHeader.Consignee.Organisation)}+{nameof(NctsHeader.Consignee.Organisation.OH_Code)}").Width);
					AssertEquals("Consignor.CompanyName", 71, grid.GetColumnStyle($"{nameof(NctsHeader.Consignor)}+{nameof(NctsHeader.Consignor.CompanyName)}").Width);
					AssertEquals("Consignor.Organisation.OH_Code", 99, grid.GetColumnStyle($"{nameof(NctsHeader.Consignor)}+{nameof(NctsHeader.Consignor.Organisation)}+{nameof(NctsHeader.Consignor.Organisation.OH_Code)}").Width);
					AssertEquals("Principal.CompanyName", 64, grid.GetColumnStyle($"{nameof(NctsHeader.Principal)}+{nameof(NctsHeader.Principal.CompanyName)}").Width);
					AssertEquals("Principal.Organisation.OH_Code", 91, grid.GetColumnStyle($"{nameof(NctsHeader.Principal)}+{nameof(NctsHeader.Principal.Organisation)}+{nameof(NctsHeader.Principal.Organisation.OH_Code)}").Width);
				});
			}
		}

		[RequiresSTA]
		public void TestBranchGridColumnWidths()
		{
			using (var filterControl = new NctsMovementFilterControl(new NctsHeaderCollection(Factory), new NctsMovementFilterStripBusinessObject()))
			{
				var grid = filterControl.Grid;
				CombineAssertions(() =>
				{
					AssertEquals("DeclarationBranch", 115, grid.GetColumnStyle($"{nameof(NctsHeader.Branch)}+{nameof(NctsHeader.Branch.GB_BranchName)}").Width);
				});
			}
		}

		[RequiresSTA]
		public void TestHoldReasonColumn()
		{
			using (var filterControl = new NctsMovementFilterControl(new NctsHeaderCollection(Factory), new NctsMovementFilterStripBusinessObject()))
			{
				var holdReason = nameof(NctsHeader.Job) + "+" + nameof(NctsHeader.Job.JH_HoldReason);
				var columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
										.Cast<ZGridColumnInfo>()
										.Any(col => col.ColumnName == holdReason && !col.IsVisible);

				Assert("Hold Reason column should exist and should NOT be visible.", columnExistsAndNotVisible);
			}
		}

		[RequiresSTA]
		public void TestBillingJobStatusGridColumn()
		{
			using (var filterControl = new NctsMovementFilterControl(new NctsHeaderCollection(Factory), new NctsMovementFilterStripBusinessObject()))
			{
				var jobStatusColumnStyle = filterControl.Grid.GetColumnStyle("Job+JH_Status");
				AssertNotNull("Job+JH_Status Column", jobStatusColumnStyle);

				CombineAssertions(() =>
				{
					AssertEquals("Caption", "Job Status", jobStatusColumnStyle.CaptionResourceString.Caption);
					AssertEquals("CharacterCasing", CharacterCasing.Upper, jobStatusColumnStyle.CharacterCasing);
					AssertEquals("IsVisible", false, jobStatusColumnStyle.IsVisible);
					AssertEquals("Width", 150, jobStatusColumnStyle.Width);
				});
			}
		}

		[RequiresSTA]
		public void TestAdditionalIdentifierGridColumn()
		{
			using (var filterControl = new NctsMovementFilterControl(new NctsHeaderCollection(Factory), new NctsMovementFilterStripBusinessObject()))
			{
				var columnStyle = filterControl.Grid.GetColumnStyle($"{nameof(CusGoodsLocation)}+{nameof(CusGoodsLocation.CGL_AdditionalIdentifier)}");

				CombineAssertions(() =>
				{
					AssertNotNull(nameof(CusGoodsLocation.CGL_AdditionalIdentifier), columnStyle);
					AssertEquals("CharacterCasing", CharacterCasing.Upper, columnStyle.CharacterCasing);
					AssertEquals("IsVisible", expected: false, columnStyle.IsVisible);
					AssertEquals("Width", 110, columnStyle.Width);
				});
			}
		}

		[RequiresSTA]
		public void TestAdditionalIdentifierDescriptionGridColumn()
		{
			using (var filterControl = new NctsMovementFilterControl(new NctsHeaderCollection(Factory), new NctsMovementFilterStripBusinessObject()))
			{
				var columnStyle = filterControl.Grid.GetColumnStyle($"{nameof(CusGoodsLocation)}+{nameof(CusGoodsLocation.AdditionalIdentifierDescription)}");

				CombineAssertions(() =>
				{
					AssertNotNull(nameof(CusGoodsLocation.AdditionalIdentifierDescription), columnStyle);
					AssertEquals("IsVisible", expected: false, columnStyle.IsVisible);
					AssertEquals("Width", 180, columnStyle.Width);
				});
			}
		}

		[RequiresSTA]
		public void TestWarehouseTransactionStatusGridColumn()
		{
			using var filterControl = new NctsMovementFilterControl(new NctsHeaderCollection(Factory), new NctsMovementFilterStripBusinessObject());
			var columnStyle = filterControl.Grid.GetColumnStyle($"{nameof(NctsHeader.MovementHeader)}+{nameof(NctsHeader.MovementHeader.BM_WarehouseTransactionStatus)}");

			CombineAssertions(() =>
			{
				AssertNotNull(nameof(NctsHeader.MovementHeader.BM_WarehouseTransactionStatus), columnStyle);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnStyle.CharacterCasing);
				AssertEquals("IsVisible", expected: false, columnStyle.IsVisible);
				AssertEquals("Width", 110, columnStyle.Width);
			});
		}

		[RequiresSTA]
		public void TestWarehouseTransactionStatusDescriptionGridColumn()
		{
			using var filterControl = new NctsMovementFilterControl(new NctsHeaderCollection(Factory), new NctsMovementFilterStripBusinessObject());
			var columnStyle = filterControl.Grid.GetColumnStyle($"{nameof(NctsHeader.MovementHeader)}+{nameof(NctsHeader.MovementHeader.WarehouseTransactionStatusDescription)}");

			CombineAssertions(() =>
			{
				AssertNotNull(nameof(NctsHeader.MovementHeader.WarehouseTransactionStatusDescription), columnStyle);
				AssertEquals("IsVisible", expected: false, columnStyle.IsVisible);
				AssertEquals("Width", 200, columnStyle.Width);
			});
		}

		[RequiresSTA]
		public void TestWorkflowCustomFieldColums()
		{
			NctsMoveCustomFieldHelper.CreateNCTSPhase5WorkflowWithCustomFields(Factory);
			using (var control = new NctsMovementFilterControl(new NctsHeaderCollection(Factory), new NctsMovementFilterStripBusinessObject()))
			{
				string[] workflowColumns = control.FilteredGrid.ColumnStyles.Cast<ZGridColumnInfo>()
					.Where(col => !string.IsNullOrEmpty(col.Caption) && col.Caption.StartsWith("Custom ", StringComparison.Ordinal))
					.Select(col => col.Caption).ToArray();

				AssertContainsExactElementsInAnyOrder(new[] { "Custom Header string", "Custom ArrivalHeader string", "Custom DepartureHeader string" }, workflowColumns);
			}
		}
	}
}
