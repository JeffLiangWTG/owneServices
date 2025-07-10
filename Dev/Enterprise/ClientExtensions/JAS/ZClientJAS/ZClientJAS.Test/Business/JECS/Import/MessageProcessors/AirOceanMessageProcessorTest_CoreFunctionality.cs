using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Client.JAS.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	internal class AirOceanMessageProcessorTest_CoreFunctionality : JXCMessageProcessorBaseTest
	{
		public void TestProcessREFRRecord()
		{
			JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
			AssertEquals("Pre-condition", "", shipment.JS_BookingReference);
			Processor.ProcessREFRRecords(shipment, 1);
			AssertEquals("No REFR record before house-level (OHBL) record found", "", shipment.JS_BookingReference);
			Processor.ProcessREFRRecords(shipment, 2);
			AssertEquals("SM6180 / PO #9716", shipment.JS_BookingReference);
			Processor.ProcessREFRRecords(shipment, 4);
			AssertEquals("65050172", shipment.JS_BookingReference);
		}

		public void TestProcessSHMKRecord()
		{
			JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
			AssertEquals("Pre-condition", "", shipment.JS_BookingReference);
			Processor.ProcessSHMKRecords(shipment, 1);
			AssertEquals("No SHMK record before house-level (OHBL) record found", "", shipment.JS_MarksAndNumbers);
			Processor.ProcessSHMKRecords(shipment, 2);
			AssertEquals("No SHMK records before the next house-level record found", "", shipment.JS_MarksAndNumbers);
			Processor.ProcessSHMKRecords(shipment, 4);
			AssertEquals("TESTING!@# BLAHBLAH", shipment.JS_MarksAndNumbers);
		}

		public void TestNotifyConsolNotUpdated()
		{
			NotificationBuffer notificationBuffer = new NotificationBuffer();
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			Processor.NotifyConsolNotUpdated(consol, notificationBuffer);
			AssertEquals(1, notificationBuffer.Events.Length);
			AssertEquals(consol.HumanReadableName + " already exist and the automatic update feature is disabled in the registry settings. Consol will not be updated.", ((WarningNotification)notificationBuffer.Events[0]).AdditionalInfo);
		}

		[ExpectNoExceptions]
		public void TestNotifyConsolNotUpdated_NullParam()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			Processor.NotifyConsolNotUpdated(consol, null);
		}

		public void TestNotifyShipmentNotUpdated()
		{
			NotificationBuffer notificationBuffer = new NotificationBuffer();
			JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
			Processor.NotifyShipmentNotUpdated(shipment, notificationBuffer);
			AssertEquals(1, notificationBuffer.Events.Length);
			AssertEquals(shipment.HumanReadableName + " already exist and the automatic update feature is disabled in the registry settings. Shipment will not be updated.", ((WarningNotification)notificationBuffer.Events[0]).AdditionalInfo);
		}

		[ExpectNoExceptions]
		public void TestNotifyShipmentNotUpdated_NullParam()
		{
			JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
			Processor.NotifyShipmentNotUpdated(shipment, null);
		}

		#region TestProcessDummyShipment
		public void TestProcessDummyShipment()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			AssertEquals("Pre-condition", 0, consol.Shipments.Count);
			Assert("Not a DummyHouse record, should return false", !Processor.ProcessDummyShipment(consol, 1, NotificationBuffer));
			AssertEquals("Not a DummyHouse record, should not be imported", 0, consol.Shipments.Count);
			Assert("Should be successful", Processor.ProcessDummyShipment(consol, 14, NotificationBuffer));
			AssertEquals("Should be imported from DOHB record", 1, consol.Shipments.Count);
			AssertEquals("HB103", consol.Shipments[0].JS_HouseBill);
			AssertEquals("Should be updated from DOHB record", "TRAFFICNO#123", consol.Shipments[0].JS_BookingReference);
			AssertBusinessObjectCreatedOrUpdatedNotification((JASForwardingShipment)consol.Shipments[0]);
		}

		public void TestProcessDummyShipment_UpdateExistingShipment()
		{
			JASForwardingShipment shipment = CreateExistingShipment();
			JASDataRegistry.Instance.EnableAutoUpdateOnImport = true;
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			AssertEquals("Pre-condition", 0, consol.Shipments.Count);
			Assert("Should be successful", Processor.ProcessDummyShipment(consol, 14, NotificationBuffer));
			AssertEquals("Should be imported from DOHB", 1, consol.Shipments.Count);
			AssertEquals("Should be attached to the Consol", shipment.PK, consol.Shipments[0].PK);
			AssertEquals("Should be updated from DOHB record", "TRAFFICNO#123", consol.Shipments[0].JS_BookingReference);
			AssertBusinessObjectCreatedOrUpdatedNotification((JASForwardingShipment)consol.Shipments[0]);
		}

		public void TestProcessDummyShipment_ShouldNotUpdateExistingShipment()
		{
			JASForwardingShipment shipment = CreateExistingShipment();
			JASDataRegistry.Instance.EnableAutoUpdateOnImport = false;
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			AssertEquals("Pre-condition", 0, consol.Shipments.Count);
			Assert("Should be successful", Processor.ProcessDummyShipment(consol, 14, NotificationBuffer));
			AssertEquals("Should be imported from DOHB", 1, consol.Shipments.Count);
			AssertEquals("Should be attached to the Consol", shipment.PK, consol.Shipments[0].PK);
			AssertEquals("Should not update from DOHB record", "", consol.Shipments[0].JS_BookingReference);
			INotification[] warningNotifications = NotificationBuffer.GetEventsByType(WarningType.Warning);
			AssertEquals(1, warningNotifications.Length);
			AssertEquals(shipment.HumanReadableName + " already exist and the automatic update feature is disabled in the registry settings. Shipment will not be updated.", ((INotificationSubscriberNotification)warningNotifications[0]).AdditionalInfo);
		}

		JASForwardingShipment CreateExistingShipment()
		{
			JASForwardingShipment shipment = Factory.NewWithValidTestData<JASForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_HouseBill = "HB103";
			shipment.JS_RL_NKOrigin = "";
			Factory.Save();
			return shipment;
		}

		void AssertBusinessObjectCreatedOrUpdatedNotification(JASForwardingShipment shipment)
		{
			BusinessObjectCreatedOrUpdatedNotification[] notifications = GetBusinessObjectCreatedOrUpdatedNotification(NotificationBuffer, typeof(JASForwardingShipment));
			AssertEquals(1, notifications.Length);
			AssertEquals(shipment, notifications[0].BusinessEntity);
		}

		#endregion
		#region TestLoadOrCreateJob
		public void TestLoadOrCreateJob_ReceivingForwarderOrControllingBranchDoesNotExist()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			JASForwardingShipment shipment = (JASForwardingShipment)consol.Shipments.AddNew();
			OrgHeader forwarder = new BusinessObjectFactory().NewWithValidTestData<OrgHeader>();
			forwarder.OH_Code = "ZUB29123";
			forwarder.OH_IsDebtor = true;
			forwarder.Factory.Save();
			consol.SetDefaultReceivingForwarderAddress(forwarder);
			AssertNull("Branch code is not specified for receiving forwarder, should not be able to create job", Processor.LoadOrCreateJob(shipment, null, NotificationBuffer));
			AssertLastNotificationIsWarningWhenJobCannotBeCreated(NotificationBuffer, "Cannot create Invoice for the Shipment Job as the Invoice branch cannot be determined from the Receiving Forwarder");
			shipment.Consols.RemoveAll();
			AssertNull("Receiving forwarder cannot be found, should not be able to create job", Processor.LoadOrCreateJob(shipment, null, NotificationBuffer));
			AssertLastNotificationIsWarningWhenJobCannotBeCreated(NotificationBuffer, "Cannot create Invoice for the Shipment Job as the Invoice branch cannot be determined from the Receiving Forwarder");
			consol.ReceivingForwarder.NettingCode = "AUCOR";
			consol.ReceivingForwarder.OfficeCode = "AUSYD";
			AssertNull("Branch code is not specified for receiving forwarder, should not be able to create job", Processor.LoadOrCreateJob(shipment, null, NotificationBuffer));
			AssertLastNotificationIsWarningWhenJobCannotBeCreated(NotificationBuffer, "Cannot create Invoice for the Shipment Job as the Invoice branch cannot be determined from the Receiving Forwarder");
		}

		public void TestLoadOrCreateJob_WithoutConsol()
		{
			JASForwardingShipment shipment = CreateShipment(null, Core.Constants.TransportModes.Air, "ITMIL", "AUMEL", true, false);
			JASOrgHeader receivingForwarder = new BusinessObjectFactory().NewWithValidTestData<JASOrgHeader>();
			receivingForwarder.OH_Code = "ZUB29123";
			receivingForwarder.OH_IsDebtor = true;
			receivingForwarder.NettingCode = "AUCOR";
			receivingForwarder.OfficeCode = "AUSYD";
			receivingForwarder.CompanyData.OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;
			receivingForwarder.Factory.Save();
			using (Job createdJob = Processor.LoadOrCreateJob(shipment, null, NotificationBuffer))
			{
				AssertCreatedJob(createdJob, "FIA", shipment, shipment.Consignee.PK, ZGuid.Empty);
			}
		}

		public void TestLoadOrCreateJob_CreatingNewJobSuccessfully()
		{
			JASForwardingConsol consol = CreateConsol(true, true, true);
			JASForwardingShipment shipment = CreateShipment(consol, Core.Constants.TransportModes.Air, "IDJKT", "AUSYD", true, true);
			OrgHeader freightBillTo = shipment.Consignee.DeliveryFreightBillTo;
			using (Job createdJob = Processor.LoadOrCreateJob(shipment, null, NotificationBuffer))
			{
				AssertCreatedJob(createdJob, "FIA", shipment, freightBillTo.PK, consol.SendingForwarder.PK);
			}

			shipment = CreateShipment(consol, "", "IDJKT", "AUSYD", true, true);
			using (Job createdJob = Processor.LoadOrCreateJob(shipment, null, NotificationBuffer))
			{
				AssertCreatedJob(createdJob, "BRN", shipment, freightBillTo.PK, consol.SendingForwarder.PK);
			}

			Factory.LoadFromNaturalKey(typeof(GlbDepartment), GlbDepartmentSchema.GE_Code, "BRN").Delete();
			shipment = CreateShipment(consol, "", "IDJKT", "AUSYD", true, true);
			using (Job createdJob = Processor.LoadOrCreateJob(shipment, null, NotificationBuffer))
			{
				ZString expectedDeptCode = (Factory.LoadTop1<GlbDepartment>(new ZQuery())).GE_Code;
				AssertCreatedJob(createdJob, expectedDeptCode, shipment, freightBillTo.PK, consol.SendingForwarder.PK);
			}

			shipment = CreateShipment(consol, Core.Constants.TransportModes.Sea, "IDJKT", "AUSYD", true, false);
			using (Job createdJob = Processor.LoadOrCreateJob(shipment, null, NotificationBuffer))
			{
				AssertCreatedJob(createdJob, "FIS", shipment, shipment.ConsigneePK, consol.SendingForwarder.PK);
			}

			shipment = CreateShipment(consol, Core.Constants.TransportModes.Sea, "IDJKT", "AUSYD", false, false);
			consol.SetDefaultSendingForwarderAddress(ZGuid.Empty);
			using (Job createdJob = Processor.LoadOrCreateJob(shipment, null, NotificationBuffer))
			{
				AssertCreatedJob(createdJob, "FIS", shipment, ZGuid.Empty, ZGuid.Empty);
			}
		}

		public void TestLoadOrCreateJob_ReceivingForwarderIsProxyToCurrentBranch()
		{
			JASForwardingConsol consol = CreateConsol(true, false, true);
			Factory.Save();
			GlbBranch currentBranch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			currentBranch.GB_OH_OrgProxy = consol.ReceivingForwarder.PK;
			Factory.Save();
			JASForwardingShipment shipment = CreateShipment(consol, Core.Constants.TransportModes.Air, "IDJKT", "AUSYD", true, true);
			using (Job createdJob = Processor.LoadOrCreateJob(shipment, null, NotificationBuffer))
			{
				AssertCreatedJob(createdJob, "FIA", shipment, shipment.Consignee.DeliveryFreightBillTo.PK, consol.SendingForwarder.PK);
			}
		}

		[ExpectNoExceptions]
		public void TestLoadOrCreateJob_CreatedJobCanBeSaved()
		{
			JASForwardingConsol consol = CreateConsol(true, true, true);
			JASForwardingShipment shipment = CreateShipment(consol, Core.Constants.TransportModes.Air, "IDJKT", "AUSYD", true, true);
			using (Job createdJob = Processor.LoadOrCreateJob(shipment, null, NotificationBuffer))
			{
				Factory.Save();
			}

			shipment = CreateShipment(consol, Core.Constants.TransportModes.Sea, "IDJKT", "AUSYD", true, false);
			using (Job createdJob = Processor.LoadOrCreateJob(shipment, null, NotificationBuffer))
			{
				Factory.Save();
			}

			shipment = CreateShipment(consol, Core.Constants.TransportModes.Sea, "IDJKT", "AUSYD", false, false);
			consol.SetDefaultSendingForwarderAddress(ZGuid.Empty);
			using (Job createdJob = Processor.LoadOrCreateJob(shipment, null, NotificationBuffer))
			{
				Factory.Save();
			}
		}

		public void TestLoadOrCreateJob_CreatingNewJobUnsuccessfully()
		{
			JASDataRegistry.Instance.JXCFinancialMessagingNotificationGroupPK = ZGuid.NewZGuid();
			JASForwardingConsol consol = CreateConsol(true, true, true);
			JASForwardingShipment shipment = CreateShipment(consol, Core.Constants.TransportModes.Air, "IDJKT", "AUPER", true, true);
			shipment.JS_UniqueConsignRef = "S001";
			ZGlobalMutex mutex = Job.GetMutex_ForTestOnly(shipment.PK);
			try
			{
				AssertEquals(true, mutex.Lock());
				AssertEquals("Pre-condition", 0, NotificationBuffer.Events.Length);
				IJobChargeData[] jobCharges = new IJobChargeData[5];
				jobCharges[0] = new JobChargeData("AB", "This is Charge 1", "AUD", 190.3948m);
				jobCharges[1] = new JobChargeData("CD", "This is Charge 2", "HKD", 100m);
				jobCharges[2] = new JobChargeData("EF", "This is Charge 3", "IDR", 200m);
				jobCharges[3] = new JobChargeData("GH", "This is Charge 4", "USD", 300m);
				jobCharges[4] = new JobChargeData("IJ", "This is Charge 5", "MYR", 400m);
				using (Job createdJob = Processor.LoadOrCreateJob(shipment, jobCharges, NotificationBuffer))
				{
					AssertNull("Should not be created if mutex cannot be obtained", createdJob);
					AssertEquals(JASDataErrorNotificationType.CannotCreateNewShipmentJob, NotificationBuffer.Events[0].Type);
					AssertEquals(@"You have created the job S001 on another form, but haven't saved it yet.
Please close or save other forms that use job S001 to continue.", ((INotificationSubscriberNotification)NotificationBuffer.Events[0]).AdditionalInfo);
					AssertEquals("There should be a notification email sent", 1, Processor.JASMailSender.EmailsSent.Length);
					AssertEmailTestLoadOrCreateJob_CreatingNewJobUnsuccessfully(shipment, ((INotificationSubscriberNotification)NotificationBuffer.Events[0]).AdditionalInfo);
				}
			}
			finally
			{
				if (mutex.HasLock)
				{
					mutex.Unlock();
				}
			}
		}

		public void TestLoadOrCreateJob_LoadingJobWithoutCharges()
		{
			JASForwardingConsol consol = CreateConsol(true, true, true);
			JASForwardingShipment shipment = CreateShipment(consol, Core.Constants.TransportModes.Air, "IDJKT", "AUPER", true, true);
			using (Job createdJob = Processor.LoadOrCreateJob(shipment, null, NotificationBuffer))
			{
				Factory.Save();
				using (Job newlyLoadedJob = Processor.LoadOrCreateJob(shipment, null, NotificationBuffer))
				{
					AssertNotNull("Should be loaded if no charges have been created for the job", newlyLoadedJob);
				}
			}
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestLoadOrCreateJob_LoadingJobWithDifferentCompanyPK()
		{
			JASForwardingConsol consol = CreateConsol(true, true, true);
			JASForwardingShipment shipment = CreateShipment(consol, Core.Constants.TransportModes.Air, "IDJKT", "AUPER", true, true);
			using (Job createdJob = Processor.LoadOrCreateJob(shipment, null, NotificationBuffer))
			{
				createdJob.JH_GC = Factory.LoadTop1(typeof(GlbCompany), new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK)).PK;
				Factory.Save();
				using (Job newlyLoadedJob = Processor.LoadOrCreateJob(shipment, null, NotificationBuffer))
				{
					AssertNotNull("Should be loaded if the job is associated to a different company", newlyLoadedJob);
				}
			}
		}

		public void TestLoadOrCreateJob_LoadingJobWithCharges()
		{
			IJobChargeData[] jobCharges = new IJobChargeData[3];
			jobCharges[0] = new JobChargeData("AB", "This is Charge 1", "AUD", 190.3948m);
			jobCharges[1] = new JobChargeData("CD", "This is Charge 2", "HKD", 100m);
			jobCharges[2] = new JobChargeData("EF", "This is Charge 3", "IDR", 200m);
			JASForwardingConsol consol = CreateConsol(true, true, true);
			JASForwardingShipment shipment = CreateShipment(consol, Core.Constants.TransportModes.Air, "IDJKT", "AUPER", true, true);
			using (Job createdJob = Processor.LoadOrCreateJob(shipment, null, NotificationBuffer))
			{
				JobCharge charge = createdJob.Charges.AddNew();
				charge.FillWithValidTestData();
				charge.JR_AC = Env.Registry.FreightChargeCode;
				charge.JR_Desc = "Something for freight";
				charge.JR_RX_NKSellCurrency = "IDR";
				charge.JR_OSSellAmt = 10500m;
				Factory.Save();
				using (Job newlyLoadedJob = Processor.LoadOrCreateJob(shipment, jobCharges, NotificationBuffer))
				{
					AssertNull("Should return null when shipment has an existing Job", newlyLoadedJob);
					AssertEquals("There should be a notification email sent", 1, Processor.JASMailSender.EmailsSent.Length);
					AssertEmailTestLoadOrCreateJob_LoadingJobWithCharges(shipment);
					AssertLastNotificationIsWarningWhenJobCannotBeCreated(NotificationBuffer, "Cannot create new Invoice for this Shipment Job as it already has existing charges");
				}
			}
		}

		public void TestLoadOrCreateJob_LoadingJobWithChargesButChargesNotInDatabase()
		{
			JASForwardingConsol consol = CreateConsol(true, true, true);
			JASForwardingShipment shipment = CreateShipment(consol, Core.Constants.TransportModes.Air, "IDJKT", "AUPER", true, true);
			using (Job createdJob = Processor.LoadOrCreateJob(shipment, null, NotificationBuffer))
			{
				Factory.Save();
				JobCharge charge = createdJob.Charges.AddNew();
				charge.FillWithValidTestData();
				charge.JR_AC = Env.Registry.FreightChargeCode;
				charge.JR_Desc = "Something for freight";
				charge.JR_RX_NKSellCurrency = "IDR";
				charge.JR_OSSellAmt = 10500m;
				using (Job newlyLoadedJob = Processor.LoadOrCreateJob(shipment, null, NotificationBuffer))
				{
					AssertEquals("Job charge is not in database, should load the Job", createdJob.PK, newlyLoadedJob.PK);
					AssertEquals("Should not remove the default charges", 1, createdJob.Charges.Count);
					AssertEquals("There should be no notification email sent", 0, Processor.JASMailSender.EmailsSent.Length);
				}
			}
		}

		void AssertCreatedJob(Job createdJob, ZString expectedDeptCode, JASForwardingShipment expectedParent, ZGuid expectedLocalClientPK, ZGuid expectedOverseasAgentPK)
		{
			AssertEquals(GlbBranch.CurrentBranch.PK, createdJob.Branch.PK);
			AssertEquals(GlbCompany.CurrentCompany.PK, createdJob.Company.PK);
			AssertEquals(expectedDeptCode, createdJob.Department.GE_Code);
			AssertEquals(JobShipmentSchema.Constants.Prefix, createdJob.JH_ParentTableCode);
			AssertEquals(expectedParent.PK, createdJob.JH_ParentID);
			AssertEquals(expectedParent.JobNumber, createdJob.JH_JobNum);
			AssertEquals(expectedLocalClientPK, createdJob.LocalChargesPK);
			AssertEquals(expectedOverseasAgentPK, createdJob.AgentCollectPK);
		}

		void AssertEmailTestLoadOrCreateJob_CreatingNewJobUnsuccessfully(JASForwardingShipment shipment, string additionalErrorMessage)
		{
			AssertNull("There should be no attachment", Processor.JASMailSender.LastAttachmentFileName);
			AssertEquals("Cannot create Invoices for " + shipment.HumanReadableName, Processor.JASMailSender.LastSubject);
			AssertEquals("Should be sent to the Financial Messaging group", JASDataRegistry.Instance.JXCFinancialMessagingNotificationGroupPK, Processor.JASMailSender.LastNotificationGroupPK);
			string expectedBodyText = "Invoice cannot be created due to the following reason: " + additionalErrorMessage + @"

The below charges are extracted from the JXC file:
AB            This is Charge 1                                    AUD  190.3948       
CD            This is Charge 2                                    HKD  100            
EF            This is Charge 3                                    IDR  200            
GH            This is Charge 4                                    USD  300            
IJ            This is Charge 5                                    MYR  400            

Please update manually.";
			AssertEquals(expectedBodyText, Processor.JASMailSender.LastBody);
		}

		void AssertEmailTestLoadOrCreateJob_LoadingJobWithCharges(JASForwardingShipment shipment)
		{
			AssertNull("There should be no attachment", Processor.JASMailSender.LastAttachmentFileName);
			AssertEquals("Cannot create Invoices for " + shipment.HumanReadableName, Processor.JASMailSender.LastSubject);
			AssertEquals("Should be sent to the Financial Messaging group", JASDataRegistry.Instance.JXCFinancialMessagingNotificationGroupPK, Processor.JASMailSender.LastNotificationGroupPK);
			string expectedBodyText = @"The below charges already attached to the shipment:
FRT           Something for freight                               IDR  10500          

The below charges are extracted from the JXC file:
AB            This is Charge 1                                    AUD  190.3948       
CD            This is Charge 2                                    HKD  100            
EF            This is Charge 3                                    IDR  200            

Please update manually.";
			AssertEquals(expectedBodyText, Processor.JASMailSender.LastBody);
		}

		JASForwardingConsol CreateConsol(bool hasReceivingForwarder, bool receivingForwarderAttachedToCurrentBranch, bool hasSendingForwarder)
		{
			JASForwardingConsol result = Factory.NewWithValidTestData<JASForwardingConsol>();
			if (hasReceivingForwarder)
			{
				JASOrgHeader forwarder = new BusinessObjectFactory().NewWithValidTestData<JASOrgHeader>();
				forwarder.OH_Code = "ZOIJSAD";
				forwarder.OH_IsDebtor = true;
				forwarder.Factory.Save();
				result.SetDefaultReceivingForwarderAddress(forwarder);
				if (receivingForwarderAttachedToCurrentBranch)
				{
					result.ReceivingForwarder.CompanyData.OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;
					forwarder.Factory.Save();
				}
			}

			if (hasSendingForwarder)
			{
				JASOrgHeader forwarder = new BusinessObjectFactory().NewWithValidTestData<JASOrgHeader>();
				forwarder.OH_Code = "POADNJF";
				forwarder.OH_IsDebtor = true;
				forwarder.Factory.Save();
				result.SetDefaultSendingForwarderAddress(forwarder);
			}

			return result;
		}

		JASForwardingShipment CreateShipment(JASForwardingConsol consol, ZString transportMode, ZString origin, ZString dest, bool hasConsignee, bool consigneeHasFreightBillToParty)
		{
			JASForwardingShipment result = (consol != null) ? (JASForwardingShipment)consol.Shipments.AddNew() : Factory.New<JASForwardingShipment>();
			result.FillWithValidTestData();
			result.JS_TransportMode = transportMode;
			result.JS_RL_NKOrigin = origin;
			result.JS_RL_NKDestination = dest;
			if (hasConsignee)
			{
				result.ConsigneePK = Consignee.PK;
				Consignee.SetRelatedParty((consigneeHasFreightBillToParty ? BillTo : Consignee), RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.All, ZString.Empty);
				Consignee.Factory.Save();
			}

			return result;
		}

		JASOrgHeader Consignee
		{
			get
			{
				if (fConsignee == null)
				{
					fConsignee = new BusinessObjectFactory().NewWithValidTestData<JASOrgHeader>();
					fConsignee.OH_Code = "DEHAPO1";
					fConsignee.OH_IsDebtor = true;
					fConsignee.Factory.Save();
				}

				return fConsignee;
			}
		}

		JASOrgHeader fConsignee;
		JASOrgHeader BillTo
		{
			get
			{
				if (fBillTo == null)
				{
					fBillTo = new BusinessObjectFactory().NewWithValidTestData<JASOrgHeader>();
					fBillTo.OH_Code = "APADFA1";
					fBillTo.OH_IsDebtor = true;
					fBillTo.Factory.Save();
				}

				return fBillTo;
			}
		}

		JASOrgHeader fBillTo;
		void AssertLastNotificationIsWarningWhenJobCannotBeCreated(NotificationBuffer notificationBuffer, ZString expectedWarningMessage)
		{
			WarningNotification warning = notificationBuffer.Events[notificationBuffer.Events.Length - 1] as WarningNotification;
			AssertNotNull("Has to be a warning notification", warning);
			AssertEquals(expectedWarningMessage, warning.AdditionalInfo);
		}

		#endregion
		AirOceanMessageProcessorForTest Processor
		{
			get
			{
				if (fProcessor == null)
				{
					fProcessor = new AirOceanMessageProcessorForTest();
				}

				return fProcessor;
			}
		}

		NotificationBuffer NotificationBuffer
		{
			get
			{
				if (fNotificationBuffer == null)
				{
					fNotificationBuffer = new NotificationBuffer();
				}

				return fNotificationBuffer;
			}
		}

		NotificationBuffer fNotificationBuffer;
		AirOceanMessageProcessorForTest fProcessor;
		#region AirOceanMessageProcessorForTest
		class AirOceanMessageProcessorForTest : AirOceanMessageProcessor
		{
			public AirOceanMessageProcessorForTest() : base(new JXCRecord[] { new HEADRecord(JXCConstants.LineTypes.HEAD, "AUSYD;USSEA;AUCOR;USCOR;AUSYD"), new OMANRecord(JXCConstants.LineTypes.OMAN, "n;01406005003509;KAPITAN MASLOV;615SB;VANCOUVER, BC, CANAD;SYDNEY, AUS.;USYVR;CAYVR;AUSYD;AUSYD;23/05/2005;23/05/2005;13/06/2005"), new OHBLRecord(JXCConstants.LineTypes.OHBL, "n;01406005003508;USSEA;406005003508;30;USYVR;FANE;FESCO AUSTRALIA NORTH AMERICA LINE;1087 DOWNTOWER BLVD;SUITE 100;MOBILE, AL 36609;;NA0059288;CONTINENTAL FOOD SALES INC;600 WINSLOW WAY E;SUITE 231;BAINBRIDGE ISLAND, WA 98110;Not Known;wa;;CA;;Y;2068427440;COFO00;;ATYS AUSTRALIA;200 GEORGE DOWNES DR.;CENTRAL MANGROVE;NSW AUSTRALIA 2250;SYDNEY;00;00000;AU;;Y;;COFO00ATYS AUS;;JAS FORWARDING (USA), INC. SEA;11521 EAST MARGINAL WAY, SUITE 120;SEATTLE, WA 98168;;;2122R/055731;VANCOUVER, BC, CANAD;SAME AS CONSIGNEE;;;;;;N;;;;;;;N;;AUSYD;USD;0;P;SYDNEY, AUS.;KAPITAN MASLOV;Voy:615SB;CA;;23/05/2005;;;;1;PCS;51200.000;L;0;4362.00;40FT REEFER CONTAINER SAID TO CONTA;1600-30# CASES FROZEN IQF;BLUEBERRIES;(VENTS CLOSED-TEMP.@-18 DEGREES C);CAED#02A200BC080820050500023;EXPRESS RELEASE;;;08;Optional Field;;23/05/2005;SEATTLE;UTC;JAS FORWARDING WORLDWIDE PTY LTD;UNIT 12, BLDG C, 2-12 BEAUCHAMP RD;BANKSMEADOW NSW 2019;AUSTRALIA;Tel:01161283362888;Fax:01161283362800;NOT KNOWN;LADEN ON BOARD/05.23.05/;ORIGIN;;;One;;1;;N"), new REFRRecord(JXCConstants.LineTypes.REFR, "SM6180 / PO #9716;S"), new OHBLRecord(JXCConstants.LineTypes.OHBL, "n;NV05050449;HKHKG;NV05050449;30;HKHKG;OOCL;OOCL;;;;;;POLK AUDIO;C/O ORIENTAL LOGISTICS CO., LTD;1-11 KA TING ROAD,;KWAI CHUNG, N.T.,;HKHKG;N/A;;HK;;n;;93-18117;;ASSOCIATED MARKETING GROUP;88 ENTERPRISE AVENUE;BERWICK (MELBOURNE), 3806;AUSTRALIA;MEL;N/A;;AU;;n;;10985;(10985);JAS FORWARDING (HK) LIMITED;UNIT B, 5/F., MTL WARHOUSE BULIDING;PHASE 1 BERTH ONE, KWAI CHUNG;CONTAINER TERMINALS, KWAI CHUNG NT;;;HONG KONG;SAME AS CONSIGNEE;;;;;;N;;;;;;;N;;AUMEL;USD;184.80;C;MELBOURNE;CSCL KELANG;068S;HK;;16/05/2005;ALL OTHER DESTINATION CHARGES INCLUDING CUSTOMS CLEARANCE & ;CHARGES TO BE COLLECT AS ARRANGED;;2;CTN;702.770;K;4.620;;SAID TO CONTAIN;;;PLTS(55 CTNS);HI-FI LOUDSPEAKER   ;   ;  ; -FREIGHT COLLECT- ;ZZ;TWO (2) PALLETS ONLY;;16/05/2005;HONG KONG;CCT;JAS FORWARDING WORLDWIDE PTY LTD;GROUND FLOOR, THE MILLS;200 ARDEN STREET, NORTH MELBOURNE,;VICTORIA 3051, AUSTRALIA;MELBOURNE;;HKHKG;;;;;3;MELBOURNE;0;;N"), new CONTRecord(JXCConstants.LineTypes.CONT, "TRIU8461602;NOT KNOWN;;4;51200.000;1.00;1;BELONG TO ANOTHER ONE;;;0;USD"), new CONTRecord(JXCConstants.LineTypes.CONT, "TRIU8461603;NOT KNOWN;;4;51200.000;1.00;1;bElong to another one;;;0;USD"), new CHGSRecord(JXCConstants.LineTypes.CHGS, "123;Charges3;1.00;c;USD"), new CONTRecord(JXCConstants.LineTypes.CONT, "TRIU8461604;NOT KNOWN;;4;51200.000;1.00;1;bElong to another ONE;;;0;USD"), new REFRRecord(JXCConstants.LineTypes.REFR, "65050178;S"), new CHGSRecord(JXCConstants.LineTypes.CHGS, "456;Charges1;2.00;P;AUD"), new REFRRecord(JXCConstants.LineTypes.REFR, "65050171;S"), new REFRRecord(JXCConstants.LineTypes.REFR, "65050172;S"), new REFRRecord(JXCConstants.LineTypes.REFR, "65050173;C"), new SHMKRecord(JXCConstants.LineTypes.SHMK, "TESTING!@#;BLAHBLAH;forty"), new DummyHouseRecord(JXCConstants.LineTypes.DOHB, "N;TRAFFICNO#123;AUCOR;HB103"), new TRLRRecord(JXCConstants.LineTypes.TRLR, "") })
			{
			}

			public new void ProcessREFRRecords(JASForwardingShipment shipment, int startingIndex)
			{
				base.ProcessREFRRecords(shipment, startingIndex);
			}

			public new void ProcessSHMKRecords(JASForwardingShipment shipment, int startingIndex)
			{
				base.ProcessSHMKRecords(shipment, startingIndex);
			}

			public new bool ProcessDummyShipment(JASForwardingConsol consol, int bodyRecordIndex, INotifications notificationSubscriber)
			{
				return base.ProcessDummyShipment(consol, bodyRecordIndex, notificationSubscriber);
			}

			public new Job LoadOrCreateJob(JASForwardingShipment shipment, IJobChargeData[] chargesData, INotifications notificationSubscriber)
			{
				return base.LoadOrCreateJob(shipment, chargesData, notificationSubscriber);
			}

			public new void NotifyConsolNotUpdated(JASForwardingConsol consol, INotifications notificationSubscriber)
			{
				base.NotifyConsolNotUpdated(consol, notificationSubscriber);
			}

			public new void NotifyShipmentNotUpdated(JASForwardingShipment shipment, INotifications notificationSubscriber)
			{
				base.NotifyShipmentNotUpdated(shipment, notificationSubscriber);
			}

			protected override bool ProcessRecordsCore(BusinessObjectFactoryProvider factoryProvider, INotifications notificationSubscriber)
			{
				return false;
			}

			protected override Type FirstLineType
			{
				get
				{
					return typeof(DummyRecord);
				}
			}

			protected override bool ProcessStandardShipment(IFactoryProvider factoryProvider, int bodyRecordIndex, INotifications notificationSubscriber)
			{
				return false;
			}

			public new DummyJASMailSender JASMailSender
			{
				get
				{
					return (DummyJASMailSender)base.JASMailSender;
				}
			}

			protected override JASMailSender GetNewJASMailSender()
			{
				return new DummyJASMailSender();
			}

			protected override bool IsHouseLevelRecord(JXCRecord record)
			{
				return record.LineType == JXCConstants.LineTypes.OHBL || record.LineType == JXCConstants.LineTypes.DOHB;
			}
		}
		#endregion
	}
}
