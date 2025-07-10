using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Customs.JP.Common;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(MSXMessageSendingObjectParent))]
	sealed class MSXMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			return new MSXMessageSendingObjectParent(declaration);
		}

		public void TestSetDefaultValues()
		{
			using (var dir = new TempDirectory())
			using (JPRegistry.Instance.DefaultFolderForExportingMessages.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dir.DirectoryName))
			{
				var declaration = Factory.New<JobDeclaration>();
				var messageSendingObjectParent = new MSXMessageSendingObjectParent(declaration);
				AssertEquals(dir.DirectoryName, messageSendingObjectParent.ExportPath);
			}
		}

		public void TestExportPath()
		{
			var messageSendingObjectParent = new MSXMessageSendingObjectParent(Factory.New<JobDeclaration>());
			var data = DataBoundResourceStrings.GetDataForProperty(messageSendingObjectParent.ExportPathInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Export To", data.Caption);
				AssertEquals("FullDescription", $"The folder the message will be exported to. This can be set in Registry -> {JPRegistry.Instance.DefaultFolderForExportingMessages.GetLocationInEnglish()}", data.FullDescription);
			});
		}

		public void TestSendingObjectsCollection()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var header1 = declaration.CustomsEntryHeaders.AddNew();
			var header2 = declaration.CustomsEntryHeaders.AddNew();
			header1.CH_CEI_Instruction = entryInstruction.PK;
			header2.CH_CEI_Instruction = entryInstruction.PK;
			var messageSendingObjectParent = new MSXMessageSendingObjectParent(declaration);
			AssertEquals(0, messageSendingObjectParent.SendingObjectsCollection.Count);

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction2 = declaration2.CustomsEntryInstructions.AddNew();
			var header3 = declaration2.CustomsEntryHeaders.AddNew();
			var header4 = declaration2.CustomsEntryHeaders.AddNew();
			header3.CH_CEI_Instruction = entryInstruction2.PK;
			header4.CH_CEI_Instruction = entryInstruction2.PK;
			header3.CH_PhaseStatus = CustomsDeclarationPhases.Codes.IDC;
			header4.CH_PhaseStatus = CustomsDeclarationPhases.Codes.EDC;
			var messageSendingObjectParent2 = new MSXMessageSendingObjectParent(declaration2);
			AssertEquals(2, messageSendingObjectParent2.SendingObjectsCollection.Count);
		}
	}
}
