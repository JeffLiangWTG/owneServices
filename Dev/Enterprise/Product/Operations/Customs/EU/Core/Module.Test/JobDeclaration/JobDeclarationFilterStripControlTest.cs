using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Module.Testing
{
	public class JobDeclarationFilterStripControlTest : Customs.Module.Testing.JobDeclarationFilterStripControlTest
	{
		[RequiresSTA]
		public void TestAdditionalColumnsExist()
		{
			var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var filterBO = new JobDeclarationFilterBusinessObject();

			using (var module = new JobDeclarationModule())
			using (var userControl = new JobDeclarationFilterStripControl(module, declarations, filterBO))
			{
				var grid = userControl.FilteredGrid;
				CombineAssertions(() =>
				{
					AssertColumnStyle<ZTextBoxColumnStyleInfo>(grid.GetColumnStyle(JobDeclaration.Schema.PackTypes), "Pack Types", 114);
					AssertColumnStyle<ZTextBoxColumnStyleInfo>(grid.GetColumnStyle(JobDeclaration.Schema.JE_LocationOfGoods), "Location Of Goods", 114);
					AssertColumnStyle<ZTextBoxColumnStyleInfo>(grid.GetColumnStyle(JobDeclaration.Schema.ZG_ShipmentType), "Shipment Entry Type", 128);
					AssertColumnStyle<ZTextBoxColumnStyleInfo>(grid.GetColumnStyle(JobDeclaration.Schema.ZG_CTStatusID), "CT Status", 71);
					AssertColumnStyle<ZTextBoxColumnStyleInfo>(grid.GetColumnStyle(JobDeclaration.Schema.JE_MessageSubType), "Entry Style", 80);
					AssertColumnStyle<ZDateEditColumnStyleInfo>(grid.GetColumnStyle(JobDeclaration.Schema.JE_Calc_DateOfClearance), "Clearance Date", 97);
					AssertColumnStyle<ZGuidFindBoxColumnStyleInfo>(grid.GetColumnStyle(JobDeclaration.Schema.JE_OH_ShippingLine), "Carrier", 71);
				});
			}
		}

		[RequiresSTA]
		public void TestExitControlColumns()
		{
			var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var filterBO = new JobDeclarationFilterBusinessObject();

			using (var module = new JobDeclarationModule())
			{
				using (var userControl = new JobDeclarationFilterStripControl(module, declarations, filterBO))
				{
					var grid = userControl.FilteredGrid;
					AssertNull(grid.GetColumnStyle(JobDeclaration.Schema.ExitPresentationStatus));
					AssertNull(grid.GetColumnStyle(JobDeclaration.Schema.ExitPresentationStatusDesc));
				}

				using (var userControl = new JobDeclarationFilterStripControlSupportingExitControlForTest(module, declarations, filterBO))
				{
					var grid = userControl.FilteredGrid;
					AssertColumnStyle<ZTextBoxColumnStyleInfo>(grid.GetColumnStyle(JobDeclaration.Schema.ExitPresentationStatus), "Exit Presentation Status", 128);
					AssertColumnStyle<ZTextBoxColumnStyleInfo>(grid.GetColumnStyle(JobDeclaration.Schema.ExitPresentationStatusDesc), "Exit Presentation Status Desc.", 150);
				}
			}
		}

		[RequiresSTA]
		public void TestSupportsExitControl()
		{
			var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var filterBO = new JobDeclarationFilterBusinessObject();

			using (var module = new JobDeclarationModule())
			using (var userControl = new JobDeclarationFilterStripControl(module, declarations, filterBO))
			{
				Assert(!userControl.SupportsExitControl);
			}
		}

		[RequiresSTA]
		public void TestShouldAddDeclarantFieldsToGrid()
		{
			var declarations = new BaseJobDeclarationCollection(Factory);
			var filterBO = new JobDeclarationFilterBusinessObject();
			using (var userControl = new JobDeclarationFilterStripControl(null, declarations, filterBO))
			{
				var grid = userControl.FilteredGrid;

				AssertNotNull(grid.GetColumnStyle("DeclarantCode"));
				AssertNotNull(grid.GetColumnStyle("DeclarantName"));
			}
		}

		void AssertColumnStyle<T>(ZGridColumnInfo columnInfo, string expectedCaption, int expectedWidth) where T : ZGridColumnInfo
		{
			AssertNotNull($"{expectedCaption} column", columnInfo);
			AssertType<T>($"{expectedCaption} column", columnInfo);
			AssertNull($"{expectedCaption} column caption", columnInfo.Caption);
			AssertEquals($"{expectedCaption} column resource string caption", expectedCaption, columnInfo.CaptionResourceString.Caption);
			AssertEquals($"{expectedCaption} width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(expectedWidth), columnInfo.Width);
		}
	}

	sealed class JobDeclarationFilterStripControlSupportingExitControlForTest : JobDeclarationFilterStripControl
	{
		public JobDeclarationFilterStripControlSupportingExitControlForTest(JobDeclarationModule module, IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject) : base(module, gridCollection, filterStripBusinessObject)
		{
		}

		protected override bool SupportsExitControlCore => true;
	}
}
