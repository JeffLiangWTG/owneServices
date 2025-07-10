using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.BISI;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(UPEDeclarationQueue))]
	sealed class UPEDeclarationQueueTest : UPEProcessQueueTestCase
	{
		public override void TestReferenceCode()
		{
			Assert("CUS Reference code", Queue.ReferenceCode == "CUS");
		}

		public void TestDeclaration()
		{
			Queue.Parent = Declaration;
			AssertEquals(Declaration, Queue.Declaration);
		}

		public void TestParentBranch()
		{
			AssertNull("Parent", Queue.Parent);
			AssertNull("Parent Branch", Queue.ParentBranch);
			Declaration.JE_GB = ZGuid.Empty;
			Queue.Parent = Declaration;
			AssertNull("Parent Branch", Queue.ParentBranch);
			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "ABC";
			branch.GB_RL_NKHomePort = "AUSYD";
			Declaration.JE_GB = branch.PK;
			AssertEquals("Parent Branch", branch.PK, Queue.ParentBranch.PK);
		}

		public void TestGetFinalisedCusHAWBs_DeclarationIsNull()
		{
			AssertEquals("Should be an empty array if Declaration is null", 0, Queue.GetFinalisedCusHAWBs().Length);
		}

		public void TestGetFinalisedCusHAWBs_DeclarationIsNotCompleted()
		{
			Queue.Parent = Declaration;
			UPECusHAWB hAWB1 = CreateHAWBRelatedToThisDeclaration("101", CustomsQueueCodeDescriptionPairList.Codes.Completed, CommercialQueueCodeDescriptionPairList.Codes.Completed);
			UPECusHAWB hAWB2 = CreateHAWBRelatedToThisDeclaration("102", CustomsQueueCodeDescriptionPairList.Codes.Completed, CommercialQueueCodeDescriptionPairList.Codes.Completed);
			Declaration.CurrentQueue.P4_CustomsQueue = "";
			AssertEquals("Should be an empty array if Declaration is not yet completed", 0, Queue.GetFinalisedCusHAWBs().Length);
		}

		public void TestGetFinalisedCusHAWBs()
		{
			Queue.Parent = Declaration;
			Queue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.CustomsBonding;
			UPECusHAWB hAWB1 = CreateHAWBRelatedToThisDeclaration("101", CargoReportQueueCodeDescriptionPairList.Codes.Completed, CommercialQueueCodeDescriptionPairList.Codes.Chase);
			UPECusHAWB hAWB2 = CreateHAWBRelatedToThisDeclaration("102", DeclarationQueueCodeDescriptionPairList.Codes.CustomsBonding, CommercialQueueCodeDescriptionPairList.Codes.Rebill);
			UPECusHAWB hAWB3 = CreateHAWBRelatedToThisDeclaration("103", CargoReportQueueCodeDescriptionPairList.Codes.Completed, CommercialQueueCodeDescriptionPairList.Codes.OnFile);
			UPECusHAWB hAWB4 = CreateHAWBRelatedToThisDeclaration("104", CargoReportQueueCodeDescriptionPairList.Codes.Completed, CommercialQueueCodeDescriptionPairList.Codes.Completed);
			UPECusHAWB hAWB5 = CreateHAWBRelatedToThisDeclaration("105", CargoReportQueueCodeDescriptionPairList.Codes.Completed, CommercialQueueCodeDescriptionPairList.Codes.Finance);
			UPECusHAWB hAWB6 = CreateHAWBRelatedToThisDeclaration("106", CargoReportQueueCodeDescriptionPairList.Codes.Intervention, CommercialQueueCodeDescriptionPairList.Codes.Completed);
			UPECusHAWB hAWB7 = CreateHAWBRelatedToThisDeclaration("107", CargoReportQueueCodeDescriptionPairList.Codes.Unknown, CommercialQueueCodeDescriptionPairList.Codes.Rebill);
			UPECusHAWB hAWB8 = CreateHAWBRelatedToThisDeclaration("108", CargoReportQueueCodeDescriptionPairList.Codes.EIR, CommercialQueueCodeDescriptionPairList.Codes.Chase);
			UPECusHAWB hAWB9 = CreateHAWBRelatedToThisDeclaration("109", CargoReportQueueCodeDescriptionPairList.Codes.Completed, CommercialQueueCodeDescriptionPairList.Codes.Hold);
			UPECusHAWB[] finalisedHAWBs = Queue.GetFinalisedCusHAWBs();
			AssertEquals(4, finalisedHAWBs.Length);
			SortHAWBs(finalisedHAWBs);
			AssertEquals("101", finalisedHAWBs[0].CS_HAWB);
			AssertEquals("102", finalisedHAWBs[1].CS_HAWB);
			AssertEquals("103", finalisedHAWBs[2].CS_HAWB);
			AssertEquals("104", finalisedHAWBs[3].CS_HAWB);
		}

		public void TestADPScoring()
		{
			Queue.P4_ParentID = Declaration.PK;
			Queue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.Classification;
			Factory.Save();
			AssertEquals("SubmissionCount zero initially for the test", 0, ADPScoring.T4_SubmissionCount);
			AssertEquals("SubmissionErrorCount zero initially for the test", 0, ADPScoring.T4_SubmissionErrorCount);
			Queue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.Lodgement;
			Factory.Save();
			AssertEquals("SubmissionCount should increment when queue moves Classification->Lodgement", 1, ADPScoring.T4_SubmissionCount);
			Queue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.Classification;
			Factory.Save();
			AssertEquals("SubmissionErrorCount should increment when queue moves Lodgement->Classification", 1, ADPScoring.T4_SubmissionErrorCount);
		}

		public void TestNewMAWBAlert()
		{
			GlbGroup group = Factory.New<GlbGroup>();
			group.GG_Code = "MAWBCLS";
			group.GG_Desc = "MAWB Classification";
			GlbStaff staff = group.Staff.AddNew();
			staff.GS_Code = "666";
			staff.GS_FullName = "Lucifer";
			staff.GS_LoginName = "Devil";
			staff.GS_EmailAddress = "test@edi.com.au";
			UPEDataRegistry.Instance.MAWBFirstMovedToClassificationNotificationGroup = group.PK.ToGuid();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			Factory.Save();
			UPECusMAWB mAWB = Factory.NewWithValidTestData<UPECusMAWB>(TestBusinessObjectKind.MinimumRequiredToSave);
			mAWB.CM_MAWB = "08165746845";
			UPEJobDeclaration jobDec = Factory.NewWithValidTestData<UPEJobDeclaration>(TestBusinessObjectKind.MinimumRequiredToSave);
			jobDec.JE_MasterBill = "08165746845";
			Factory.Save();
			AssertEquals("Has not moved to CLS queue. should be no email", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			jobDec.MoveToQueue(AutoDeclarationQueueCodeDescriptionPairList.Codes.Classification, ZString.Empty, ZString.Empty, ZString.Empty);
			Factory.Save();
			AssertEquals("email not sent when Masterbill moved to CLS Queue for the fist time", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertCollectionContains("Recipient should be test@edi.com.au", "test@edi.com.au", email.Recipients.ToStringCollection());
			AssertEquals("Subject", "MAWB: '08165746845' has been moved to the CLS queue for the first time.", email.Subject);
			AssertEquals("Body", "MAWB: '08165746845' has been moved to the CLS queue for the first time.", email.Body);
			ZQuery emailFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EmailSent.Code);
			emailFilter.AddToFilter(StmALogSchema.SL_Reference, "MAWB moved to Classification Queue");
			BusinessObject[] bizObjs = mAWB.Logs.GetAllLogs().Find(emailFilter);
			AssertEquals(1, bizObjs.Length);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			UPEJobDeclaration anotherJobDec = Factory.NewWithValidTestData<UPEJobDeclaration>(TestBusinessObjectKind.MinimumRequiredToSave);
			anotherJobDec.JE_MasterBill = "08165746845";
			anotherJobDec.MoveToQueue(AutoDeclarationQueueCodeDescriptionPairList.Codes.Classification, ZString.Empty, ZString.Empty, ZString.Empty);
			Factory.Save();
			AssertEquals("Masterbill has been moved to CLS Queue before. email should not be sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		#region AutoDeliverAlternateBrokerDocumentPack
		const bool WithAlternateBroker = true;
		const bool WithITFChargable = true;
		const bool WithFreightCharges = true;
		const bool ExpectDocumentDelivery = true;

		public void TestAutoDeliverAlternateBrokerDocumentPackIfRequired()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			Declaration.Delete();
			TestAutoDeliverAlternateBrokerDocumentPackIfRequired(WithAlternateBroker, WithITFChargable, !WithFreightCharges, !ExpectDocumentDelivery);
			TestAutoDeliverAlternateBrokerDocumentPackIfRequired(WithAlternateBroker, !WithITFChargable, WithFreightCharges, !ExpectDocumentDelivery);
			TestAutoDeliverAlternateBrokerDocumentPackIfRequired(WithAlternateBroker, WithITFChargable, WithFreightCharges, !ExpectDocumentDelivery);
			TestAutoDeliverAlternateBrokerDocumentPackIfRequired(WithAlternateBroker, !WithITFChargable, !WithFreightCharges, ExpectDocumentDelivery);
			TestAutoDeliverAlternateBrokerDocumentPackIfRequired(!WithAlternateBroker, !WithITFChargable, !WithFreightCharges, !ExpectDocumentDelivery);
		}

		void TestAutoDeliverAlternateBrokerDocumentPackIfRequired(bool withAlternateBroker, bool withITFChargable, bool withFreightCharges, bool expectDocumentDelivery)
		{
			Callout callout = Factory.NewWithValidTestData<Callout>();
			callout.CS_JE_CustomsFormalEntry = Factory.NewWithValidTestData<BaseJobDeclaration>().PK;
			callout.Declaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertAlternateBrokerDocumentsDelivered("Documents not delivered until declaration completed", callout, false);
			if (withAlternateBroker)
			{
				callout.Declaration.Importer.SetRelatedParty(NewAlternateBrokerWithEmailDocumentDeliveryMode(), RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Air, ZString.Empty);
			}

			Factory.Save();
			AssertAlternateBrokerDocumentsDelivered("Documents not delivered until declaration completed", callout, false);
			callout.Declaration.Importer.IsITFChargableForThisImporter = withITFChargable;
			if (!expectDocumentDelivery)
			{
				callout.CurrentQueue.P4_QueueName = ZString.Empty;
			}

			Factory.Save();
			AssertAlternateBrokerDocumentsDelivered("Documents not delivered until declaration completed", callout, false);
			if (withFreightCharges)
			{
				SetupFreightCharge(callout);
			}

			Factory.Save();
			AssertAlternateBrokerDocumentsDelivered("Documents not delivered until declaration completed", callout, false);
			callout.Declaration.CurrentQueue.P4_CustomsQueue = CustomsQueueCodeDescriptionPairList.Codes.Completed;
			Factory.Save();
			if (expectDocumentDelivery)
			{
				AssertAlternateBrokerDocumentsDelivered("Documents delivered when declaration completed and there are no ITF/freight charges", callout, true);
			}
			else
			{
				AssertAlternateBrokerDocumentsDelivered("Documents not delivered when declaration completed if there are ITF/freight charges", callout, false);
			}

			DeletePrintJobs();
		}

		void DeletePrintJobs()
		{
			StmPrintJob[] printJobs = (StmPrintJob[])Factory.Load(typeof(StmPrintJob), new ZQuery());
			foreach (StmPrintJob printJob in printJobs)
			{
				printJob.Delete();
			}

			Factory.Save();
		}

		OrgHeader NewAlternateBrokerWithEmailDocumentDeliveryMode()
		{
			OrgHeader result = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = result.Contacts.AddNew();
			contact.OC_ContactName = "Test Contact";
			contact.OC_Email = "clinton@edi.com.au";
			contact.OC_NotifyMode = Core.Constants.ContactNotifyModes.Email;
			OrgDocument document = contact.Documents.AddNew();
			document.OD_SU_MenuItem = new UPEDocumentMenuItemLoader(Factory).LoadAlternateBrokerDocumentPack().PK;
			document.OD_DeliverBy = Core.Constants.ContactNotifyModes.Email;
			return result;
		}

		void SetupFreightCharge(Callout callout)
		{
			callout.EnsureJobHeaderExists();
			CalloutCharge freightCharge = callout.JobHeader.Charges.AddNew();
			freightCharge.JR_Desc = ShipmentChargeDescription.Freight;
			freightCharge.TaxableAmount = 9m;
		}

		void AssertAlternateBrokerDocumentsDelivered(ZString message, Callout callout, bool expectDelivered)
		{
			StmPrintJob[] printJobs = (StmPrintJob[])Factory.Load(typeof(StmPrintJob), new ZQuery());
			// must only have been auto-delivered once, if delivered
			AssertEquals(message, expectDelivered ? 1 : 0, printJobs.Length);
		}

		#endregion
		#region Implementation
		protected override Type ExpectedParentBusinessObjectType
		{
			get
			{
				return typeof(UPEJobDeclaration);
			}
		}

		protected override Type ExpectedLookupsType
		{
			get
			{
				return typeof(UPEDeclarationQueueLookups);
			}
		}

		protected override Type ExpectedValidationType
		{
			get
			{
				return typeof(UPEDeclarationQueueValidation);
			}
		}

		new UPEDeclarationQueue Queue
		{
			get
			{
				return (UPEDeclarationQueue)base.Queue;
			}
		}

		void SortHAWBs(UPECusHAWB[] hAWBs)
		{
			Array.Sort(hAWBs, delegate(UPECusHAWB x, UPECusHAWB y)
			{
				return x.CS_HAWB.CompareTo(y.CS_HAWB);
			});
		}

		UPECusHAWB CreateHAWBRelatedToThisDeclaration(ZString hAWB, ZString customsQueue, ZString commercialQueue)
		{
			UPECusHAWB result = Factory.New<UPECusHAWB>();
			result.CS_HAWB = hAWB;
			result.CS_JE_CustomsFormalEntry = Declaration.PK;
			result.CurrentQueue.P4_CustomsQueue = customsQueue;
			result.CurrentQueue.P4_QueueName = commercialQueue;
			return result;
		}

		UPEJobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<UPEJobDeclaration>();
				}

				return fDeclaration;
			}
		}

		UPEADPScoring ADPScoring
		{
			get
			{
				return new UPEADPScoring.Loader(Factory).LoadOrCreate();
			}
		}

		UPEJobDeclaration fDeclaration;
		#endregion
	}
}
