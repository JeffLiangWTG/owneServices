using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.Customs.DE.Messaging;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.DE.GUI.Testing
{
	sealed class CHGSPODeclarationUserControlTest : TestCaseWithFactory
	{
		public void TestNewOwnerReferenceTypeDropEdit()
		{
			using (var form = new ZForm())
			using (var control = new CHGSPODeclarationUserControl())
			{
				control.SetDataBinding(storageJobHeader, "");
				form.Controls.Add(control);
				form.Show();
				var newOwnerReferenceTypeDropEdit = control.NewOwnerReferenceTypeDropEdit;
				AssertEquals("BindingMember", nameof(CusTempStorageJobHeader.CHGSPOCusTempStorageDecs) + "." + nameof(CHGSPOCusTempStorageDec.CusTempStorageLines) + "." + nameof(CHGSPOCusTempStorageLine.TSL_OwnerReferenceType), newOwnerReferenceTypeDropEdit.GetBindingMember());
			}
		}

		public void TestNewOwnerReferenceNoTextBox()
		{
			using (var form = new ZForm())
			using (var control = new CHGSPODeclarationUserControl())
			{
				control.SetDataBinding(storageJobHeader, "");
				form.Controls.Add(control);
				form.Show();
				var newOwnerReferenceNoTextBox = control.NewOwnerReferenceNoTextBox;
				AssertEquals("BindingMember", nameof(CusTempStorageJobHeader.CHGSPOCusTempStorageDecs) + "." + nameof(CHGSPOCusTempStorageDec.CusTempStorageLines) + "." + nameof(CHGSPOCusTempStorageLine.TSL_OwnerReferenceNumber), newOwnerReferenceNoTextBox.GetBindingMember());
			}
		}

		public void TestDeleteDeclarationsGrid()
		{
			storageDec.STH_MessageStatus = "SNT";
			using (var form = new ZForm())
			using (var control = new CHGSPODeclarationUserControl())
			{
				control.SetDataBinding(storageJobHeader, "");
				form.Controls.Add(control);
				form.Show();
				var declarationsGrid = control.DeclarationsGrid;
				declarationsGrid.SelectSingleElement(storageDec);
				declarationsGrid.DeleteMenuItem.PerformClick();
				Assert(!storageDec.IsDeleted);
				storageDec.STH_MessageStatus = "";
				declarationsGrid.DeleteMenuItem.PerformClick();
				Assert(storageDec.IsDeleted);
			}
		}

		public void TestDeleteLinesGrid()
		{
			var storageLine = storageDec.CusTempStorageLines.AddNew();
			storageDec.STH_MessageStatus = Common.Shared.MessageStatusList.Codes.Sent;

			using (var form = new ZForm())
			using (var control = new CHGSPODeclarationUserControl())
			{
				control.SetDataBinding(storageJobHeader, ZString.Empty);
				form.Controls.Add(control);
				form.Show();
				var declarationsGrid = control.DeclarationsGrid;
				declarationsGrid.SelectSingleElement(storageDec);
				var linesGrid = control.LinesGrid;
				linesGrid.SelectSingleElement(storageLine);
				linesGrid.DeleteMenuItem.PerformClick();
				Assert(!storageLine.IsDeleted);
				storageDec.STH_MessageStatus = ZString.Empty;
				linesGrid.DeleteMenuItem.PerformClick();
				Assert(storageLine.IsDeleted);
			}
		}

		public void TestLinesAndMessagesTabPages()
		{
			using (var form = new ZForm())
			using (var control = new CHGSPODeclarationUserControl())
			{
				control.SetDataBinding(storageJobHeader, "");
				form.Controls.Add(control);
				form.Show();
				var linesTabPage = control.LinesTabPage;
				AssertEquals("Lines", linesTabPage.CaptionResourceString.Caption);
				var messagesTabPage = control.MessagesTabPage;
				AssertEquals("Messages", messagesTabPage.CaptionResourceString.Caption);
			}
		}

		public void TestFormattedOwnerReferenceNumberColumn_Properties()
		{
			using (var control = new CHGSPODeclarationUserControl())
			{
				var decsGrid = control.DeclarationsGrid;

				CombineAssertions(() =>
				{
					var style = (ZMultiControlColumnStyleInfo)decsGrid.GetColumnStyle(CusTempStorageDec.Schema.FormattedOwnerReferenceNumber);
					AssertEquals(nameof(CusTempStorageDec.Lookups) + "." + nameof(CusTempStorageDecLookups.CusTempStorageRegLineCollection), style.BindToList);
					AssertEquals(ModuleIDs.Customs.EU.DE.ImportFromSumARegister, style.ModuleID);
					AssertEquals(nameof(CusTempStorageDec.Schema.FormattedOwnerReferenceNumber), style.ColumnName);
					AssertEquals("FieldTypeColumnName", nameof(CHGSPOCusTempStorageDec.ReferenceNumberColumnFieldType), style.FieldTypeColumnName);
				});
			}
		}

		public void TestTSL_LineNoCaption()
		{
			using (var form = new ZForm(storageJobHeader))
			using (var chgspoDeclarationUserControl = new CHGSPODeclarationUserControl())
			{
				chgspoDeclarationUserControl.Dock = DockStyle.Fill;
				form.Controls.Add(chgspoDeclarationUserControl);
				form.SetDataBinding(storageJobHeader, ".");
				form.Show();
				var linesGrid = (ZGrid)chgspoDeclarationUserControl.Controls.Find("LinesGrid", true).First();
				AssertEquals("Reference Line No." ,linesGrid.GetColumnCaption(CusTempStorageLine.Schema.TSL_LineNo));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			storageJobHeader = Factory.New<CusTempStorageJobHeader>();
			storageDec = storageJobHeader.CHGSPOCusTempStorageDecs.AddNew();
			storageDec.STH_DeclarationType = TemporaryStorageDeclarationTypeList.Codes.ChangeOfSpecificOrderTerm;
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
		}

		CusTempStorageJobHeader storageJobHeader;
		CHGSPOCusTempStorageDec storageDec;
	}
}
