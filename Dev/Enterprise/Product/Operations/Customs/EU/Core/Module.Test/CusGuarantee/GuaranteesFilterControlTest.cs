using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Module.Testing
{
	[TestedType(typeof(GuaranteesFilterControl))]
	sealed class GuaranteesFilterControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestGuaranteeAmountColumn()
		{
			using (var form = new ZForm())
			using (var filterControl = GetNewFilterControl())
			{
				form.Controls.Add(filterControl);
				form.Show();

				var column = filterControl.Grid.GetColumnStyle(nameof(BaseCusPermitHeader.CPH_Calc_OpeningBalance));
				CombineAssertions(() =>
				{
					AssertType<ZCalcEditColumnStyleInfo>("Column type is correct", column);
					AssertEquals("Column is visible", true, column.IsVisible);
					AssertEquals("Column caption is correct", "Guarantee Amount", column.CaptionResourceString.Caption);
				});
			}
		}

		[RequiresSTA]
		public void TestGridDefaultColumnOrder()
		{
			using (var form = new ZForm())
			using (var filterControl = GetNewFilterControl())
			{
				form.Controls.Add(filterControl);
				form.Show();

				AssertSequencesEqual(OrderedColumnDetails.Select(x => x.ColumnName), filterControl.Grid.DefaultColumns.Where(x => x.IsVisible).Select(x => x.ColumnName));
			}
		}

		[RequiresSTA]
		public void TestNewZFilterStrip()
		{
			using var filterControl = GetNewFilterControl();
			var invocations = new List<(object sender, ZFilterStripEventArgs e)>(1);
			filterControl.FilterStripAdding += OnFilterStripAdding;
			filterControl.ResetFilterStrips();
			filterControl.FilterStripAdding -= OnFilterStripAdding;
			AssertNotNull("OnFilterStripAdding was invoked", invocations.FirstOrDefault());
			var index = 0;
			foreach (var (sender, e) in invocations)
			{
				AssertEquals($"Invocation {index} sender", filterControl, sender);
				AssertType<GuaranteesFilterStrip>($"Invocation {index} e.Strip", e.Strip);
				index++;
			}
			void OnFilterStripAdding(object sender, ZFilterStripEventArgs e)
			{
				invocations.Add((sender, e));
			}
		}

		GuaranteesFilterControl GetNewFilterControl()
		{
			var collection = new CusGuaranteeHeaderCollection(Factory);
			var filterBizO = new GuaranteesFilterStripBusinessObject();
			return new GuaranteesFilterControl(collection, filterBizO);
		}

		static (string ColumnName, string ColumnCaption, int ColumnWidth)[] OrderedColumnDetails => new[]
		{
			(nameof(BaseCusPermitHeader.PermitHolder) + "+" + nameof(OrgHeader.OH_Code), "Permit Holder", 100),
			(CusPermitHeaderSchema.Constants.CPH_RN_NKCountryCode, "Country Code", 100),
			(CusPermitHeaderSchema.Constants.CPH_Number, "Permit Number", 100),
			(CusPermitHeaderSchema.Constants.CPH_StartDate, "Start Date", 100),
			(CusPermitHeaderSchema.Constants.CPH_EndDate, "End Date", 100),
			(CusPermitHeaderSchema.Constants.CPH_Type, "Permit Type", 80),
			(CusPermitHeaderSchema.Constants.CPH_SubType, "Permit Sub Type", 90),
			(CusPermitHeaderSchema.Constants.CPH_QtyValIndicator, "Qty/Val Indicator", 100),
			(nameof(BaseCusPermitHeader.CPH_Calc_OpeningBalance), "Guarantee Amount", 110),
			(nameof(BaseCusPermitHeader.ValueBalance), "Remaining Value", 110),
			(nameof(BaseCusPermitHeader.QuantityBalance), "Remaining Quantity", 110),
			(nameof(BaseCusPermitHeader.AuthLatestValueBalanceWithMsg), "Auth. Latest Val", 110),
			(nameof(BaseCusPermitHeader.AuthLatestQuantityBalanceWithMsg), "Auth. Latest Qty", 90),
			(CusPermitHeaderSchema.Constants.CPH_UnitOfMeasure, "Unit of Measure", 110),
			(CusPermitHeaderSchema.Constants.CPH_IsClosed, "Closed", 110)
		};
	}
}
