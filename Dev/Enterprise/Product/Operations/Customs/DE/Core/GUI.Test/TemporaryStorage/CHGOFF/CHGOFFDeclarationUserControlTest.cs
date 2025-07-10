using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	class CHGOFFDeclarationUserControlTest : TestCaseWithFactory
	{
		public void TestLinesGridColumnAvailabilityAndVisibilityAWB()
		{
			using (var form = new ZForm(header))
			using (var chgoffDeclarationUserControl = new CHGOFFDeclarationUserControl())
			{
				chgoffDeclarationUserControl.Dock = DockStyle.Fill;
				form.Controls.Add(chgoffDeclarationUserControl);
				form.SetDataBinding(header, ".");
				form.Show();
				storageDec1.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
				var decsGrid = (ZGrid)chgoffDeclarationUserControl.Controls.Find("DecsGrid", true).Single();
				var linesGrid = (ZGrid)chgoffDeclarationUserControl.Controls.Find("LinesGrid", true).Single();
				var columns = linesGrid.Columns;
				CombineAssertions(() =>
				{
					AssertEquals("Only 10 columns avaialble", 10, columns.Count);
					AssertEquals("Owner Reference Type visible", true, columns[CusTempStorageLine.Schema.TSL_OwnerReferenceType].IsVisible);
					AssertEquals("Custodian Address visible", true, columns[CusTempStorageLine.Schema.TSL_OA_Custodian].IsVisible);
					AssertEquals("Custodian Org visible", true, columns[CusTempStorageLine.Schema.CustodianOrgPK].IsVisible);
					AssertEquals("Custodian Identifier visible", true, columns[CusTempStorageLine.Schema.TSL_CustodianIdentifier].IsVisible);
					AssertEquals("Custodian Branch visible", true, columns[CusTempStorageLine.Schema.TSL_CustodianIdentifierBranchNo].IsVisible);
					AssertEquals("Goods Owner Address visible", true, columns[CusTempStorageLine.Schema.TSL_OA_GoodsOwner].IsVisible);
					AssertEquals("Goods Owner Org visible", true, columns[CusTempStorageLine.Schema.GoodsOwnerOrgPK].IsVisible);
					AssertEquals("Goods Owner Identifier visible", true, columns[CusTempStorageLine.Schema.TSL_GoodsOwnerIdentifier].IsVisible);
					AssertEquals("Goods Owner Branch visible", true, columns[CusTempStorageLine.Schema.TSL_GoodsOwnerIdentifierBranchNo].IsVisible);
					AssertEquals("Customs Status visible", true, columns[CusTempStorageLine.Schema.TSL_CustomsStatus].IsVisible);
				});
			}
		}

		public void TestLinesGridColumnAvailabilityAndVisibilityREG()
		{
			using (var form = new ZForm(header))
			using (var chgoffDeclarationUserControl = new CHGOFFDeclarationUserControl())
			{
				chgoffDeclarationUserControl.Dock = DockStyle.Fill;
				form.Controls.Add(chgoffDeclarationUserControl);
				form.SetDataBinding(header, ".");
				form.Show();
				storageDec1.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
				var decsGrid = (ZGrid)chgoffDeclarationUserControl.Controls.Find("DecsGrid", true).Single();
				var linesGrid = (ZGrid)chgoffDeclarationUserControl.Controls.Find("LinesGrid", true).Single();
				var columns = linesGrid.Columns;
				CombineAssertions(() =>
				{
					AssertEquals("Only 6 columns avaialble", 6, columns.Count);
					AssertEquals("Line Number visible", true, columns[CusTempStorageLine.Schema.TSL_LineNo].IsVisible);
					AssertEquals("Goods Owner Address visible", true, columns[CusTempStorageLine.Schema.TSL_OA_GoodsOwner].IsVisible);
					AssertEquals("Goods Owner Org visible", true, columns[CusTempStorageLine.Schema.GoodsOwnerOrgPK].IsVisible);
					AssertEquals("Goods Owner Identifier visible", true, columns[CusTempStorageLine.Schema.TSL_GoodsOwnerIdentifier].IsVisible);
					AssertEquals("Goods Owner Branch visible", true, columns[CusTempStorageLine.Schema.TSL_GoodsOwnerIdentifierBranchNo].IsVisible);
					AssertEquals("Customs Status visible", true, columns[CusTempStorageLine.Schema.TSL_CustomsStatus].IsVisible);
				});
			}
		}

		public void TestControlVisibility()
		{
			using (var form = new ZForm(header))
			using (var chgoffDeclarationUserControl = new CHGOFFDeclarationUserControl())
			{
				chgoffDeclarationUserControl.Dock = DockStyle.Fill;
				form.Controls.Add(chgoffDeclarationUserControl);
				form.SetDataBinding(header, ".");
				form.Show();
				storageDec1.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
				Assert(!chgoffDeclarationUserControl.Controls.Find("AWBDeclarationUserControl", true).First().Visible);
				Assert(chgoffDeclarationUserControl.Controls.Find("REGDeclarationUserControl", true).First().Visible);

				storageDec1.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
				Assert(chgoffDeclarationUserControl.Controls.Find("AWBDeclarationUserControl", true).First().Visible);
				Assert(!chgoffDeclarationUserControl.Controls.Find("REGDeclarationUserControl", true).First().Visible);
			}
		}

		public void TestMaxCountValidation()
		{
			using (var form = new ZForm(header))
			using (var chgoffDeclarationUserControl = new CHGOFFDeclarationUserControl())
			{
				chgoffDeclarationUserControl.Dock = DockStyle.Fill;
				form.Controls.Add(chgoffDeclarationUserControl);
				form.SetDataBinding(header, ".");
				form.Show();
				var decsGrid = (ZGrid)chgoffDeclarationUserControl.Controls.Find("DecsGrid", true).First();
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

		public void TestLocationOfGoodsRemovedFromLinesGrid()
		{
			using (var form = new ZForm(header))
			using (var chgoffDeclarationUserControl = new CHGOFFDeclarationUserControl())
			{
				chgoffDeclarationUserControl.Dock = DockStyle.Fill;
				form.Controls.Add(chgoffDeclarationUserControl);
				form.SetDataBinding(header, ".");
				form.Show();
				var linesGrid = (ZGrid)chgoffDeclarationUserControl.Controls.Find("LinesGrid", true).First();
				AssertNull(linesGrid.GetColumnStyle(CusTempStorageLine.Schema.TSL_LocationOfGoods));
			}
		}

		public void TestCustodianAndDisposalEntitledTraderBindToLists()
		{
			using (var form = new ZForm(header))
			using (var chgoffDeclarationUserControl = new CHGOFFDeclarationUserControl())
			{
				chgoffDeclarationUserControl.Dock = DockStyle.Fill;
				form.Controls.Add(chgoffDeclarationUserControl);
				form.SetDataBinding(header, ".");
				form.Show();
				var linesGrid = (ZGrid)chgoffDeclarationUserControl.Controls.Find("LinesGrid", true).First();
				AssertEquals("TSL_OA_Custodian_ZAddress.OrgAddress_List", ((ZGuidDropEditColumnStyleInfo)linesGrid.GetColumnStyle(CusTempStorageLine.Schema.TSL_OA_Custodian)).BindToList);
				AssertEquals("Lookups.OrganizationsFindBoxList", ((ZOrganisationFindBoxColumnStyleInfo)linesGrid.GetColumnStyle(CusTempStorageLine.Schema.CustodianOrgPK)).BindToList);
				AssertEquals("TSL_OA_GoodsOwner_ZAddress.OrgAddress_List", ((ZGuidDropEditColumnStyleInfo)linesGrid.GetColumnStyle(CusTempStorageLine.Schema.TSL_OA_GoodsOwner)).BindToList);
				AssertEquals("Lookups.OrganizationsFindBoxList", ((ZOrganisationFindBoxColumnStyleInfo)linesGrid.GetColumnStyle(CusTempStorageLine.Schema.GoodsOwnerOrgPK)).BindToList);
			}
		}

		public void TestDeleteDeclarationsGrid()
		{
			storageDec1.STH_MessageStatus = Common.Shared.MessageStatusList.Codes.Sent;
			using (var form = new ZForm())
			using (var control = new CHGOFFDeclarationUserControl())
			{
				control.SetDataBinding(header, "");
				form.Controls.Add(control);
				form.Show();
				var declarationsGrid = (ZGrid)control.Controls.Find("DecsGrid", true).First();
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
			using (var control = new CHGOFFDeclarationUserControl())
			{
				control.SetDataBinding(header, ZString.Empty);
				form.Controls.Add(control);
				form.Show();
				var declarationsGrid = (ZGrid)control.Controls.Find("DecsGrid", true).First();
				declarationsGrid.SelectSingleElement(storageDec1);
				var linesGrid = (ZGrid)control.Controls.Find("LinesGrid", true).First();
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
			using (var control = new CHGOFFDeclarationUserControl())
			{
				control.SetDataBinding(header, "");
				form.Controls.Add(control);
				form.Show();
				var linesTabPage = (ZTabPage)control.Controls.Find("LinesTabPage", true).First();
				AssertEquals("Lines", linesTabPage.CaptionResourceString.Caption);
				var messagesTabPage = (ZTabPage)control.Controls.Find("MessagesTabPage", true).First();
				AssertEquals("Messages", messagesTabPage.CaptionResourceString.Caption);
			}
		}

		public void TestTSL_LineNoCaption()
		{
			using (var form = new ZForm(header))
			using (var chgoffDeclarationUserControl = new CHGOFFDeclarationUserControl())
			{
				chgoffDeclarationUserControl.Dock = DockStyle.Fill;
				form.Controls.Add(chgoffDeclarationUserControl);
				form.SetDataBinding(header, ".");
				form.Show();
				var linesGrid = (ZGrid)chgoffDeclarationUserControl.Controls.Find("LinesGrid", true).First();
				AssertEquals("Reference Line No.", linesGrid.GetColumnCaption(CusTempStorageLine.Schema.TSL_LineNo));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusTempStorageJobHeader>();
			storageDec1 = header.CHGOFFCusTempStorageDecs.AddNew();
			storageDec2 = header.CHGOFFCusTempStorageDecs.AddNew();
		}
		CusTempStorageJobHeader header;
		CHGOFFCusTempStorageDec storageDec1;
		CHGOFFCusTempStorageDec storageDec2;
	}
}
