using System;
using System.IO;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class ReportPrintSetEndToEndTestHelper : Assertion
	{
		readonly BusinessObjectFactory factory;

		public ReportPrintSetEndToEndTestHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;

			template = factory.New<StmTemplate>();
			menuItem = factory.New<ReportCommand>();

			var pivot = factory.New<StmMenuTemplatePivot>();
			pivot.SI_SU = menuItem.PK;
			pivot.SI_SO = template.PK;
		}

		public void SetTemplate(string templateContents)
		{
			Template.SO_Template = DocumentEngineTestHelper.CreateTemplateFromString(templateContents);
		}

		public void AssertRun(string expected)
		{
			var deliveryInstructions = new DeliveryInstructions();
			deliveryInstructions.Destination = DeliveryInstructionDestination.TakenFromContact;
			deliveryInstructions.Recipients.RemoveAndDeleteAll();
			var recipient = deliveryInstructions.Recipients.AddNew();
			recipient.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
			recipient.AttachmentType = "PDF";
			recipient.Email = "unit.test@cargowise.com";

			AssertRun(expected, deliveryInstructions);
		}

		public void AssertRun(string expected, DeliveryInstructions deliveryInstructions)
		{
			using (var printSet = new ReportPrintSet(menuItem))
			{
				AssertRun(expected, printSet, deliveryInstructions);
			}
		}

		public void AssertRun(string expected, ReportPrintSet printSet, DeliveryInstructions deliveryInstructions)
		{
			using (var excelInterface = new ExcelInterface())
			{
				var printJob = Run(printSet, deliveryInstructions);
				using (var stream = new MemoryStream(printJob.SP_CustomProperties))
				{
					excelInterface.LoadExcelFile(stream);
					var result = excelInterface.WorkSheets[0].ToString();
					AssertMultilineASCIIEquals("Comparing resulting print job...", expected, result);
				}
			}
		}

		public void AssertRunWithException<T>(ReportPrintSet printSet, DeliveryInstructions deliveryInstructions) where T : Exception
		{
			using (var excelInterface = new ExcelInterface())
			{
				AssertExceptionThrown<T>(() => Run(printSet, deliveryInstructions));
			}
		}

		internal StmPrintJob Run(ReportPrintSet printSet, DeliveryInstructions deliveryInstructions)
		{
			var printJobs = new StmPrintJobCollection(factory);
			printJobs.Load();
			printJobs.RemoveAndDeleteAll();
			printSet.Run(deliveryInstructions);
			printJobs.Load();

			Assert("There should be at least one print job created.", printJobs.Count > 0);
			return printJobs[0];
		}

		public StmTemplate Template { get { return template; } }
		public ReportCommand MenuItem { get { return menuItem; } }

		readonly StmTemplate template;
		readonly ReportCommand menuItem;
	}
}
