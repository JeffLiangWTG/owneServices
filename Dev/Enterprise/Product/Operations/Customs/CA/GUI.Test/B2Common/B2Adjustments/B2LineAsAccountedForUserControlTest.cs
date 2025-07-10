using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Common.CA;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class B2LineAsAccountedForUserControlTest : TestCaseWithFactory
	{
		public void TestTariffUserControlForCAGlobalTariff()
		{
			var gridName = "AsAccountForGrid";
			var columnName = JobComInvoiceLine.Schema.JI_FormattedTariff;
			var tariffFindBoxName_TrfCA = "ClassificationTariffFindBox";
			var tariffFindBoxName_SRDb = "ClassificationTariffFromSRDbFindBox";
			var tariffColumnInfoType = typeof(Universal.GUI.TariffColumnStyleInfo);

			var b2 = Factory.New<JobDeclaration>();
			b2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var subHeader = b2.Invoices.AddNew();
			var asAccountLine = subHeader.AsAccountForFilteredInvoiceLines.AddNew();
			asAccountLine.OnLoaded();

			using (var form = new ZForm(b2))
			using (var control = new B2LineAsAccountedForUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var tariffColumnInfoCaption = ClassificationTariffUserControlTestHelper.AssertTariffColumnInfo_GetTariffFromTrfCA(control, gridName, columnName, tariffColumnInfoType);
				ClassificationTariffUserControlTestHelper.AssertTariffColumnInfo_GetTariffFromSRDb(control, gridName, columnName, tariffColumnInfoCaption);
				ClassificationTariffUserControlTestHelper.AssertTariffFindBox_GetTariffFromSRDb(control, tariffFindBoxName_TrfCA, tariffFindBoxName_SRDb);
			}
		}

		public void TestOnCurrentDataItem_OnApportionmentDirtyChangedShouldBeDetached()
		{
			var b2 = Factory.New<JobDeclaration>();
			b2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var subHeader = b2.Invoices.AddNew();
			var asAccountLine = subHeader.AsAccountForFilteredInvoiceLines.AddNew();
			asAccountLine.OnLoaded();
			using (var b2LineAsAccountedForUserControl = new B2LineAsAccountedForUserControl())
			{
				b2LineAsAccountedForUserControl.SetDataBinding(b2, "");
			}

			var onApportionmentDirtyChangedField = b2.GetType().BaseType.BaseType.GetField("OnApportionmentDirtyChanged", BindingFlags.Instance | BindingFlags.NonPublic);
			var onApportionmentDirtyChanged = onApportionmentDirtyChangedField.GetValue(b2);
			AssertNull("OnCurrentDataItem_OnApportionmentDirtyChanged should be detached", onApportionmentDirtyChanged);
		}

		public void TestCurrentDataItem_OnApportionmentDirtyChanged()
		{
			var b2 = Factory.New<JobDeclaration>();
			b2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var subHeader = b2.Invoices.AddNew();
			var asAccountLine = subHeader.AsAccountForFilteredInvoiceLines.AddNew();
			asAccountLine.OnLoaded();
			using (var b2LineAsAccountedForUserControl = new B2LineAsAccountedForUserControl())
			{
				b2LineAsAccountedForUserControl.SetDataBinding(b2, "");
				b2LineAsAccountedForUserControl.Show();
				Assert(!b2LineAsAccountedForUserControl.ApportionmentPendingLabel.Visible);
				asAccountLine.CA_CVforCurrConv = 4;
				Assert(b2LineAsAccountedForUserControl.ApportionmentPendingLabel.Visible);
			}
		}

		public void TestSelectingLinesButton_Click()
		{
			var b2 = Factory.New<JobDeclaration>();
			b2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			using (var b2LineAsAccountedForUserControl = new B2LineAsAccountedForUserControl())
			{
				b2LineAsAccountedForUserControl.SetDataBinding(b2, "");
				b2LineAsAccountedForUserControl.Show();
				b2LineAsAccountedForUserControl.SelectingLinesButton.PerformClick();
				AssertEquals("There is no accepted Entry message in the system corresponding to the Original Transaction Number.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSelectingLinesButton_ClickWithB3Lines()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "842957342RM0001");
			var b3Dec = Factory.New<JobDeclaration>();
			b3Dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			b3Dec.JE_MessageSubType = B3EntryTypeList.Codes.AutomotiveP;
			b3Dec.JE_OH_Importer = importer.PK;
			b3Dec.TransactionNumber.AccountSecurityCode = "40000";
			b3Dec.TransactionNumber.SequentialNumber = "04228";
			b3Dec.CA_K84AccountingDate = new ZDateTime(2012, 05, 29);
			var invoice = b3Dec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			b3Dec.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			b3Dec.DoMerge();
			Factory.Save();
			var b2Dec = Factory.New<JobDeclaration>();
			b2Dec.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			b2Dec.CA_B2Type = B2TypeList.Codes.Specific;
			b2Dec.CA_OriginalTransactionNo = b3Dec.DeclarationNumber;

			using (var b2LineAsAccountedForUserControl = new B2LineAsAccountedForUserControl())
			{
				AssertNoExceptionThrown(() =>
				{
					b2LineAsAccountedForUserControl.SetDataBinding(b2Dec, "");
					b2LineAsAccountedForUserControl.Show();
					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
					{
						(dialog as ZForm).Shown += (sender, e) =>
						{
							var form = sender as SelectingLinesForm;
							form.Close();
						};
					});
					b2LineAsAccountedForUserControl.SelectingLinesButton.PerformClick();
				});
			}
		}
	}
}
