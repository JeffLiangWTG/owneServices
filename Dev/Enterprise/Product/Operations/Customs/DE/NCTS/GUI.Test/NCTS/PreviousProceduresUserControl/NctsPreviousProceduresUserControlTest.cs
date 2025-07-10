using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.Customs.DE.GUI;
using Enterprise.Customs.DE.NCTS.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.Universal.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using NctsHeader = Enterprise.Customs.DE.NCTS.Business.NctsHeader;
using NctsPreviousDocument = Enterprise.Customs.DE.NCTS.Business.NctsPreviousDocument;

namespace Enterprise.Customs.DE.NCTS.GUI.Testing
{
	sealed class NctsPreviousProceduresUserControlTest : TestCaseWithFactory
	{
		public void TestPreviousDocumentsNctsATZLPanel()
		{
			using (var control = new NctsPreviousProceduresUserControl())
			{
				CombineAssertions(() =>
				{
					control.SetPreviousDocumentsGridColumnsVisible(NctsPreviousProcedureList.Codes._9DEZ);
					var atzlPanel = control.FindSingle<NctsPreviousProceduresATZLPanel>();
					AssertNotNull("Exists", atzlPanel);
					AssertEquals("reference allows lowercase", CharacterCasing.Normal, atzlPanel.LocalReferenceTextBox.CharacterCasing);

					control.SetPreviousDocumentsGridColumnsVisible(NctsPreviousProcedureList.Codes._N337);
					AssertEquals("Removed", false, control.FindAll<NctsPreviousProceduresATZLPanel>().Any());
				});
			}
		}

		public void TestPreviousDocumentsNctsATZLPanel_AuthorizationNumber()
		{
			using (var control = new NctsPreviousProceduresUserControl())
			{
				control.SetPreviousDocumentsGridColumnsVisible(NctsPreviousProcedureList.Codes._9DEZ);
				CombineAssertions(() =>
				{
					var atZlPanel = control.FindSingle<NctsPreviousProceduresATZLPanel>();
					var authorizationNumberDropEdit = atZlPanel.AuthorizationNumberDropEdit;
					AssertEquals("ShowInDropDownList", ZDropEdit.ShowInDropDownList.ShowCodeAndDescription, authorizationNumberDropEdit.ShowInDropDown);
					AssertEquals("ShowDescriptionBox", false, authorizationNumberDropEdit.ShowDescriptionBox);
				});
			}
		}

		public void TestPreviousDocumentsNctsATAVPanel()
		{
			using (var control = new NctsPreviousProceduresUserControl())
			{
				control.SetPreviousDocumentsGridColumnsVisible(NctsPreviousProcedureList.Codes._9DEY);
				AssertNotNull(control.FindSingle<NctsPreviousProceduresATAVPanel>());
			}
		}

		public void TestSetPreviousDocumentsGridColumnsVisible()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();

			using (var form = new ZForm(goodsItem))
			using (var control = new NctsPreviousProceduresUserControl())
			{
				control.SetDataBinding(nctsHeader, "Bills.GoodsItems");
				form.Controls.Add(control);
				form.Show();
				var previousDocumentsGrid = (ZGrid)control.Controls.Find("PreviousDocumentsGrid", true).Single();

				goodsItem.PreviousProcedureMaster.CSI_Procedure = NctsPreviousDocTypList.Codes.OHNE;
				control.SetPreviousDocumentsGridColumnsVisible(NctsPreviousDocTypList.Codes.OHNE);
				AssertEquals("PreviousDocumentsGrid is invisible.", false, previousDocumentsGrid.Visible);
			}
		}

		public void TestImportSumARegisterButtonVisible()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var bill = nctsHeader.Bills.AddNew();
			var goodsItem = bill.GoodsItems.AddNew();

