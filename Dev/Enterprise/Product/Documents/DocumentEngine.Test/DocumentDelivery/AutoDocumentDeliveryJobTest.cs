using System;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	public abstract class AutoDocumentDeliveryJobTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTooManyRowsExceptionIsHandled()
		{
			var customizedTemplate = Factory.New<StmTemplateBase>();
			customizedTemplate.SO_DataContext = "GenericFreightJob";
			customizedTemplate.SO_Name = "Customized Document Elements";
			var customizedExcelTemplate = new ExcelTemplateForUnitTesting("Customized Document Elements With Too Many Rows in One Section.xlsx", TestFilesSubFolder.DocumentTestFiles);
			customizedTemplate.SO_Template = customizedExcelTemplate.GetAsByteArray();

			var shipment = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			shipment[JobShipmentSchema.JS_IsForwardRegistered] = false;
			var printer = Factory.NewWithValidTestData<StmPrintQueue>();

			Factory.Save();

			var deliveryJob = new AutoDocumentDeliveryJob((IDocumentSupportable)shipment, true, new ZGuid("f4064bef-635d-4f21-b72f-d31d34dac98e"), printer.PK);
			AssertNoExceptionThrown(() => { deliveryJob.Deliver(Notifications); });
			AssertEquals(@"Error generating the template when delivering the following document menu item:
	Menu Path: 
	Menu Name: Cover Sheet
	System Defined: Y
	Parent: Shipment S00001000

The following error occurred:
Too many rows (1048577): The maximum number of rows supported by this file format is 1048576.
Please check your Customized Document Elements template.", Notifications.AsString.Trim());
		}

		public void TestUserDefinedFieldListIsNotNull()
		{
			var job = new MockedDocumentDeliveryJob(BusinessObjectToDeliver, DocumentCommand.PK);
			if (job.DocumentSupportable is IStmNoteParent)
			{
				AssertNotNull(job.GetNewDocumentPrintSetForDelivery_Exposed().UserFieldList);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestDeliveryGroupUsesOverridenEmailSubjectInMenuItem()
		{
			DocumentCommand.SU_EmailSubjectLine = "Hello World";
			OrgDocument.OD_DeliverBy = Core.Constants.ContactNotifyModes.Email;
			Recipient.OC_Email = "unit.test@cargowise.com";
			Factory.Save();

			DocumentDelivery.Deliver(Notifications);

			var result = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals("result.Length", 1, result.Length);

			var printJob = result[0];
			AssertEquals("printJob.DeliveryGroup.SB_EmailSubjectLine", "Hello World", printJob.DeliveryGroup.SB_EmailSubjectLine);
		}

		public void TestDeliverInNonUserInteractiveEnvironmentDoesNotShowAnyForms()
		{
			using (new PrintTaskUIProviderForTestingSuppressor()) // Stop the suppression of notifications to simulate production environment.
			{
				bool originalIsUserInteractive = Globals.IsUserInteractive;

				try
				{
					long originalFormConstructedCount = TestingState.FormConstructedCount;

					Globals.IsUserInteractive = false;

					OrgDocument.OD_DeliverBy = Core.Constants.ContactNotifyModes.Email;
					Recipient.OC_Email = "jimmy.luong@cargowise.com";
					Factory.Save();

					DocumentDelivery.Deliver(Notifications);
					AssertEquals("No new forms should be shown.", originalFormConstructedCount, TestingState.FormConstructedCount);
					AssertDocumentDelivered("jimmy.luong@cargowise.com", Core.Constants.ContactNotifyModes.Email);
				}
				finally
				{
					Globals.IsUserInteractive = originalIsUserInteractive;
				}
			}
		}

		public void TestSerializable()
		{
#pragma warning disable SYSLIB0050 // 'Type.IsSerializable' is obsolete
			AssertEquals("Type must be serializable to function correctly", true, typeof(AutoDocumentDeliveryJob).IsSerializable);
#pragma warning restore SYSLIB0050 // 'Type.IsSerializable' is obsolete
		}

		[ExpectNoExceptions]
		public void TestDeliveringInPrimaryAppDomain()
		{
			OrgDocument.OD_DeliverBy = Core.Constants.ContactNotifyModes.Email;
			Recipient.OC_Email = "clinton@edi.com.au";
			Factory.Save();

			// the first delivery may populate unserializable field values
			DocumentDelivery.Deliver(Notifications);
		}

		public void TestDeliver_WithIncompleteDeliveryInstructions()
		{
			OrgDocument.OD_DeliverBy = Core.Constants.ContactNotifyModes.Email;
			Recipient.OC_Email = ""; // invalid delivery instructions
			Factory.Save();

			DocumentDelivery.Deliver(Notifications);
			AssertDocumentNotDelivered("clinton@edi.com.au", Core.Constants.ContactNotifyModes.Email);
			AssertEquals("Incomplete delivery instructions should produce a notification error", true, Notifications.HasErrors);
			AssertEquals("Error: DeliveryAddress: Please enter an Email Address.", Notifications.AsString.Trim());
		}

		public void TestDeliver_ByEmail()
		{
			OrgDocument.OD_DeliverBy = Core.Constants.ContactNotifyModes.Email;
			Recipient.OC_Email = "clinton@edi.com.au";
			Factory.Save();

			DocumentDelivery.Deliver(Notifications);
			AssertDocumentDelivered("clinton@edi.com.au", Core.Constants.ContactNotifyModes.Email);
		}

		public void TestDeliver_ByFax()
		{
			OrgDocument.OD_DeliverBy = Core.Constants.ContactNotifyModes.Fax;
			Recipient.OC_Fax = "+61280012200";
			Factory.Save();

			DocumentDelivery.Deliver(Notifications);
			AssertDocumentDelivered("+61280012200", Core.Constants.ContactNotifyModes.Fax);
		}

		public void TestDeliver_ByPrinter()
		{
			StmPrintQueue printQueue = Factory.New<StmPrintQueue>();
			printQueue.SQ_QueueName = "TEST";
			Factory.Save();

			AutoDocumentDeliveryJob job = new AutoDocumentDeliveryJob(BusinessObjectToDeliver, false, DocumentCommand.PK, printQueue.PK);
			job.Deliver(Notifications);
			AssertNotNull("queued for printing", Factory.LoadTop1<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_SQ, printQueue.PK)));
		}

		public void TestSendToEDocsFlag()
		{
			AutoDocumentDeliveryJob job = new AutoDocumentDeliveryJob(BusinessObjectToDeliver, false, DocumentCommand.PK);
			AssertEquals("Should NOT have SendToEDocs flag set", false, job.SendToEDocs);

			job = new AutoDocumentDeliveryJob(BusinessObjectToDeliver, false, DocumentCommand.PK, ZGuid.Empty);
			AssertEquals("Should NOT have SendToEDocs flag set", false, job.SendToEDocs);

			job = new AutoDocumentDeliveryJob(BusinessObjectToDeliver, DocumentCommand.PK, true);
			AssertEquals("Should NOT have SendToEDocs flag set", false, job.SendToEDocs);

			job = new AutoDocumentDeliveryJob(BusinessObjectToDeliver, DocumentCommand.PK, false);
			AssertEquals("Should NOT have SendToEDocs flag set", false, job.SendToEDocs);

			job = new AutoDocumentDeliveryJob(BusinessObjectToDeliver, DocumentCommand.PK, ZGuid.Empty, true);
			AssertEquals("Should NOT have SendToEDocs flag set", false, job.SendToEDocs);

			job = new AutoDocumentDeliveryJob(BusinessObjectToDeliver, DocumentCommand.PK, ZGuid.Empty, false);
			AssertEquals("Should NOT have SendToEDocs flag set", false, job.SendToEDocs);

			job = new AutoDocumentDeliveryJob(BusinessObjectToDeliver, DocumentCommand.PK, ZGuid.Empty, false, true);
			AssertEquals("SHOULD have SendToEDocs flag set", true, job.SendToEDocs);

			job = new AutoDocumentDeliveryJob(BusinessObjectToDeliver, DocumentCommand.PK, ZGuid.Empty, false, false);
			AssertEquals("Should NOT have SendToEDocs flag set", false, job.SendToEDocs);
		}

		#region TestIsAutoDocumentDelivery

		public void TestIsAutoDocumentDelivery_Set()
		{
			var runPrecondition = false;
			var runAssertion = false;

			var job = new MockedDocumentDeliveryJob(BusinessObjectToDeliver, DocumentCommand.PK);
			job.ActionOnGettingNewDocumentPrintSetForDelivery +=
				(dps) =>
				{
					AssertEquals("Precondition.", false, dps.IsAutoDocumentDelivery);
					runPrecondition = true;
				};

			job.ActionAfterRunningDeliver +=
				(dps) =>
				{
					AssertEquals("Should have set IsAutoDelivery.", true, dps.IsAutoDocumentDelivery);
					runAssertion = true;
				};

			job.Deliver(Notifications);
			AssertEquals("Should have run precondition.", true, runPrecondition);
			AssertEquals("Should have run assertion.", true, runAssertion);
		}

		public void TestIsAutoDocumentDelivery_NotSet()
		{
			var runPrecondition = false;
			var runAssertion = false;

			var job = new MockedDocumentDeliveryJob(BusinessObjectToDeliver, DocumentCommand.PK);
			job.ActionOnGettingNewDocumentPrintSetForDelivery +=
				(dps) =>
				{
					AssertEquals("Precondition.", false, dps.IsAutoDocumentDelivery);
					runPrecondition = true;
				};

			job.ActionAfterRunningDeliver +=
				(dps) =>
				{
					AssertEquals("Should *not* have set IsAutoDelivery.", false, dps.IsAutoDocumentDelivery);
					runAssertion = true;
				};

			job.Deliver_WithoutSettingFlag(Notifications);
			AssertEquals("Should have run precondition.", true, runPrecondition);
			AssertEquals("Should have run assertion.", true, runAssertion);
		}

		[Serializable]
		class MockedDocumentDeliveryJob : AutoDocumentDeliveryJob
		{
			public MockedDocumentDeliveryJob(IDocumentSupportable businessObject, ZGuid documentCommandPK)
				: base(businessObject, false, documentCommandPK)
			{
			}

#if NETFRAMEWORK
			protected MockedDocumentDeliveryJob(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif

			public void Deliver_WithoutSettingFlag(INotifications notifications)
			{
				DeliverCore(notifications, shouldSetIsAutoDocumentDeliveryFlag: false);
			}

			protected override DocumentPrintSet GetNewDocumentPrintSetForDelivery()
			{
				var printSet = base.GetNewDocumentPrintSetForDelivery();
				ActionOnGettingNewDocumentPrintSetForDelivery?.Invoke(printSet);
				return printSet;
			}

			public DocumentPrintSet GetNewDocumentPrintSetForDelivery_Exposed()
			{
				return base.GetNewDocumentPrintSetForDelivery();
			}

			protected override void OnDeliveredBeforeDocumentPrintSetDisposed(DocumentPrintSet documentPrintSet)
			{
				ActionAfterRunningDeliver?.Invoke(documentPrintSet);
				base.OnDeliveredBeforeDocumentPrintSetDisposed(documentPrintSet);
			}

			public Action<DocumentPrintSet> ActionOnGettingNewDocumentPrintSetForDelivery;
			public Action<DocumentPrintSet> ActionAfterRunningDeliver;
		}

		#endregion

		public void TestDeliver_AddToEDocs()
		{
			AutoDocumentDeliveryJob docDelivery = NewDocumentDeliveryJob(true);
			if (docDelivery.OnlySendToDocManager)
			{
				Factory.Save();
				docDelivery.Deliver(Notifications);
				AssertDocumentDelivered("", nameof(PrintType.DDS), true);
			}
			else
			{
				Assert("Not relevant", true);
			}
		}

		[ExpectNoExceptions]
		public void TestDeliver_WithNoDetails()
		{
			RecipientOrganisation.Delete();
			Factory.Save();
			DocumentDelivery.Deliver(Notifications);
		}

		[ExpectNoExceptions]
		public void TestDeliver_WhenJobNotApplicable()
		{
			OrgDocument.OD_DeliverBy = Core.Constants.ContactNotifyModes.Fax;
			Recipient.OC_Fax = "";
			Factory.Save();
			DocumentDelivery.Deliver(Notifications);
			AssertEquals("DeliveryInstructions should have been validated", true, Notifications.HasErrors);
		}

		[ExpectNoExceptions]
		public void TestDeliver_WithInvalidFaxDetails()
		{
			OrgDocument.OD_DeliverBy = Core.Constants.ContactNotifyModes.Fax;
			Recipient.OC_Fax = "";
			Factory.Save();
			DocumentDelivery.Deliver(Notifications);
			AssertEquals("DeliveryInstructions should have been validated", true, Notifications.HasErrors);
		}

		public void TestDeliver_WithNoRecipient()
		{
			DocumentDelivery.Deliver(Notifications);
			AssertEquals("Should receive a warning when there is no recipient", true, Notifications.HasWarnings);
		}

		#region Implementation

		protected abstract IDocumentSupportable NewBusinessObjectToDeliver();
		protected abstract DocumentCommand DocumentCommand { get; }
		protected abstract AutoDocumentDeliveryJob NewDocumentDeliveryJob(bool onlySendToDocManager);

		internal DocumentZQuery documentQuery = new DocumentZQuery(BusinessContext.Shipment, "Delay Alert");

		protected IDocumentSupportable BusinessObjectToDeliver
		{
			get
			{
				if (businessObjectToDeliver == null)
				{
					businessObjectToDeliver = NewBusinessObjectToDeliver();
				}
				return businessObjectToDeliver;
			}
		}
		IDocumentSupportable businessObjectToDeliver;

		protected AutoDocumentDeliveryJob DocumentDelivery
		{
			get
			{
				if (documentDelivery == null)
				{
					documentDelivery = NewDocumentDeliveryJob(false);
				}
				return documentDelivery;
			}
		}
		AutoDocumentDeliveryJob documentDelivery;

		protected OrgHeader RecipientOrganisation
		{
			get
			{
				if (recipientOrganisation == null)
				{
					recipientOrganisation = Factory.NewWithValidTestData<OrgHeader>();
				}
				return recipientOrganisation;
			}
		}
		OrgHeader recipientOrganisation;

		protected OrgContact Recipient
		{
			get
			{
				if (recipient == null)
				{
					recipient = RecipientOrganisation.Contacts.AddNew();
					recipient.OC_ContactName = "Test Contact";
				}
				return recipient;
			}
		}
		OrgContact recipient;

		protected OrgDocument OrgDocument
		{
			get
			{
				if (orgDocument == null)
				{
					orgDocument = Recipient.Documents.AddNew();
					orgDocument.OD_SU_MenuItem = DocumentCommand.PK;
				}
				return orgDocument;
			}
		}
		OrgDocument orgDocument;

		protected NotificationBuffer Notifications
		{
			get
			{
				if (notifications == null)
				{
					notifications = new NotificationBuffer();
				}
				return notifications;
			}
		}
		NotificationBuffer notifications;

		protected void AssertDocumentDelivered(string emailOrFax, string notifyMode)
		{
			AssertDocumentDelivered(emailOrFax, notifyMode, true);
		}

		protected void AssertDocumentNotDelivered(string emailOrFax, string notifyMode)
		{
			AssertDocumentDelivered(emailOrFax, notifyMode, false);
		}

		protected void AssertDocumentDelivered(string emailOrFax, string notifyMode, bool expectDelivery)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(StmPrintJob));
			if (notifyMode == Core.Constants.ContactNotifyModes.Fax)
			{
				query.AddToFilter(JoinCondition.And, StmPrintJobSchema.SP_FaxDestination, emailOrFax);
			}
			else if (notifyMode == Core.Constants.ContactNotifyModes.Email)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(StmPrintJobCopyRecipient), StmPrintJobCopyRecipientSchema.SPR_SP);
				subQuery.AddToFilter(StmPrintJobCopyRecipientSchema.SPR_RecipientType, SQLComparisonOperator.Equal, Enterprise.Core.Constants.CopyRecipientType.EmailToRecipient);
				subQuery.AddToFilter(StmPrintJobCopyRecipientSchema.SPR_EmailAddress, SQLComparisonOperator.Equal, emailOrFax);
				query.AddSubQuery(subQuery, JoinCondition.And);
			}

			StmPrintJob printJob = Factory.LoadTop1<StmPrintJob>(query);
			if (expectDelivery)
			{
				AssertNotNull("Document should be delivered", printJob);
				AssertEquals("Job type should be delivered by " + notifyMode, notifyMode, printJob.SP_JobType);
			}
			else
			{
				AssertNull("Document should NOT be delivered", printJob);
			}
		}

		protected override void SetUp()
		{
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipientSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);
			base.SetUp();
		}

		protected DocumentCommand LoadDocumentCommand(BusinessContext businessContext, string name)
		{
			DocumentZQuery query = new DocumentZQuery(businessContext, name);
			DocumentCommand command = Factory.LoadTop1<DocumentCommand>(query);
			command.Parent = BusinessObjectToDeliver;
			return command;
		}

		#endregion
	}
}
