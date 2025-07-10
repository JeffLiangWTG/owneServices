using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.Customs.DE.Messaging;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.DE.GUI.Testing
{
	sealed class CHGBaseDeclarationUserControlTest : TestCaseWithFactory
	{
		public void TestDecsGridDock()
		{
			using (var chgoffDeclarationUserControl = new CHGOFFDeclarationUserControl())
			{
				AssertEquals("DecsGrid is docked as Fill", DockStyle.Fill, chgoffDeclarationUserControl.DecsGrid.Dock);
			}
		}

		public void TestDecsGridColumnSizes()
		{
			using (var chgoffDeclarationUserControl = new CHGOFFDeclarationUserControl())
			{
				var decsGrid = chgoffDeclarationUserControl.DecsGrid;
				CombineAssertions(() =>
				{
					AssertEquals("Type is the correct size", 62, decsGrid.GetColumnStyle(CusTempStorageDec.Schema.STH_IdentificationIndicator).Width);
					AssertEquals("Create Time is the correct size", 95, decsGrid.GetColumnStyle(CusTempStorageDec.Schema.STH_SystemCreateTimeUtc).Width);
					AssertEquals("Message Status is the correct size", 80, decsGrid.GetColumnStyle(CusTempStorageDec.Schema.STH_MessageStatus).Width);
					AssertEquals("Owner Reference is the correct size", 187, decsGrid.GetColumnStyle(CusTempStorageDec.Schema.FormattedOwnerReferenceNumber).Width);
				});
			}
		}

		public void TestLineNumberColumnProperties()
		{
			using (var form = new ZForm(header))
			using (var chgoffDeclarationUserControl = new CHGOFFDeclarationUserControl())
			{
				var storageDec = header.CHGOFFCusTempStorageDecs.AddNew();
				chgoffDeclarationUserControl.Dock = DockStyle.Fill;
				form.Controls.Add(chgoffDeclarationUserControl);
				form.SetDataBinding(header, ".");
				form.Show();
				storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
				var linesGrid = (ZGrid)chgoffDeclarationUserControl.Controls.Find("LinesGrid", true).Single();
				var columnName = CusTempStorageLine.Schema.TSL_LineNo;
				AssertStartsWith("Caption for Line No.", "Reference Line No.", linesGrid.GetColumnCaption(columnName));
				AssertEquals("Column Width for Line No.", 105, linesGrid.GetColumnWidth(columnName));
			}
		}

		public void TestFormattedOwnerReferenceNumberColumn_Properties()
		{
			using (var chgoffDeclarationUserControl = new CHGOFFDeclarationUserControl())
			{
				var decsGrid = chgoffDeclarationUserControl.DecsGrid;
				CombineAssertions(() =>
				{
					var style = (ZMultiControlColumnStyleInfo)decsGrid.GetColumnStyle(CusTempStorageDec.Schema.FormattedOwnerReferenceNumber);
					AssertEquals("BindToList", nameof(CusTempStorageDec.Lookups) + "." + nameof(CusTempStorageDecLookups.CusTempStorageRegLineCollection), style.BindToList);
					AssertEquals("ModuleID", ModuleIDs.Customs.EU.DE.ImportFromSumARegister, style.ModuleID);
					AssertEquals("ColumnName", nameof(CusTempStorageDec.Schema.FormattedOwnerReferenceNumber), style.ColumnName);
					AssertEquals("FieldTypeColumnName", nameof(CusTempStorageDec.ReferenceNumberColumnFieldType), style.FieldTypeColumnName);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusTempStorageJobHeader>();
		}
		CusTempStorageJobHeader header;
	}
}
