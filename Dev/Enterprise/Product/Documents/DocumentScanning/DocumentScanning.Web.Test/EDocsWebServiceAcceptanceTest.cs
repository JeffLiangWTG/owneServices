using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineIntegration;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Services.Testing
{
	sealed class EDocsWebServiceAcceptanceTest : TestCaseWithFactory
	{
		public void TestAddTwoEDocsDeleteOneThenDeliver()
		{
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipient.Schema.TableName);
			TestCaseHelper.ClearTable(StmPrintJob.Schema.TableName);

			var service = new EDocsService();
			service.AddOrUpdateEDoc(
				tablePrefix,
				businessObjectPK,
				new EDocDetail()
				{
					DocumentTypePK = mscDocType.PK.ToGuid(),
					Description = "HELLO WORLD",
					FileName = "EDoc 1",
					IsPublished = true,
				},
				new byte[] { 6, 7, 8 }, true);

			var eDocs = service.GetEDocDetails(businessObjectPK, false, true);
			AssertEquals(1, eDocs.Length);

			var eDoc1 = eDocs[0];
			AssertEquals(mscDocType.PK, eDoc1.DocumentTypePK);
			AssertEquals("HELLO WORLD", eDoc1.Description);
			AssertEquals("EDoc 1", eDoc1.FileName);
			AssertEquals(true, eDoc1.IsPublished);
			AssertEquals("Organization (UnitTest)", eDoc1.OwnerReadableName);
			var imageData = service.GetEDocImageData(eDoc1.Id, eDoc1.DatabaseNumber, true);
			AssertEquals(new byte[] { 6, 7, 8 }, imageData.Data);
			AssertEquals("EDoc 1", imageData.FullFileName);

			service.AddOrUpdateEDoc(tablePrefix, businessObjectPK,
				new EDocDetail()
				{
					DocumentTypePK = invoiceDocType.PK.ToGuid(),
					Description = "GOOD NIGHT",
					FileName = "EDoc 2",
					IsPublished = false,
				},
				new byte[] { 2, 4, 16 }, true);

			eDocs = service.GetEDocDetails(businessObjectPK, false, true);
			AssertEquals(2, eDocs.Length);

			var eDoc2 = eDocs[1];
			AssertEquals(invoiceDocType.PK, eDoc2.DocumentTypePK);
			AssertEquals("GOOD NIGHT", eDoc2.Description);
			AssertEquals("EDoc 2", eDoc2.FileName);
			AssertEquals(false, eDoc2.IsPublished);
			AssertEquals("Organization (UnitTest)", eDoc2.OwnerReadableName);
			imageData = service.GetEDocImageData(eDoc2.Id, eDoc2.DatabaseNumber, true);
			AssertEquals(new byte[] { 2, 4, 16 }, imageData.Data);
			AssertEquals("EDoc 2", imageData.FullFileName);

			service.DeleteEDoc(eDoc1.Id, eDoc1.DatabaseNumber, true);

			eDocs = service.GetEDocDetails(businessObjectPK, true, true);
			AssertEquals(2, eDocs.Length);

			eDocs = service.GetEDocDetails(businessObjectPK, false, true);
			AssertEquals(1, eDocs.Length);
			eDoc2 = eDocs[0];
			AssertEquals("EDoc 2", eDoc2.FileName);

			var recipient = new DeliveryRecipientBase();
			recipient.Name = "Steve Jobs";
			recipient.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			recipient.Email = "unit.test@cargowise.com";
			recipient.EmailAttachmentType = OrgConstants.AttachmentType.TIF;

			var deliveryInstructions = new DeliveryInstructionsBase();
			deliveryInstructions.Recipients = new[] { recipient };

			service.DeliverEDoc(eDoc2.Id, eDoc2.DatabaseNumber, deliveryInstructions, true);

			var printJobs = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals(1, printJobs.Length);

			var printJob = printJobs[0];
			AssertEquals("unit.test@cargowise.com", printJob.SP_Destination);
			AssertEquals(new byte[] { 2, 4, 16 }, printJob.SP_CustomProperties);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			(new DocManagerDBHelper()).LastWritableDatabaseWithFreeSpace(); //to ensure SD001 exists
			var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);

			var parent = documentFactory.New<OrgHeader>();
			parent.OH_Code = "UnitTest";

			documentFactory.Save();

			tablePrefix = parent.TablePrefix;
			businessObjectPK = parent.PK.ToGuid();

			mscDocType = Factory.LoadTop1<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, Core.Constants.RefDocTypes.MiscellaneousDocument));
			invoiceDocType = Factory.LoadTop1<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, Core.Constants.RefDocTypes.Invoice));
		}

		string tablePrefix;
		Guid businessObjectPK;
		RefDocType mscDocType;
		RefDocType invoiceDocType;

		#endregion
	}
}
