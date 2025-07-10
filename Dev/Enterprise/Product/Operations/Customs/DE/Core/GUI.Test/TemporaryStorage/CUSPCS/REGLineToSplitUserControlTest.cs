using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.Customs.DE.Messaging;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.DE.GUI.Testing
{
	sealed class REGLineToSplitUserControlTest : TestCaseWithFactory
	{
		public void TestControls_Binding()
		{
			using (var userControl = new REGLineToSplitUserControl())
			{
				var atbNumberCodeFindBox = userControl.ATBNumberCodeFindBoxWithSelectedEvent;
				var lineNumberTextBox = userControl.LineNumberTextBox;
				var messageStatusTextBox = userControl.MessageStatusTextBox;
				var customerReferenceTextBox = userControl.CustomerReferenceTextBox;
				var packagesTextBox = userControl.PackagesAndPackageTypeTextBox;
				var ownerReferenceTextBox = userControl.OwnerReferenceTextBox;
				var goodsDescriptionTextBox = userControl.GoodsDescriptionTextBox;
				CombineAssertions("Binding", () =>
				{
					AssertEquals("atbNumberCodeFindBox: Binding", "FormattedOwnerReferenceNumber", atbNumberCodeFindBox.GetBindingMember());
					AssertEquals("lineNumberTextBox: Binding", "TSL_LineNo", lineNumberTextBox.GetBindingMember());
					AssertEquals("messageStatusTextBox: Binding", "Dec.STH_MessageStatus", messageStatusTextBox.GetBindingMember());
					AssertEquals("customerReferenceTextBox: Binding", "RelatedRegLineCustomerReference", customerReferenceTextBox.GetBindingMember());
					AssertEquals("ownerReferenceTextBox: Binding", "RelatedRegLineOwnerReference", ownerReferenceTextBox.GetBindingMember());
					AssertEquals("packagesTextBox: Binding", "RelatedRegLinePackagesAndPackageType", packagesTextBox.GetBindingMember());
					AssertEquals("goodsDescriptionTextBox: Binding", "RelatedRegLineGoodsDescription", goodsDescriptionTextBox.GetBindingMember());
				});
			}
		}

		public void TestAtbNumberCodeFindBox()
		{
			using (var userControl = new REGLineToSplitUserControl())
			{
				var atbNumberCodeFindBox = userControl.ATBNumberCodeFindBoxWithSelectedEvent;
				CombineAssertions("Control Properties", () =>
				{
					AssertEquals("ModuleId", ModuleIDs.Customs.EU.DE.ImportFromSumARegister, atbNumberCodeFindBox.ModuleID);
					AssertEquals("ShowDescriptionBox", expected: false, atbNumberCodeFindBox.ShowDescriptionBox);
					AssertEquals("Should resize", expected: false, atbNumberCodeFindBox.ShouldResize);
					AssertEquals("Prebound MaxLength", 30, atbNumberCodeFindBox.PreBoundMaxLength);
				});
			}
		}

		public void TestControl_TabIndexes()
		{
			using (var userControl = new REGLineToSplitUserControl())
			{
				var atbNumberCodeFindBox = userControl.ATBNumberCodeFindBoxWithSelectedEvent;
				var lineNumberTextBox = userControl.LineNumberTextBox;
				CombineAssertions(() =>
				{
					AssertEquals("ATBNumberCodeFindBox", 0, atbNumberCodeFindBox.TabIndex);
					AssertEquals("LineNumberTextBox", 1, lineNumberTextBox.TabIndex);
				});
			}
		}

		public void TestAtbNumberCodeFindBox_SelectedEvent()
		{
			var regHeader = Factory.New<CusTempStorageRegHeader>();
			regHeader.SRH_AppCode = TemporaryStorageApplicationCodeList.Codes.SumA;
			regHeader.SRH_Reference = "ATB123";
			var regLine = regHeader.CusTempStorageRegLines.AddNew();
			regLine.SRL_LineNumber = 2;
			regLine.SRL_CustomsStatus = CustomsStatusList.Codes.TST;
			Factory.Save();

			var storageJobHeader = Factory.New<CusTempStorageJobHeader>();
			var storageDec = storageJobHeader.CUSPCSCusTempStorageDecs.AddNew();
			storageDec.STH_DeclarationType = TemporaryStorageDeclarationTypeList.Codes.CustomsPresentationCargo;
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			var consolidatedStorageLine = storageDec.ConsolidatedCusTempStorageLine;

			using (var form = new ZForm())
			using (var control = new CUSPCSDeclarationUserControl())
			{
				control.SetDataBinding(storageJobHeader, ZString.Empty);
				form.Controls.Add(control);
				form.Show();
				var declarationsGrid = (ZGrid)control.Controls.Find("StorageDecGrid", searchAllChildren: true)[0];
				declarationsGrid.SelectSingleElement(storageDec);
				var lineToSplitUserControl = control.FindSingle<LineToSplitDynamicUserControl>();
				var regLineToSplitUserControl = lineToSplitUserControl.Controls.Find("REGLineToSplitUserControl", searchAllChildren: true)[0];
				var atbNumberCodeFindBox = ((REGLineToSplitUserControl)regLineToSplitUserControl).ATBNumberCodeFindBoxWithSelectedEvent;
				CombineAssertions(() =>
				{
					AssertEquals("Before select", 1, consolidatedStorageLine.TSL_LineNo);
					atbNumberCodeFindBox.CodeBox.Text = "ATB123";
					atbNumberCodeFindBox.SelectFromPopupForm(autoSelect: true);
					AssertEquals("After select", 2, consolidatedStorageLine.TSL_LineNo);
				});
			}
		}
	}
}
