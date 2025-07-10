using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.DE.GUI.Testing
{
	sealed class CHGTSTDeclarationUserControlTest : TestCaseWithFactory
	{
		public void TestDecGridColumnVisibility()
		{
			using (var form = new ZForm(header))
			using (var chgtstDeclarationUserControl = new CHGTSTDeclarationUserControl())
			{
				chgtstDeclarationUserControl.Dock = DockStyle.Fill;
				form.Controls.Add(chgtstDeclarationUserControl);
				form.SetDataBinding(header, ".");
				form.Show();
				var decsGrid = chgtstDeclarationUserControl.DecsGrid;
				Assert(decsGrid.Columns[CHGTSTCusTempStorageDec.Schema.NewCustodianBranch].IsVisible);
			}
		}

		public void TestLinesGridColumnAvailabilityAndVisibilityAWB()
		{
			using (var form = new ZForm(header))
			using (var chgtstDeclarationUserControl = new CHGTSTDeclarationUserControl())
			{
				chgtstDeclarationUserControl.Dock = DockStyle.Fill;
				form.Controls.Add(chgtstDeclarationUserControl);
				form.SetDataBinding(header, ".");
				form.Show();
				storageDec1.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
				var linesGrid = chgtstDeclarationUserControl.LinesGrid;
				var columns = linesGrid.Columns;
				CombineAssertions(() =>
				{
					AssertEquals("Only 7 columns avaialble", 7, columns.Count);
					AssertEquals("Owner Reference Type visible", true, columns[CusTempStorageLine.Schema.TSL_OwnerReferenceType].IsVisible);
					AssertEquals("Custodian Address visible", true, columns[CusTempStorageLine.Schema.TSL_OA_Custodian].IsVisible);
					AssertEquals("Custodian Org visible", true, columns[CusTempStorageLine.Schema.CustodianOrgPK].IsVisible);
					AssertEquals("Custodian Identifier visible", true, columns[CusTempStorageLine.Schema.TSL_CustodianIdentifier].IsVisible);
					AssertEquals("Custodian Branch visible", true, columns[CusTempStorageLine.Schema.TSL_CustodianIdentifierBranchNo].IsVisible);
					AssertEquals("New Goods Location visible", true, columns[CusTempStorageLine.Schema.TSL_LocationOfGoods].IsVisible);
					AssertEquals("Customs Status visible", true, columns[CusTempStorageLine.Schema.TSL_CustomsStatus].IsVisible);
				});
			}
		}

		public void TestLinesGridColumnAvailabilityAndVisibilityREG()
		{
			using (var form = new ZForm(header))
			using (var chgtstDeclarationUserControl = new CHGTSTDeclarationUserControl())
			{
				chgtstDeclarationUserControl.Dock = DockStyle.Fill;
				form.Controls.Add(chgtstDeclarationUserControl);
				form.SetDataBinding(header, ".");
				form.Show();
				storageDec1.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
				var linesGrid = chgtstDeclarationUserControl.LinesGrid;
				var columns = linesGrid.Columns;
				CombineAssertions(() =>
				{
					AssertEquals("Only 3 columns avaialble", 3, columns.Count);
					AssertEquals("Line No visible", true, columns[CusTempStorageLine.Schema.TSL_LineNo].IsVisible);
					AssertEquals("New Goods Location visible", true, columns[CusTempStorageLine.Schema.TSL_LocationOfGoods].IsVisible);
					AssertEquals("Customs Status visible", true, columns[CusTempStorageLine.Schema.TSL_CustomsStatus].IsVisible);
				});
			}
		}

		public void TestControlVisibility()
		{
			using (var form = new ZForm(header))
			using (var chgtstDeclarationUserControl = new CHGTSTDeclarationUserControl())
			{
				chgtstDeclarationUserControl.Dock = DockStyle.Fill;
				form.Controls.Add(chgtstDeclarationUserControl);
				form.SetDataBinding(header, ".");
				form.Show();
				storageDec1.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
				Assert(!chgtstDeclarationUserControl.AWBDeclarationUserControl.Visible);
				Assert(chgtstDeclarationUserControl.REGDeclarationUserControl.Visible);

				storageDec1.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
				Assert(chgtstDeclarationUserControl.AWBDeclarationUserControl.Visible);
				Assert(!chgtstDeclarationUserControl.REGDeclarationUserControl.Visible);
			}
		}

		public void TestMaxCountValidation()
		{
			using (var form = new ZForm(header))
			using (var chgtstDeclarationUserControl = new CHGTSTDeclarationUserControl())
			{
				chgtstDeclarationUserControl.Dock = DockStyle.Fill;
				form.Controls.Add(chgtstDeclarationUserControl);
				form.SetDataBinding(header, ".");
				form.Show();
				var decsGrid = chgtstDeclarationUserControl.DecsGrid;
				storageDec1.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
				var storageLine1 = storageDec1.CusTempStorageLines.AddNew();
				var storageLine2 = storageDec1.CusTempStorageLines.AddNew();
				header.RunPreSaveValidation();
				AssertNoRowErrors(storageLine1);
				AssertNoRowErrors(storageLine2);
				decsGrid.CurrentRowIndex = 1;
				storageDec2.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
				storageLine1 = storageDec2.CusTempStorageLines.AddNew();
				storageLine2 = storageDec2.CusTempStorageLines.AddNew();
				header.RunPreSaveValidation();
				AssertNoRowErrors(storageLine1);
				AssertHasRowError(storageLine2, "When Identification Type is 'AWB' only a single line is allowed");
			}
		}

		public void TestUnrequiredColumnsAreRemoved()
		{
			using (var form = new ZForm(header))
			using (var chgtstDeclarationUserControl = new CHGTSTDeclarationUserControl())
			{
				chgtstDeclarationUserControl.Dock = DockStyle.Fill;
				form.Controls.Add(chgtstDeclarationUserControl);
				form.SetDataBinding(header, ".");
				form.Show();
				var linesGrid = chgtstDeclarationUserControl.LinesGrid;
				AssertNull(linesGrid.GetColumnStyle(CusTempStorageLine.Schema.GoodsOwnerOrgPK));
				AssertNull(linesGrid.GetColumnStyle(CusTempStorageLine.Schema.TSL_OA_GoodsOwner));
				AssertNull(linesGrid.GetColumnStyle(CusTempStorageLine.Schema.TSL_GoodsOwnerIdentifier));
				AssertNull(linesGrid.GetColumnStyle(CusTempStorageLine.Schema.TSL_GoodsOwnerIdentifierBranchNo));
			}
		}

		public void TestCustodianBindToLists()
		{
			using (var form = new ZForm(header))
			using (var chgoffDeclarationUserControl = new CHGTSTDeclarationUserControl())
			{
				chgoffDeclarationUserControl.Dock = DockStyle.Fill;
				form.Controls.Add(chgoffDeclarationUserControl);
				form.SetDataBinding(header, ".");
				form.Show();
				var linesGrid = chgoffDeclarationUserControl.LinesGrid;
				AssertEquals("TSL_OA_Custodian_ZAddress.OrgAddress_List", ((ZGuidDropEditColumnStyleInfo)linesGrid.GetColumnStyle(CusTempStorageLine.Schema.TSL_OA_Custodian)).BindToList);
				AssertEquals("Lookups.OrganizationsFindBoxList", ((ZOrganisationFindBoxColumnStyleInfo)linesGrid.GetColumnStyle(CusTempStorageLine.Schema.CustodianOrgPK)).BindToList);
			}
		}

		public void TestDeleteDeclarationsGrid()
		{
			storageDec1.STH_MessageStatus = Common.Shared.MessageStatusList.Codes.Sent;
			using (var form = new ZForm())
			using (var control = new CHGTSTDeclarationUserControl())
			{
				control.SetDataBinding(header, "");
				form.Controls.Add(control);
				form.Show();
				var declarationsGrid = control.DecsGrid;
				declarationsGrid.SelectSingleElement(storageDec1);
				declarationsGrid.DeleteMenuItem.PerformClick();
				Assert(!storageDec1.IsDeleted);
				storageDec1.STH_MessageStatus = "";
				declarationsGrid.DeleteMenuItem.PerformClick();
				Assert(storageDec1.IsDeleted);
			}
		}

		public void TestDeleteLinesGrid()
		{
			var storageLine = storageDec1.CusTempStorageLines.AddNew();
			storageDec1.STH_MessageStatus = Common.Shared.MessageStatusList.Codes.Sent;

			using (var form = new ZForm())
			using (var control = new CHGTSTDeclarationUserControl())
			{
				control.SetDataBinding(header, ZString.Empty);
				form.Controls.Add(control);
				form.Show();
				var declarationsGrid = control.DecsGrid;
				declarationsGrid.SelectSingleElement(storageDec1);
				var linesGrid = control.LinesGrid;
				linesGrid.SelectSingleElement(storageLine);
				linesGrid.DeleteMenuItem.PerformClick();
				Assert(!storageLine.IsDeleted);
				storageDec1.STH_MessageStatus = ZString.Empty;
				linesGrid.DeleteMenuItem.PerformClick();
				Assert(storageLine.IsDeleted);
			}
		}

		public void TestLinesAndMessagesTabPages()
		{
			using (var form = new ZForm())
			using (var control = new CHGTSTDeclarationUserControl())
			{
				control.SetDataBinding(header, "");
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
			using (var chgtstDeclarationUserControl = new CHGTSTDeclarationUserControl())
			{
				var decsGrid = chgtstDeclarationUserControl.DecsGrid;

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
			storageDec1 = header.CHGTSTCusTempStorageDecs.AddNew();
			storageDec2 = header.CHGTSTCusTempStorageDecs.AddNew();
		}
		CusTempStorageJobHeader header;
		CHGTSTCusTempStorageDec storageDec1;
		CHGTSTCusTempStorageDec storageDec2;
	}
}
