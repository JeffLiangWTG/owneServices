using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.Customs.DE.Messaging;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	public class CUSPCSDeclarationUserControlTest : TestCaseWithFactory
	{
		public void TestColumnTSL_GoodsDescription_CharacterCasing()
		{
			using (var control = new CUSPCSDeclarationUserControl())
			{
				var grid = control.FindSingle<ZGrid>("NewSplitLinesGrid");
				AssertEquals(System.Windows.Forms.CharacterCasing.Normal, grid.GetColumnStyle("TSL_GoodsDescription").CharacterCasing);
			}
		}

		public void TestColumnTSL_OwnerReferenceNumber_CharacterCasing()
		{
			using (var control = new CUSPCSDeclarationUserControl())
			{
				var grid = control.FindSingle<ZGrid>("NewSplitLinesGrid");
				AssertEquals(System.Windows.Forms.CharacterCasing.Normal, grid.GetColumnStyle("TSL_OwnerReferenceNumber").CharacterCasing);
			}
		}

		public void TestColumnTSL_DestinationPlace_CharacterCasing()
		{
			using (var control = new CUSPCSDeclarationUserControl())
			{
				var grid = control.FindSingle<ZGrid>("NewSplitLinesGrid");
				AssertEquals(System.Windows.Forms.CharacterCasing.Normal, grid.GetColumnStyle("TSL_DestinationPlace").CharacterCasing);
			}
		}

		public void TestColumnSTH_AdditionalInformation_CharacterCasing()
		{
			using (var control = new CUSPCSDeclarationUserControl())
			{
				var grid = control.FindSingle<ZGrid>("StorageDecGrid");
				AssertEquals(System.Windows.Forms.CharacterCasing.Normal, grid.GetColumnStyle("STH_AdditionalInformation").CharacterCasing);
			}
		}

		public void TestDisableSplitterTabStop()
		{
			using (var form = new ZForm())
			using (var control = new CUSPCSDeclarationUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				Assert(!((KSplitContainer)control.Controls.Find("DecSplitContainer", true).First()).TabStop);
			}
		}

		public void TestLineNoReadonlyForTabSkip()
		{
			using (var form = new ZForm())
			using (var control = new CUSPCSDeclarationUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var newLinesGrid = (ZGrid)control.Controls.Find("NewSplitLinesGrid", true).First();
				var lineNoColumn = newLinesGrid.GetColumnStyle(CusTempStorageLine.Schema.TSL_LineNo);
				Assert(lineNoColumn.IsReadOnly);
			}
		}

		public void TestDeleteDeclarationsGrid()
		{
			storageDec.STH_MessageStatus = "SNT";
			using (var form = new ZForm())
			using (var control = new CUSPCSDeclarationUserControl())
			{
				control.SetDataBinding(storageJobHeader, "");
				form.Controls.Add(control);
				form.Show();
				var declarationsGrid = (ZGrid)control.Controls.Find("StorageDecGrid", true).First();
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
			var storageLine = storageDec.ConsolidatedCusTempStorageLine;
			var storageLineTo = storageLine.CusTempStorageLinesTo.AddNew();
			storageDec.STH_MessageStatus = Common.Shared.MessageStatusList.Codes.Sent;

			using (var form = new ZForm())
			using (var control = new CUSPCSDeclarationUserControl())
			{
				control.SetDataBinding(storageJobHeader, ZString.Empty);
				form.Controls.Add(control);
				form.Show();
				var declarationsGrid = (ZGrid)control.Controls.Find("StorageDecGrid", true).First();
				declarationsGrid.SelectSingleElement(storageDec);
				var linesGrid = (ZGrid)control.Controls.Find("NewSplitLinesGrid", true).First();
				linesGrid.SelectSingleElement(storageLineTo);
				linesGrid.DeleteMenuItem.PerformClick();
				Assert(!storageLineTo.IsDeleted);
				storageDec.STH_MessageStatus = ZString.Empty;
				linesGrid.DeleteMenuItem.PerformClick();
				Assert(storageLineTo.IsDeleted);
			}
		}

		public void TestLinesAndMessagesTabPages()
		{
			using (var form = new ZForm())
			using (var control = new CUSPCSDeclarationUserControl())
			{
				control.SetDataBinding(storageJobHeader, "");
				form.Controls.Add(control);
				form.Show();
				var linesTabPage = (ZTabPage)control.Controls.Find("LinesTabPage", true).First();
				AssertEquals("Lines", linesTabPage.CaptionResourceString.Caption);
				var messagesTabPage = (ZTabPage)control.Controls.Find("MessagesTabPage", true).First();
				AssertEquals("Messages", messagesTabPage.CaptionResourceString.Caption);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			storageJobHeader = Factory.New<CusTempStorageJobHeader>();
			storageDec = storageJobHeader.CUSPCSCusTempStorageDecs.AddNew();
			storageDec.STH_DeclarationType = TemporaryStorageDeclarationTypeList.Codes.CustomsPresentationCargo;
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
		}

		CusTempStorageJobHeader storageJobHeader;
		CUSPCSCusTempStorageDec storageDec;
	}
}
