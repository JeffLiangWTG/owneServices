using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using CusEntryHeader = Enterprise.Customs.AsycudaCustoms.Business.CusEntryHeader;

namespace Enterprise.Customs.AsycudaCustoms.Module.Testing
{
	class EntryHeaderFilterUserControlTest : Customs.Module.Testing.EntryHeaderFilterUserControlTest
	{
		public void TestRiskManagementColumns_RiskManagementEnabled()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.Risk, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Now, true))
			using (var form = new ZForm())
			using (var filterStrip = GetNewEntryHeaderFilterUserControl())
			{
				CombineAssertions(() =>
				{
					form.Controls.Add(filterStrip);
					form.Show();
					var grid = filterStrip.FilteredGrid;
					var remainingCustomsValue = (ZArchitecture.ZCalcEditColumnStyleInfo)grid.GetColumnStyle(EntryHeaderFilterUserControl.ColumnNames.RemainingCustomsValue);
					var remainingWeight = (ZArchitecture.ZCalcEditColumnStyleInfo)grid.GetColumnStyle(EntryHeaderFilterUserControl.ColumnNames.RemainingWeight);
					var remainingCustomsQuantity = (ZArchitecture.ZCalcEditColumnStyleInfo)grid.GetColumnStyle(EntryHeaderFilterUserControl.ColumnNames.RemainingCustomsQuantity);
					AssertEquals("RemainingCustomsValue availability", false, remainingCustomsValue.IsUnavailable);
					AssertEquals("RemainingCustomsValue visibility", true, remainingCustomsValue.IsVisible);
					AssertEquals("RemainingWeight availability", false, remainingWeight.IsUnavailable);
					AssertEquals("RemainingWeight visibility", true, remainingWeight.IsVisible);
					AssertEquals("RemainingCustomsQuantity availability", false, remainingCustomsQuantity.IsUnavailable);
					AssertEquals("RemainingCustomsQuantity visibility", true, remainingCustomsQuantity.IsVisible);
				});
			}
		}

		public void TestRiskManagementColumns_RiskManagementNotEnabled()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.Risk, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Now, false))
			using (var form = new ZForm())
			using (var filterStrip = GetNewEntryHeaderFilterUserControl())
			{
				CombineAssertions(() =>
				{
					form.Controls.Add(filterStrip);
					form.Show();
					var grid = filterStrip.FilteredGrid;
					AssertNull(grid.GetColumnStyle(EntryHeaderFilterUserControl.ColumnNames.RemainingCustomsValue));
					AssertNull(grid.GetColumnStyle(EntryHeaderFilterUserControl.ColumnNames.RemainingWeight));
					AssertNull(grid.GetColumnStyle(EntryHeaderFilterUserControl.ColumnNames.RemainingCustomsQuantity));
				});
			}
		}

		public void TestAcquittedAndAcquitByDateProperties()
		{
			using (var form = new ZForm())
			using (var filterStrip = GetNewEntryHeaderFilterUserControl())
			{
				CombineAssertions(() =>
				{
					form.Controls.Add(filterStrip);
					form.Show();
					var grid = filterStrip.FilteredGrid;
					var acquitBy = (ZArchitecture.ZDateEditColumnStyleInfo)grid.GetColumnStyle("CH_BondValidToDate");
					var acquitted = (ZArchitecture.ZDateEditColumnStyleInfo)grid.GetColumnStyle("CH_BondAcquittedDate");
					AssertEquals(ZArchitecture.Core.ZDateTimePickerFormat.Short, acquitBy.DateTimeFormat);
					AssertEquals(ZArchitecture.Core.ZDateTimePickerFormat.Short, acquitted.DateTimeFormat);
					AssertEquals(true, acquitBy.IsVisible);
					AssertEquals(true, acquitted.IsVisible);
				});
			}
		}

		public void TestEntryHeaderFilteredGridColumns()
		{
			using ((ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.Customs.EntryHeader))
			{
				var declaration = Factory.New<JobDeclaration>();
				var entries = new CusEntryHeaderCollection<CusEntryHeader>(declaration, Factory);
				var filterBusinessObject = new EntryHeaderFilterBusinessObject();
				using (var form = new ZForm())
				{
					using (var filter = new EntryHeaderFilterUserControl(entries, filterBusinessObject))
					{
						form.Controls.Add(filter);
						form.Show();
						var filteredGrid = filter.FilteredGrid;
						var continuousGuarantee = filteredGrid.GetColumnStyle(EntryHeaderFilterUserControl.ColumnNames.IsContinuousGuarantee);
						var linkedGuarantee = filteredGrid.GetColumnStyle(EntryHeaderFilterUserControl.ColumnNames.LinkedGuarantee);
						var guaranteedAmount = filteredGrid.GetColumnStyle(EntryHeaderFilterUserControl.ColumnNames.GuaranteeAmount);
						var guaranteeActivity = filteredGrid.GetColumnStyle(EntryHeaderFilterUserControl.ColumnNames.GuaranteeActivity);
						var guaranteeStatus = filteredGrid.GetColumnStyle(EntryHeaderFilterUserControl.ColumnNames.GuaranteeStatus);
						var transitPermitCount = filteredGrid.GetColumnStyle(EntryHeaderFilterUserControl.ColumnNames.TransitPermitCount);
						var transitPermitInTransitCount = filteredGrid.GetColumnStyle(EntryHeaderFilterUserControl.ColumnNames.TransitPermitInTransitCount);
						var expired = filteredGrid.GetColumnStyle(EntryHeaderFilterUserControl.ColumnNames.TransitExpired);
						var earliestExpiryDate = filteredGrid.GetColumnStyle(EntryHeaderFilterUserControl.ColumnNames.TransitEarliestExpiryDate);
						var completed = filteredGrid.GetColumnStyle(EntryHeaderFilterUserControl.ColumnNames.TransitCompleted);
						AssertEquals("Continuous Guarantee", continuousGuarantee.CaptionResourceString.Caption);
						AssertEquals("Linked Guarantee", linkedGuarantee.CaptionResourceString.Caption);
						AssertEquals("Guarantee Amount", guaranteedAmount.CaptionResourceString.Caption);
						AssertEquals("Guarantee Activity", guaranteeActivity.CaptionResourceString.Caption);
						AssertEquals("Guarantee Status", guaranteeStatus.CaptionResourceString.Caption);
						AssertEquals("No. of Transit Permits", transitPermitCount.CaptionResourceString.Caption);
						AssertEquals("No. still in Transit", transitPermitInTransitCount.CaptionResourceString.Caption);
						AssertEquals("Transit Expired", expired.CaptionResourceString.Caption);
						AssertEquals("Transit Earliest Expiry Date", earliestExpiryDate.CaptionResourceString.Caption);
						AssertEquals("Transit Completed", completed.CaptionResourceString.Caption);
					}
				}
			}
		}

		protected override Customs.Module.EntryHeaderFilterUserControl GetNewEntryHeaderFilterUserControl()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var cusEntryHeaders = new CusEntryHeaderCollection<CusEntryHeader>(jobDeclaration, Factory);
			var filterBusinessObject = new EntryHeaderFilterBusinessObject();
			return new EntryHeaderFilterUserControl(cusEntryHeaders, filterBusinessObject);
		}

		protected override List<string> FilteredGridColumns
		{
			get
			{
				var list = new List<string>();
				list.AddRange(base.FilteredGridColumns);
				list.AddRange(new List<string>
				{
					EntryHeaderFilterUserControl.ColumnNames.IsContinuousGuarantee,
					EntryHeaderFilterUserControl.ColumnNames.LinkedGuarantee,
					EntryHeaderFilterUserControl.ColumnNames.GuaranteeActivity,
					EntryHeaderFilterUserControl.ColumnNames.GuaranteeStatus,
					EntryHeaderFilterUserControl.ColumnNames.GuaranteeAmount,
					CusEntryHeader.Schema.CH_BondValidToDate,
					CusEntryHeader.Schema.CH_BondAcquittedDate,
					EntryHeaderFilterUserControl.ColumnNames.TransitPermitCount,
					EntryHeaderFilterUserControl.ColumnNames.TransitPermitInTransitCount,
					EntryHeaderFilterUserControl.ColumnNames.TransitExpired,
					EntryHeaderFilterUserControl.ColumnNames.TransitEarliestExpiryDate,
					EntryHeaderFilterUserControl.ColumnNames.TransitCompleted
				});
				return list;
			}
		}

		protected override List<string> ColumnNamesInSortOrder => new List<string>
		{
			CusEntryHeader.Schema.EntryNumber,
			CusEntryHeader.Schema.DeclarationReference,
			CusEntryHeader.Schema.CH_BGMReference,
			CusEntryHeader.Schema.CH_EntryStatus,
			CusEntryHeader.Schema.EntryHeaderStatusDescription,
			CusEntryHeader.Schema.CH_EntrySubmittedDate,
			CusEntryHeader.Schema.CH_EntryReleaseDate,
			CusEntryHeader.Schema.CH_Status,
			CusEntryHeader.Schema.MessageStatusDescription,
			CusEntryHeader.Schema.CH_MessageType,
			CusEntryHeader.Schema.CH_MessageTypeDescription,
			EntryHeaderFilterUserControl.Schema.BranchName,
			EntryHeaderFilterUserControl.Schema.ImporterName,
			EntryHeaderFilterUserControl.Schema.SupplierName,
			EntryHeaderFilterUserControl.Schema.AgentsReference,
			EntryHeaderFilterUserControl.Schema.DateOfArrival,
			CusEntryHeader.Schema.CH_TotalPaid,
			CusEntryHeader.Schema.CH_BondValidToDate,
			CusEntryHeader.Schema.CH_BondAcquittedDate,
			CusEntryHeader.Schema.CH_WarehouseTransactionStatus,
			CusEntryHeader.Schema.CH_WarehouseTransactionStatusDescription,
			EntryHeaderFilterUserControl.ColumnNames.IsContinuousGuarantee,
			EntryHeaderFilterUserControl.ColumnNames.LinkedGuarantee,
			EntryHeaderFilterUserControl.ColumnNames.GuaranteeActivity,
			EntryHeaderFilterUserControl.ColumnNames.GuaranteeStatus,
			EntryHeaderFilterUserControl.ColumnNames.GuaranteeAmount,
			EntryHeaderFilterUserControl.ColumnNames.TransitPermitCount,
			EntryHeaderFilterUserControl.ColumnNames.TransitPermitInTransitCount,
			EntryHeaderFilterUserControl.ColumnNames.TransitExpired,
			EntryHeaderFilterUserControl.ColumnNames.TransitEarliestExpiryDate,
			EntryHeaderFilterUserControl.ColumnNames.TransitCompleted
		};
	}
}
