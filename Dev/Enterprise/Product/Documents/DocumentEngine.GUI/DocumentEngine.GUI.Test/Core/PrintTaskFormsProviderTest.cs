using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.DocumentDelivery;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Testing
{
	sealed class PrintTaskFormsProviderTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestShowRuntimeOptionsFormWhenIsReportPrintSet()
		{
			var factory = new BusinessObjectFactory();
			var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.DocumentTestFiles.NewLine.xls", "NewLine.xls");
			var excelTemplate = new ExcelTemplateForUnitTesting("NewLine.xls", Path.GetFullPath(tempFileName));
			var template = factory.NewWithValidTestData<StmTemplateBase>();
			template.SO_Name = "NewLine Template 1";
			template.SO_Template = excelTemplate.GetAsByteArray();

			var command = factory.NewWithValidTestData<ReportCommand>();
			command.SU_MenuName = "NewLine Report 1";

			var document = command.Documents.AddNew();
			document.SI_SU = command.PK;
			document.SI_SO = template.PK;

			factory.Save();

			var formsProvider = new PrintTaskFormsProvider();
			using (var printSet = new ReportPrintSet(command))
			{
				var result = (formsProvider as IPrintTaskUIProvider).ShowRuntimeOptionsUI(printSet, AllowedDeliveryOptions.All, new DeliveryInstructions(), Env.Security.None);
				Assert(result);
			}
		}

		public void TestShowPreviewIsNotModalWhenThereIsNoParentForm()
		{
			AssertNull("Pre-condition: lastFormCreated should be null.", lastFormCreated);
			AssertNull("Pre-condition: ZFormModaliser.LastFormShownDialogForTest should be null.", ZFormModaliser.LastFormShownDialogForTest);
			AssertNull("Pre-condition: ZFormModaliser.LastFormShownForTest should be null.", ZFormModaliser.LastFormShownForTest);

			IDeliverCapableForm parentForm = null;

			var formsProvider = new PrintTaskFormsProvider();
			using (var xlsStream = OnePageSlowPreviewXlsStream)
			{
				var deliveryInfos = new DeliveryInfo[] { new DeliveryInfo(DeliveryInfo.DeliveryFormats.Report) };
				((IPrintTaskUIProvider)formsProvider).ShowPreview(xlsStream, deliveryInfos, parentForm);
			}

			AssertEquals("lastFormCreated should be of type XLSPreviewForm.", typeof(XLSPreviewForm), lastFormCreated.GetType());
			AssertEquals("lastFormCreated should not be modal.", false, lastFormCreated.Modal);
			AssertNull("ZFormModaliser.LastFormShownDialogForTest should be null.", ZFormModaliser.LastFormShownDialogForTest);
			AssertNull("ZFormModaliser.LastFormShownForTest should be null.", ZFormModaliser.LastFormShownForTest);
		}

		public void TestShowPreviewIsNotModalWhenThereIsAParentForm()
		{
			AssertNull("Pre-condition: lastFormCreated should be null.", lastFormCreated);
			AssertNull("Pre-condition: ZFormModaliser.LastFormShownDialogForTest should be null.", ZFormModaliser.LastFormShownDialogForTest);
			AssertNull("Pre-condition: ZFormModaliser.LastFormShownForTest should be null.", ZFormModaliser.LastFormShownForTest);

			using (var parentForm = new DocDeliveryForm(new DeliveryInstructions(), Env.Security.None))
			{
				parentForm.Show();

				var formsProvider = new PrintTaskFormsProvider();
				using (var xlsStream = OnePageSlowPreviewXlsStream)
				{
					var deliveryInfos = new DeliveryInfo[] { new DeliveryInfo(DeliveryInfo.DeliveryFormats.Report) };

					((IPrintTaskUIProvider)formsProvider).ShowPreview(xlsStream, deliveryInfos, parentForm);
				}

				AssertEquals("lastFormCreated should be of type XLSPreviewForm.", typeof(XLSPreviewForm), lastFormCreated.GetType());
				AssertEquals("lastFormCreated should not be modal.", false, lastFormCreated.Modal);
				AssertNull("ZFormModaliser.LastFormShownDialogForTest should be null.", ZFormModaliser.LastFormShownDialogForTest);
				AssertNull("ZFormModaliser.LastFormShownForTest should be null.", ZFormModaliser.LastFormShownForTest);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			ZForm.FormCreated += new EventHandler(ZForm_FormCreated);
		}

		protected override void TearDown()
		{
			base.TearDown();
			ZForm.FormCreated -= new EventHandler(ZForm_FormCreated);
			lastFormCreated = null;
			foreach (var item in itemsToDispose)
			{
				item.Dispose();
			}
			itemsToDispose.Clear();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever(typeof(PrintTaskTest).Assembly));

		Stream OnePageSlowPreviewXlsStream => resourceRetriever.Value.GetStream("Enterprise.DocumentEngine.Test.FlexCelInterface.Testing.OnePageSlowPreviewWithCenterAcrossCellsRemovedFromFirstRow.xls");

		void ZForm_FormCreated(object sender, EventArgs e)
		{
			lastFormCreated = sender as ZForm;
			itemsToDispose.Add(lastFormCreated);
		}

		ZForm lastFormCreated;

		readonly List<IDisposable> itemsToDispose = new List<IDisposable>();
	}
}
