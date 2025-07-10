using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(UPEDocumentPack))]
	sealed class UPEDocumentPackTest : NonPersistentBusinessObjectCollectionTestCase<UPEDocumentPack>
	{
		public void TestRun()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			AssertEquals("There should be any print jobs", 0, Factory.GetDatabaseCount(typeof(StmPrintJob)));
			GlbStaff staff = CreditNotificationGroup.Staff.AddNew();
			staff.GS_EmailAddress = "sirko@sobaka.com";
			staff.GS_Code = "ZAC";
			Factory.Save();

			UPEDocumentPack pack = new UPEDocumentPack(DocumentCommand, Factory.New<UPEJobDeclaration>(), null, null);
			DeliveryInstructions instructions = new DeliveryInstructions();
			instructions.Destination = DeliveryInstructionDestination.TakenFromContact;

			using (PrintTask printTask = new PrintTask())
			{
				printTask.Add(pack);
				printTask.Run(instructions);
			}

			AssertEquals("There should be 1 print job created", 1, Factory.GetDatabaseCount(typeof(StmPrintJob)));
			StmPrintJob job = Factory.LoadTop1<StmPrintJob>(new ZQuery());
			AssertEquals("Email address should be the same", staff.GS_EmailAddress, job.SP_Destination);
			AssertEquals("Attachment Format", "PDF", job.SP_EmailAttachmentFormat);
		}

		DocumentCommand DocumentCommand
		{
			get { return new UPEDocumentMenuItemLoader(Factory).LoadElectronicCreditNote(); }
		}

		GlbGroup CreditNotificationGroup
		{
			get
			{
				GlbGroup result = Factory.Load<GlbGroup>(UPEDataRegistry.Instance.CreditNotificationGroup);
				if (result == null)
				{
					result = Factory.NewWithValidTestData<GlbGroup>();
					UPEDataRegistry.Instance.CreditNotificationGroup = result.PK.ToGuid();
				}
				return result;
			}
		}

		protected override UPEDocumentPack GetCollectionToTest()
		{
			return new UPEDocumentPack();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new Report(new UPEDocumentPack(), null, Guid.Empty, Core.Constants.DataContext.None);
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}
	}
}