			using (var control = new NctsPreviousProceduresUserControl())
			{
				var previousDocumentGrid = (ZGrid)control.Controls.Find("PreviousDocumentsGrid", true).Single();
				var collection = new NctsPreviousDocumentCollection<NctsPreviousDocument>(goodsItem);
				collection.AddNew();
				previousDocumentGrid.SetDataBinding(collection, ZString.Empty);
				previousDocumentGrid.ListManager.Position = 0;
				control.Show();

				control.SetPreviousDocumentsGridColumnsVisible(NctsPreviousProcedureList.Codes._N337);
				AssertEquals(true, control.ImportSumARegisterButton.Visible);

				control.SetPreviousDocumentsGridColumnsVisible(NctsPreviousProcedureList.Codes._9DEZ);
				AssertEquals(false, control.ImportSumARegisterButton.Visible);
			}
		}

		public void TestImportFromSumARegister()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = nctsHeader.Bills.AddNew();
			var goodsItem = bill.GoodsItems.AddNew();

			var regHeader = Factory.New<CusTempStorageRegHeader>();
			regHeader.SRH_Reference = "REF001";
			regHeader.SRH_CustomsOffice = "MYCODE";
			var regLine = regHeader.CusTempStorageRegLines.AddNew();
			regLine.SRL_LineNumber = 1;
			regLine.SRL_PackagesRemaining = 10;
			regLine.SRL_LimitDate = ZDate.Today.AddDays(2);

			NCTSTestHelper.CreateCustomsOfficeForTest(nctsHeader.MovementHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "MYCODE", ZDateTime.Empty, true);

			Factory.Save();

			using (var form = new ZForm(nctsHeader))
			using (var control = new NctsPreviousProceduresUserControl())
			{
				control.SetDataBinding(goodsItem, ZString.Empty);
				form.Controls.Add(control);
				form.Show();

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(
					dialog =>
					{
						var importFromSumARegisterModuleForm = ((ImportFromSumARegisterModuleForm)dialog);
						var filterStripControl = importFromSumARegisterModuleForm.FilterControlPanel.FindSingle<ZFilterStripControl>();
						filterStripControl.FirePerformSearch();
						filterStripControl.FilteredGrid.Select(0);
						importFromSumARegisterModuleForm.FindSingle<ZButton>("OK_Button").PerformClick();
					});

				var importButton = control.ImportSumARegisterButton;

				importButton.Visible = true;

				importButton.PerformClick();

				AssertEquals(1, goodsItem.PreviousProcedures.Count);

				importButton.PerformClick();

				AssertEquals(2, goodsItem.PreviousProcedures.Count);
			}

			Factory.Save();
		}

		public void TestPreviousDocumentProcedureCodeAboutToChange()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();

			using (var form = new ZForm(goodsItem))
			using (var control = new NctsPreviousProceduresUserControl())
			{
				control.SetDataBinding(nctsHeader, "Bills.GoodsItems");
				form.Controls.Add(control);
				form.Show();

				goodsItem.PreviousProcedureMaster.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				goodsItem.PreviousProcedureMaster.CSI_Procedure = NctsPreviousProcedureList.Codes._N337;
				AssertEquals("Message is displayed", "The System is about to delete all existing Previous Procedure lines.\r\nDo you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestColumnWidth()
		{
			using (var control = new NctsPreviousProceduresUserControl())
			{
				var previousDocumentsGrid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
				CombineAssertions(() =>
				{
					AssertEquals("CSI_ReferenceNumber2", 120, previousDocumentsGrid.GetColumnStyle(Customs.Business.AutoCusSupportingInfo.Schema.CSI_ReferenceNumber2).Width);
					AssertEquals("CSI_Description", 160, previousDocumentsGrid.GetColumnStyle(Customs.Business.AutoCusSupportingInfo.Schema.CSI_Description).Width);
					AssertEquals("FormattedTariff", 106, previousDocumentsGrid.GetColumnStyle(NctsPreviousDocument.Schema.FormattedTariff).Width);
					AssertEquals("CSI_Quantity", 124, previousDocumentsGrid.GetColumnStyle(Customs.Business.AutoCusSupportingInfo.Schema.CSI_Quantity).Width);
					AssertEquals("CSI_UnitOfQuantity", 112, previousDocumentsGrid.GetColumnStyle(Customs.Business.AutoCusSupportingInfo.Schema.CSI_UnitOfQuantity).Width);
					AssertEquals("CSI_Quantity2", 93, previousDocumentsGrid.GetColumnStyle(Customs.Business.AutoCusSupportingInfo.Schema.CSI_Quantity2).Width);
					AssertEquals("CSI_UnitOfQuantity2", 112, previousDocumentsGrid.GetColumnStyle(Customs.Business.AutoCusSupportingInfo.Schema.CSI_UnitOfQuantity2).Width);
					AssertEquals("CSI_SubType", 80, previousDocumentsGrid.GetColumnStyle(Customs.Business.AutoCusSupportingInfo.Schema.CSI_SubType).Width);
					AssertEquals("CSI_CustomsOffice", 150, previousDocumentsGrid.GetColumnStyle(Customs.Business.AutoCusSupportingInfo.Schema.CSI_CustomsOffice).Width);
					AssertEquals("CSI_Procedure", 72, previousDocumentsGrid.GetColumnStyle(Customs.Business.AutoCusSupportingInfo.Schema.CSI_Procedure).Width);
					AssertEquals("CSI_AdditionalDescription", 84, previousDocumentsGrid.GetColumnStyle(Customs.Business.AutoCusSupportingInfo.Schema.CSI_AdditionalDescription).Width);
					AssertEquals("Status", 105, previousDocumentsGrid.GetColumnStyle(NctsPreviousDocument.Schema.Status).Width);
					AssertEquals("UsualProcessingFlag", 128, previousDocumentsGrid.GetColumnStyle(NctsPreviousDocument.Schema.UsualProcessingFlag).Width);
				});
			}
		}

		public void TestColumnCasing()
		{
			using (var control = new NctsPreviousProceduresUserControl())
			{
				var previousDocumentsGrid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
				CombineAssertions(() =>
				{
					AssertEquals("CSI_ReferenceNumber", CharacterCasing.Normal, previousDocumentsGrid.GetColumnStyle(Customs.Business.AutoCusSupportingInfo.Schema.CSI_ReferenceNumber).CharacterCasing);
					AssertEquals("CSI_ReferenceNumber2", CharacterCasing.Normal, previousDocumentsGrid.GetColumnStyle(Customs.Business.AutoCusSupportingInfo.Schema.CSI_ReferenceNumber2).CharacterCasing);
					AssertEquals("CSI_Description", CharacterCasing.Normal, previousDocumentsGrid.GetColumnStyle(Customs.Business.AutoCusSupportingInfo.Schema.CSI_Description).CharacterCasing);
					AssertEquals("CSI_UnitOfQuantity", CharacterCasing.Upper, previousDocumentsGrid.GetColumnStyle(Customs.Business.AutoCusSupportingInfo.Schema.CSI_UnitOfQuantity).CharacterCasing);
					AssertEquals("CSI_UnitOfQuantity2", CharacterCasing.Upper, previousDocumentsGrid.GetColumnStyle(Customs.Business.AutoCusSupportingInfo.Schema.CSI_UnitOfQuantity2).CharacterCasing);
					AssertEquals("CSI_SubType", CharacterCasing.Upper, previousDocumentsGrid.GetColumnStyle(Customs.Business.AutoCusSupportingInfo.Schema.CSI_SubType).CharacterCasing);
					AssertEquals("CSI_CustomsOffice", CharacterCasing.Upper, previousDocumentsGrid.GetColumnStyle(Customs.Business.AutoCusSupportingInfo.Schema.CSI_CustomsOffice).CharacterCasing);
					AssertEquals("CSI_AdditionalDescription", CharacterCasing.Normal, previousDocumentsGrid.GetColumnStyle(Customs.Business.AutoCusSupportingInfo.Schema.CSI_AdditionalDescription).CharacterCasing);
					AssertEquals("CSI_Procedure", CharacterCasing.Upper, previousDocumentsGrid.GetColumnStyle(Customs.Business.AutoCusSupportingInfo.Schema.CSI_Procedure).CharacterCasing);
				});
			}
		}

		public void TestColumnReadOnly()
		{
			using (var control = new NctsPreviousProceduresUserControl())
			{
				var previousDocumentsGrid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
				AssertEquals("CSI_Procedure", true, previousDocumentsGrid.GetColumnStyle(Customs.Business.AutoCusSupportingInfo.Schema.CSI_Procedure).IsReadOnly);
			}
		}

		public void TestOnCurrentDataItemChanged()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItems = nctsHeader.Bills.AddNew().GoodsItems;
			var goodsItem = goodsItems.AddNew();
			goodsItem.PreviousProcedureMaster.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
			var goodsItem2 = goodsItems.AddNew();
			goodsItem2.PreviousProcedureMaster.CSI_Procedure = NctsPreviousProcedureList.Codes._N337;

			using (var form = new ZForm(goodsItem))
			using (var control = new NctsPreviousProceduresUserControl())
			{
				control.SetDataBinding(nctsHeader, "Bills.GoodsItems");
				form.Controls.Add(control);
				form.Show();
				CombineAssertions(() =>
				{
					control.BindingContext[nctsHeader, "Bills.GoodsItems"].Position = 0;
					var previousDocumentsGrid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
					AssertNotNull("Exists", control.FindSingle<NctsPreviousProceduresATZLPanel>());
					AssertEquals("CSI_SubType unavailable", true, previousDocumentsGrid.GetColumnStyle(Customs.Business.AutoCusSupportingInfo.Schema.CSI_SubType).IsUnavailable);
					control.BindingContext[nctsHeader, "Bills.GoodsItems"].Position = 1;
					AssertEquals("Removed", false, control.FindAll<NctsPreviousProceduresATZLPanel>().Any());
					AssertEquals("CSI_SubType available", false, previousDocumentsGrid.GetColumnStyle(Customs.Business.AutoCusSupportingInfo.Schema.CSI_SubType).IsUnavailable);
				});
			}
		}

		public void TestPrevDocsGroupBoxVisibility()
		{
			using (var control = new NctsPreviousProceduresUserControl())
			{
				var prevDocsGroupBox = control.FindSingle<ZGroupBox>("PrevDocsGroupBox");
				AssertEquals("PreviousDocumentsGrid is invisible.", false, prevDocsGroupBox.Visible);
			}
		}

		[TestDate(2020, 09, 16)]
		public void TestColumnFormattedTariff()
		{
			using (var control = new NctsPreviousProceduresUserControl())
			{
				var previousDocumentsGrid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
				var columnInfo = (TariffColumnStyleInfo)previousDocumentsGrid.GetColumnStyle(NctsPreviousDocument.Schema.FormattedTariff);
				CombineAssertions(() =>
				{
					AssertEquals("GetCountryCode", Core.Constants.CountryCodes.Germany, columnInfo.GetCountryCode());
					AssertEquals("GetDataGrouping", Core.Constants.CountryCodes.Germany, columnInfo.GetDataGrouping());
					AssertEquals("TariffType", Universal.Constants.TariffTypes.Import, columnInfo.TariffType);
					AssertEquals("EffectiveDate", new ZDateTime(2020, 09, 16), columnInfo.GetEffectiveDate.Invoke());
				});
			}
		}

		public void TestBottomPanel()
		{
			using (var control = new NctsPreviousProceduresUserControl())
			{
				var bottomPanel = control.FindSingle<ZPanel>("BottomPanel");
				AssertEquals(false, bottomPanel.Visible);
			}
		}
	}
}
