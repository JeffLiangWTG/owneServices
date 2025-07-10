using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	public class APDraftInvoiceLinkColumnStyleTest : TestCaseWithFactory
	{
		public void TestAPDraftInvoiceLinkColumnStyleInfo()
		{
			var styleInfo = new APDraftInvoiceLinkColumnStyleInfo();
			AssertEquals("The column style has an expected type", typeof(APDraftInvoiceLinkColumnStyle), styleInfo.ColumnStyleType);
		}

		public void TestAPDraftInvoiceLinkColumnStyle_RenderedCell()
		{
			var dummy = new BusinessObjectFactory().NewWithValidTestData<DummyBusinessObject>();
			using (var form = new ZForm(dummy))
			using (var grid = new ZGrid { Location = new Point(0, 0), Size = new Size(200, 200) })
			{
				form.Controls.Add(grid);
				var columnStyleInfo = new APDraftInvoiceLinkColumnStyleInfoForTestOnly { ColumnName = DummyBizoSchema.Constants.Z0_Code };
				grid.ColumnStyles.Add(columnStyleInfo);
				dummy.Collection.AddNew();
				dummy.Collection.AddNew();
				dummy.Collection[0].Z0_Code = "123";
				dummy.Collection[1].Z0_Code = "456";
				grid.SetDataBinding(dummy, "Collection", dummy.Collection.GetType().Name);

				var columnStyle = grid.Columns[0].ColumnStyle as APDraftInvoiceLinkColumnStyleForTestOnly;
				AssertNotNull(columnStyle);
				columnStyle.PropertyDescriptor = TypeDescriptor.GetProperties(typeof(DummyChildBusinessObject)).OfType<PropertyDescriptor>().Single(d => d.DisplayName == DummyBizoSchema.Constants.Z0_Code);

				grid.Select(0);
#if WINZOR
				var cellStyleString = columnStyle.GetCellStyleString(grid.ListManager, 0);
				AssertContains("text-decoration: underline;", cellStyleString);
				AssertContains("color: Blue;", cellStyleString);
				AssertContains("cursor: pointer;", cellStyleString);
#else
				columnStyle.Paint(grid.CreateGraphics(), new Rectangle(0, 0, 2000, 2000), grid.ListManager, 0, SystemBrushes.Control, SystemBrushes.ControlText, false);
				AssertEquals(((SolidBrush)SystemBrushes.Control).Color, ((SolidBrush)columnStyle.BackBrush).Color);
				AssertEquals(Color.Blue, ((SolidBrush)columnStyle.ForeBrush).Color);
				AssertEquals("123", columnStyle.CellText);
				AssertEquals(new Font(grid.Font, FontStyle.Underline), columnStyle.CellFont);

				columnStyle.Paint(grid.CreateGraphics(), new Rectangle(0, 0, 2000, 2000), grid.ListManager, 1, SystemBrushes.Control, SystemBrushes.ControlText, false);
				AssertEquals(SystemDataRegistry.Instance.ColorTheme.GridReadOnlyColor, ((SolidBrush)columnStyle.BackBrush).Color);
				AssertEquals(Color.Blue, ((SolidBrush)columnStyle.ForeBrush).Color);
				AssertEquals("456", columnStyle.CellText);
				AssertEquals(new Font(grid.Font, FontStyle.Underline), columnStyle.CellFont);
#endif
			}
		}

		#region Testable Classes

		class APDraftInvoiceLinkColumnStyleForTestOnly : APDraftInvoiceLinkColumnStyle
		{
			public APDraftInvoiceLinkColumnStyleForTestOnly(APDraftInvoiceLinkColumnStyleInfo info)
				: base(info)
			{
			}

#if WINZOR
			public new string GetCellStyleString(CurrencyManager source, int rowNum)
			{
				return base.GetCellStyleString(source, rowNum);
			}
#endif

#if !WINZOR
			public new void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int paintingRowNum, Brush backBrush, Brush foreBrush, bool alignedToRight)
			{
				base.Paint(g, bounds, source, paintingRowNum, backBrush, foreBrush, alignedToRight);
			}

			protected override void PaintText(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, string cellText, Font cellFont, Brush backBrush, Brush foreBrush, bool rightToLeft)
			{
				this.BackBrush = backBrush;
				this.ForeBrush = foreBrush;
				this.CellText = cellText;
				this.CellFont = cellFont;
			}

			public Brush BackBrush;
			public Brush ForeBrush;
			public string CellText;
			public Font CellFont;
#endif
		}

		class APDraftInvoiceLinkColumnStyleInfoForTestOnly : APDraftInvoiceLinkColumnStyleInfo
		{
			public override Type ColumnStyleType
			{
				get { return typeof(APDraftInvoiceLinkColumnStyleForTestOnly); }
			}
		}

		#endregion
	}
}
