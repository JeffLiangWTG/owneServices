using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	[TestedType(typeof(PrintTaskSettings))]
	sealed class PrintTaskSettingsTest : NonPersistentBusinessObjectTestCase
	{
		#region Test Properties

		#region IsDraft

		public void TestIsDraft()
		{
			AssertEquals(ZBool.False, TaskSettings.IsDraft);

			TaskSettings.IsDraft = ZBool.True;
			AssertEquals(ZBool.True, TaskSettings.IsDraft);
			foreach (DeliveryInstructions instruction in TaskSettings.DocPacksDeliveryInstructions)
			{
				AssertEquals(ZBool.True, instruction.IsDraft);
			}

			TaskSettings.IsDraft = ZBool.False;
			AssertEquals(ZBool.False, TaskSettings.IsDraft);
			foreach (DeliveryInstructions instruction in TaskSettings.DocPacksDeliveryInstructions)
			{
				AssertEquals(ZBool.False, instruction.IsDraft);
			}
		}

		public void TestIsDraftInfo()
		{
			AssertEquals("IsDraft", TaskSettings.IsDraftInfo.Name);
		}

		#endregion

		#region HasPrintedDocuments

		public void TestHasPrintedDocuments()
		{
			foreach (DeliveryInstructions instruction in TaskSettings.DocPacksDeliveryInstructions)
			{
				AssertEquals("Precondition: Instructions should not have printed documents", false, instruction.HasPrintedDocuments);
			}
			AssertEquals(false, TaskSettings.HasPrintedDocuments);

			DocDeliveryContact contact = new DocDeliveryContact(Factory);
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
			TaskSettings.DocPacksDeliveryInstructions[0].Recipients.Add(contact);
			AssertEquals(true, TaskSettings.DocPacksDeliveryInstructions[0].HasPrintedDocuments);
			AssertEquals(true, TaskSettings.HasPrintedDocuments);

			TaskSettings.DocPacksDeliveryInstructions[0].Recipients.RemoveAll();
			TaskSettings.DocPacksDeliveryInstructions[1].Recipients.Add(contact);
			AssertEquals(true, TaskSettings.DocPacksDeliveryInstructions[1].HasPrintedDocuments);
			AssertEquals(true, TaskSettings.HasPrintedDocuments);
		}

		#endregion

		#region AllowPreview

		public void TestAllowPreview()
		{
			AssertEquals("Precondition: Task should have 3 document packs", 3, TaskSettings.PrintTask.Count);
			AssertEquals("Testing default value", ZBool.True, TaskSettings.AllowPreview);

			TaskSettings.AllowPreview = ZBool.False;
			AssertEquals(ZBool.False, TaskSettings.AllowPreview);

			TaskSettings.AllowPreview = ZBool.True;
			AssertEquals(ZBool.True, TaskSettings.AllowPreview);

			for (int count = 0; count < PrintTask.MaxPreviewCount; count++)
			{
				TaskSettings.PrintTask.Add(new DocumentPack());
			}
			AssertEquals("Task should have more than 50 document packs", true, TaskSettings.PrintTask.Count > 50);
			TaskSettings.AllowPreview = ZBool.True;
			AssertEquals(ZBool.False, TaskSettings.AllowPreview);
			TaskSettings.AllowPreview = ZBool.False;
			AssertEquals(ZBool.False, TaskSettings.AllowPreview);
		}

		#endregion

		#region MultipleDocumentPacks

		public void TestMultipleDocumentPacks()
		{
			AssertEquals("Precondition: Task should have more than one document pack", true, TaskSettings.PrintTask.Count > 1);
			AssertEquals(ZBool.True, TaskSettings.MultipleDocumentPacks);

			PrintTask task = new PrintTask();
			TaskSettings = new PrintTaskSettings(task);
			AssertEquals(0, TaskSettings.PrintTask.Count);
			AssertEquals(ZBool.False, TaskSettings.MultipleDocumentPacks);

			task.Add(new DocumentPack());
			TaskSettings = new PrintTaskSettings(task);
			AssertEquals(1, TaskSettings.PrintTask.Count);
			AssertEquals(ZBool.False, TaskSettings.MultipleDocumentPacks);
		}

		#endregion

		#endregion

		#region Related Objects

		public void TestPrintTask()
		{
			PrintTask task = new PrintTask();
			TaskSettings = new PrintTaskSettings(task);
			AssertNotNull(TaskSettings.PrintTask);
			AssertEquals(task, TaskSettings.PrintTask);
		}

		public void TestDocPacksDeliveryInstructions()
		{
			PrintTask task = new PrintTask();
			TaskSettings = new PrintTaskSettings(task);

			AssertEquals(0, TaskSettings.DocPacksDeliveryInstructions.Count);

			DocumentPack pack1 = new DocumentPack();
			task.Add(pack1);
			AssertEquals(1, TaskSettings.DocPacksDeliveryInstructions.Count);
			AssertCollectionContains(pack1.DeliveryInstructions, TaskSettings.DocPacksDeliveryInstructions);

			DocumentPack pack2 = new DocumentPack();
			task.Add(pack2);
			AssertEquals(2, TaskSettings.DocPacksDeliveryInstructions.Count);
			AssertCollectionContains(pack2.DeliveryInstructions, TaskSettings.DocPacksDeliveryInstructions);

			DocumentPack pack3 = new DocumentPack();
			task.Add(pack3);
			AssertEquals(3, TaskSettings.DocPacksDeliveryInstructions.Count);
			AssertCollectionContains(pack3.DeliveryInstructions, TaskSettings.DocPacksDeliveryInstructions);
		}

		public void TestModifyDocumentCheckPoint()
		{
			TaskSettings.ModifyDocumentCheckPoint = Env.Security.None;
			AssertEquals(Env.Security.None, TaskSettings.ModifyDocumentCheckPoint);

			TaskSettings.ModifyDocumentCheckPoint = Env.Security.Warehouse;
			AssertEquals(Env.Security.Warehouse, TaskSettings.ModifyDocumentCheckPoint);

			TaskSettings.ModifyDocumentCheckPoint = Env.Security.WhsInventory;
			AssertEquals(Env.Security.WhsInventory, TaskSettings.ModifyDocumentCheckPoint);
		}

		public void TestSingleDocPackInstructions()
		{
			PrintTask task = new PrintTask();
			TaskSettings = new PrintTaskSettings(task);
			AssertNull(TaskSettings.SingleDocPackInstructions);

			DocumentPack pack1 = new DocumentPack();
			task.Add(pack1);
			AssertNotNull(TaskSettings.SingleDocPackInstructions);
			AssertEquals(pack1.DeliveryInstructions, TaskSettings.SingleDocPackInstructions);

			task.Add(new DocumentPack());
			AssertNull(TaskSettings.SingleDocPackInstructions);
		}

		#region DeliveryOptions

		public void TestDeliveryOptions()
		{
			AllowedDeliveryOptions options = AllowedDeliveryOptions.AllExceptPreview;
			PrintTask task = new PrintTask();
			TaskSettings = new PrintTaskSettings(task);
			AssertEquals(AllowedDeliveryOptions.All, TaskSettings.DeliveryOptions);

			TaskSettings.DeliveryOptions = options;
			AssertEquals(options, TaskSettings.DeliveryOptions);

			DocumentPack pack1 = new DocumentPack();
			pack1.DeliveryInstructions.DeliveryOptions = AllowedDeliveryOptions.HardCopyOnly;
			task.Add(pack1);
			AssertEquals(pack1.DeliveryInstructions.DeliveryOptions, TaskSettings.DeliveryOptions);
			AssertNotEquals(options, TaskSettings.DeliveryOptions);

			task.Add(new DocumentPack());
			AssertEquals(options, TaskSettings.DeliveryOptions);
		}

		#endregion

		#region AllowSaveDefaults

		public void TestAllowSaveDefaults()
		{
			PrintTask task = new PrintTask();
			TaskSettings = new PrintTaskSettings(task);
			AssertEquals(true, TaskSettings.AllowSaveDefaults);

			DocumentPack pack1 = new DocumentPack();
			task.Add(pack1);
			pack1.DeliveryInstructions.AllowSaveDefaults = true;
			AssertEquals(true, TaskSettings.AllowSaveDefaults);
			pack1.DeliveryInstructions.AllowSaveDefaults = false;
			AssertEquals(false, TaskSettings.AllowSaveDefaults);

			task.Add(new DocumentPack());
			TaskSettings.AllowSaveDefaults = false;
			AssertEquals(false, TaskSettings.AllowSaveDefaults);
			TaskSettings.AllowSaveDefaults = true;
			AssertEquals(true, TaskSettings.AllowSaveDefaults);

			pack1.DeliveryInstructions.AllowSaveDefaults = false;
			AssertEquals(true, TaskSettings.AllowSaveDefaults);
		}

		#endregion

		#region Destination

		public void TestDestination()
		{
			PrintTask task = new PrintTask();
			TaskSettings = new PrintTaskSettings(task);
			AssertEquals(DeliveryInstructionDestination.None, TaskSettings.Destination);

			DocumentPack pack1 = new DocumentPack();
			task.Add(pack1);
			pack1.DeliveryInstructions.Destination = DeliveryInstructionDestination.Auto;
			AssertEquals(DeliveryInstructionDestination.Auto, TaskSettings.Destination);
			pack1.DeliveryInstructions.Destination = DeliveryInstructionDestination.Disk;
			AssertEquals(DeliveryInstructionDestination.Disk, TaskSettings.Destination);

			task.Add(new DocumentPack());
			TaskSettings.Destination = DeliveryInstructionDestination.DocManager;
			AssertEquals(DeliveryInstructionDestination.DocManager, TaskSettings.Destination);
			TaskSettings.Destination = DeliveryInstructionDestination.Preview;
			AssertEquals(DeliveryInstructionDestination.Preview, TaskSettings.Destination);

			pack1.DeliveryInstructions.Destination = DeliveryInstructionDestination.UserCancelled;
			AssertEquals(DeliveryInstructionDestination.Preview, TaskSettings.Destination);
		}

		#endregion

		#region Printer Delivery Details

		public void TestPrinterDelivery()
		{
			DocDeliveryPrintDetails printDetails = new DocDeliveryPrintDetails(Factory);

			AssertNotNull(TaskSettings.PrinterDelivery);

			TaskSettings.PrinterDelivery = printDetails;
			AssertEquals(printDetails, TaskSettings.PrinterDelivery);
			foreach (DeliveryInstructions instruction in TaskSettings.DocPacksDeliveryInstructions)
			{
				AssertEquals(printDetails, instruction.PrinterDelivery);
			}
		}

		#endregion

		#endregion

		#region Overrides

		public void TestToString()
		{
			AssertEquals("PrintTaskSettings", TaskSettings.ToString());
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var task = new PrintTask();
			task.Add(new DocumentPack());
			task.Add(new DocumentPack());
			task.Add(new DocumentPack());
			return new PrintTaskSettings(task);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TaskSettings = (PrintTaskSettings)GetNewBusinessObject();
			AssertEquals(3, TaskSettings.DocPacksDeliveryInstructions.Count);
		}

		PrintTaskSettings TaskSettings;

		#endregion
	}
}
