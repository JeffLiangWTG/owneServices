using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(ShipmentHeldLetterBusinessObject))]
	sealed class ShipmentHeldLetterBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestQueueForBatchPrintAndSave()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			BizObj.QueueForBatchPrintAndSave(ShipmentHeldLetterRecipient.Consignee);
			AssertEquals("Expect the factory to be saved now", false, CusHAWB.HasChanges);
			UPEPrintBatch printBatch = new UPEPrintBatch.Loader(Factory).CreateOrLoadLatestBatch(UPEPrintBatchTypes.Codes.ShipmentHeldLetter);
			AssertEquals("The document should be queued for batch print", 1, printBatch.PrintItems.Count);
			AssertEquals("The document should be queued for batch print", CusHAWB.PK, printBatch.PrintItems[0].Parent.PK);
		}

		public void TestCusHAWBQueueMovementRequiresAutoDelivery()
		{
			AssertQueueMovementRequiresAutoDelivery(false, false, ReasonCodeDescriptionPairList.Codes.AM_RefusedCancelledOrder);
			AssertQueueMovementRequiresAutoDelivery(false, false, ReasonCodeDescriptionPairList.Codes.RU_AlternateBroker);
			AssertQueueMovementRequiresAutoDelivery(false, false, ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease);
			AssertQueueMovementRequiresAutoDelivery(false, false, ReasonCodeDescriptionPairList.Codes.SN_PersonalEffectsHold);
			AssertQueueMovementRequiresAutoDelivery(false, true, ReasonCodeDescriptionPairList.Codes.BA_InadequateDescription);
			AssertQueueMovementRequiresAutoDelivery(false, true, ReasonCodeDescriptionPairList.Codes.BP_PhoneNumberMissing);
			AssertQueueMovementRequiresAutoDelivery(false, true, ReasonCodeDescriptionPairList.Codes.NY_MissingInvoice);
			AssertQueueMovementRequiresAutoDelivery(false, true, ReasonCodeDescriptionPairList.Codes.S1_ShipperConsigneeDetailsInsufficient);
			AssertQueueMovementRequiresAutoDelivery(false, true, ReasonCodeDescriptionPairList.Codes.XH_DocumentInsufficient);
			AssertQueueMovementRequiresAutoDelivery(false, true, ReasonCodeDescriptionPairList.Codes.XN_PhoneNumberInvalid);
		}

		public void TestDeclarationQueueMovementRequiresAutoDelivery()
		{
			AssertQueueMovementRequiresAutoDelivery(true, false, ReasonCodeDescriptionPairList.Codes.S1_ShipperConsigneeDetailsInsufficient);
			AssertQueueMovementRequiresAutoDelivery(true, false, ReasonCodeDescriptionPairList.Codes.SS_CustomsHold);
			AssertQueueMovementRequiresAutoDelivery(true, false, ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration);
			AssertQueueMovementRequiresAutoDelivery(true, false, ReasonCodeDescriptionPairList.Codes.B5_ClientRegistration);
			AssertQueueMovementRequiresAutoDelivery(true, true, ReasonCodeDescriptionPairList.Codes.BA_InadequateDescription);
			AssertQueueMovementRequiresAutoDelivery(true, true, ReasonCodeDescriptionPairList.Codes.BP_PhoneNumberMissing);
			AssertQueueMovementRequiresAutoDelivery(true, true, ReasonCodeDescriptionPairList.Codes.NY_MissingInvoice);
			AssertQueueMovementRequiresAutoDelivery(true, true, ReasonCodeDescriptionPairList.Codes.XH_DocumentInsufficient);
			AssertQueueMovementRequiresAutoDelivery(true, true, ReasonCodeDescriptionPairList.Codes.XN_PhoneNumberInvalid);
		}

		void AssertQueueMovementRequiresAutoDelivery(bool useDeclarationQueue, bool expectRequiresAutoDelivery, ZString reasonCode)
		{
			UPECusHAWB cusHAWB = Factory.NewWithValidTestData<UPECusHAWB>();
			cusHAWB.CS_JE_CustomsFormalEntry = Factory.NewWithValidTestData(typeof(BaseJobDeclaration)).PK;
			UPEProcessQueue queue = useDeclarationQueue ? cusHAWB.Declaration.CurrentQueue : cusHAWB.CurrentQueue;
			queue.P4_CustomsQueue = CustomsQueueCodeDescriptionPairList.Codes.Hold;
			queue.P4_CustomsStatus = reasonCode;
			AssertEquals("Queue movement to non-EIR queue doesn't require delivery", false, cusHAWB.ShipmentHeldLetterDetails.QueueMovementRequiresAutoDelivery());
			queue.P4_CustomsQueue = CustomsQueueCodeDescriptionPairList.Codes.EIR;
			queue.P4_CustomsStatus = reasonCode;
			AssertEquals("Queue movement to EIR/" + reasonCode + " with IsInDatabase=false", expectRequiresAutoDelivery, cusHAWB.ShipmentHeldLetterDetails.QueueMovementRequiresAutoDelivery());
			queue.P4_CustomsQueue = CustomsQueueCodeDescriptionPairList.Codes.Hold;
			queue.P4_CustomsStatus = reasonCode;
			Factory.Save();
			queue.P4_CustomsQueue = CustomsQueueCodeDescriptionPairList.Codes.EIR;
			queue.P4_CustomsStatus = reasonCode;
			AssertEquals(true, queue.P4_CustomsQueueInfo.HasChanges);
			AssertEquals(false, queue.P4_CustomsStatusInfo.HasChanges);
			AssertEquals("Queue movement to EIR/" + reasonCode + " with P4_CustomsQueueInfo.HasChanges=true", expectRequiresAutoDelivery, cusHAWB.ShipmentHeldLetterDetails.QueueMovementRequiresAutoDelivery());
			queue.P4_CustomsStatus = "";
			Factory.Save();
			queue.P4_CustomsStatus = reasonCode;
			AssertEquals(false, queue.P4_CustomsQueueInfo.HasChanges);
			AssertEquals(true, queue.P4_CustomsStatusInfo.HasChanges);
			AssertEquals("Queue movement to EIR/" + reasonCode + " with P4_CustomsStatusInfo.HasChanges=true", expectRequiresAutoDelivery, cusHAWB.ShipmentHeldLetterDetails.QueueMovementRequiresAutoDelivery());
			Factory.Save();
			queue.P4_CustomsQueue = "";
			queue.P4_CustomsStatus = "";
			queue.P4_CustomsQueue = CustomsQueueCodeDescriptionPairList.Codes.EIR;
			queue.P4_CustomsStatus = reasonCode;
			AssertEquals("Delivery is not required a second time when the queue/reason hasn't changed", false, cusHAWB.ShipmentHeldLetterDetails.QueueMovementRequiresAutoDelivery());
		}

		public void TestSetDefaultsForAutoDelivery()
		{
			CusHAWB.CurrentQueue.P4_CustomsQueue = CustomsQueueCodeDescriptionPairList.Codes.EIR;
			CusHAWB.CurrentQueue.P4_CustomsStatus = ReasonCodeDescriptionPairList.Codes.BP_PhoneNumberMissing;
			CusHAWB.ShipmentHeldLetterDetails.SetDefaultsForAutoDelivery();
			AssertEquals("Populate from the cargo report if there is no declaration", ReasonCodeDescriptionPairList.Codes.BP_PhoneNumberMissing, CusHAWB.ShipmentHeldLetterDetails.ReasonCode);
			CusHAWB.CS_JE_CustomsFormalEntry = Factory.NewWithValidTestData(typeof(BaseJobDeclaration)).PK;
			CusHAWB.Declaration.CurrentQueue.P4_CustomsQueue = CustomsQueueCodeDescriptionPairList.Codes.EIR;
			CusHAWB.Declaration.CurrentQueue.P4_CustomsStatus = ReasonCodeDescriptionPairList.Codes.BA_InadequateDescription;
			CusHAWB.ShipmentHeldLetterDetails.SetDefaultsForAutoDelivery();
			AssertEquals("Populate from the declaration if there is a hold reason on cargo report and declaration", ReasonCodeDescriptionPairList.Codes.BA_InadequateDescription, CusHAWB.ShipmentHeldLetterDetails.ReasonCode);
			CusHAWB.Declaration.CurrentQueue.P4_CustomsStatus = "";
			CusHAWB.ShipmentHeldLetterDetails.SetDefaultsForAutoDelivery();
			AssertEquals("Populate from the cargo report if there is no hold reason on the declaration", ReasonCodeDescriptionPairList.Codes.BP_PhoneNumberMissing, CusHAWB.ShipmentHeldLetterDetails.ReasonCode);
		}

		#region Load/Save from Note
		public void TestLoadFromNoteSaveToNote()
		{
			TestLoadFromNoteSaveToNote("OTH", "UserEditedReasonText");
			TestLoadFromNoteSaveToNote(ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment, ReasonCodeDescriptionPairList.Descriptions.OQ_HeldForPayment);
		}

		void TestLoadFromNoteSaveToNote(ZString reasonCode, ZString reasonText)
		{
			BizObj.UPSContactName = "ContactName";
			BizObj.UPSContactPhone = "ContactPhone";
			BizObj.ReasonCode = reasonCode;
			BizObj.ReasonText = reasonText;
			BizObj.QueueForBatchPrint = true;
			BizObj.DeliverByEmailFax = true;
			Factory.Save();
			BusinessObjectFactory separateFactory = new BusinessObjectFactory();
			UPECusHAWB loadedCusHAWB = separateFactory.Load<UPECusHAWB>(CusHAWB.PK);
			AssertEquals("ContactName", loadedCusHAWB.ShipmentHeldLetterDetails.UPSContactName);
			AssertEquals("ContactPhone", loadedCusHAWB.ShipmentHeldLetterDetails.UPSContactPhone);
			AssertEquals(reasonCode, loadedCusHAWB.ShipmentHeldLetterDetails.ReasonCode);
			AssertEquals(reasonText, loadedCusHAWB.ShipmentHeldLetterDetails.ReasonText);
			AssertEquals(true, loadedCusHAWB.ShipmentHeldLetterDetails.QueueForBatchPrint);
			AssertEquals(true, loadedCusHAWB.ShipmentHeldLetterDetails.DeliverByEmailFax);
		}

		public void TestNewPropertiesShouldBeSerialized()
		{
			int propertyCount = 0;
			foreach (PropertyInfo property in typeof(ShipmentHeldLetterBusinessObject).GetProperties())
			{
				if (property.DeclaringType.IsSubclassOf(typeof(NonPersistentBusinessObject)))
				{
					propertyCount++;
				}
			}

			AssertEquals("Any newly added properties must be serialized to the note", 13, propertyCount);
		}

		public void TestXmlNoteContentCompatibility()
		{
			var customsNote = Factory.New<HiddenStmNote>();
			CusHAWB.Notes.Add(customsNote);
			customsNote.ST_NoteText = CustomsSampleXmlNoteContent;
			customsNote.ST_Description = "Customs Shipment Held Letter";
			var financeNote = Factory.New<HiddenStmNote>();
			CusHAWB.Notes.Add(financeNote);
			financeNote.ST_NoteText = FinanceSampleXmlNoteContent;
			financeNote.ST_Description = "Finance Shipment Held Letter";
			const string CustomsMessage = "Existing Customs note description and text format must remain compatible, or a transformation must be written";
			AssertEquals(CustomsMessage, "CustomsContactName", CusHAWB.ShipmentHeldLetterDetails.UPSContactName);
			AssertEquals(CustomsMessage, "CustomsContactPhone", CusHAWB.ShipmentHeldLetterDetails.UPSContactPhone);
			AssertEquals(CustomsMessage, "OTH", CusHAWB.ShipmentHeldLetterDetails.ReasonCode);
			AssertEquals(CustomsMessage, "CustomsReasonText", CusHAWB.ShipmentHeldLetterDetails.ReasonText);
			AssertEquals(CustomsMessage, true, CusHAWB.ShipmentHeldLetterDetails.QueueForBatchPrint);
			AssertEquals(CustomsMessage, true, CusHAWB.ShipmentHeldLetterDetails.DeliverByEmailFax);
			Callout callout = Factory.Load<Callout>(CusHAWB.PK);
			const string FinanceMessage = "Existing Finance note description and text format must remain compatible, or a transformation must be written";
			AssertEquals(FinanceMessage, "FinanceContactName", callout.ShipmentHeldLetterDetails.UPSContactName);
			AssertEquals(FinanceMessage, "FinanceContactPhone", callout.ShipmentHeldLetterDetails.UPSContactPhone);
			AssertEquals(FinanceMessage, "OTH", callout.ShipmentHeldLetterDetails.ReasonCode);
			AssertEquals(FinanceMessage, "FinanceReasonText", callout.ShipmentHeldLetterDetails.ReasonText);
			AssertEquals(FinanceMessage, true, callout.ShipmentHeldLetterDetails.QueueForBatchPrint);
			AssertEquals(FinanceMessage, true, callout.ShipmentHeldLetterDetails.DeliverByEmailFax);
			ErrorReporter.Clear();
		}

		readonly string FinanceSampleXmlNoteContent = @"
<ShipmentHeldLetter>
  <UPSContactName>FinanceContactName</UPSContactName>
  <UPSContactPhone>FinanceContactPhone</UPSContactPhone>
  <ReasonCode>OTH</ReasonCode>
  <ReasonText>FinanceReasonText</ReasonText>
  <QueueForBatchPrint>true</QueueForBatchPrint>
  <DeliverByEmailFax>true</DeliverByEmailFax>
</ShipmentHeldLetter>
".Trim();

		readonly string CustomsSampleXmlNoteContent = @"
<ShipmentHeldLetter>
  <UPSContactName>CustomsContactName</UPSContactName>
  <UPSContactPhone>CustomsContactPhone</UPSContactPhone>
  <ReasonCode>OTH</ReasonCode>
  <ReasonText>CustomsReasonText</ReasonText>
  <QueueForBatchPrint>true</QueueForBatchPrint>
  <DeliverByEmailFax>true</DeliverByEmailFax>
</ShipmentHeldLetter>
".Trim();
		#endregion
		#region New Properties
		public void TestValidateUPSContactName()
		{
			BizObj.UPSContactName = "";
			AssertHasErrors("Mandatory validation", BizObj.UPSContactNameInfo);
			BizObj.UPSContactName = "Clinty";
			AssertNoErrors("Mandatory validation ok", BizObj.UPSContactNameInfo);
		}

		public void TestValidateUPSContactPhone()
		{
			BizObj.UPSContactPhone = "";
			AssertHasErrors("Mandatory validation", BizObj.UPSContactPhoneInfo);
			BizObj.UPSContactPhone = "90251173";
			AssertNoErrors("Mandatory validation ok", BizObj.UPSContactPhoneInfo);
			BizObj.UPSContactPhone = "ABCDEFG";
			AssertHasErrors("Invalid phone number", BizObj.UPSContactPhoneInfo);
		}

		public void TestValidateReasonCode()
		{
			BizObj.ReasonCode = "XXX";
			AssertHasErrors("Reason code invalid", BizObj.ReasonCodeInfo);
			BizObj.ReasonCode = ReasonCodeDescriptionPairList.Codes.XN_PhoneNumberInvalid;
			AssertNoErrors("Reason code now valid", BizObj.ReasonCodeInfo);
		}

		public void TestValidateReasonText()
		{
			BizObj.ReasonText = "";
			AssertHasErrors("Mandatory validation", BizObj.ReasonTextInfo);
			BizObj.ReasonText = "Goods exploded, please collect the debris";
			AssertNoErrors("Mandatory validation ok", BizObj.ReasonTextInfo);
		}

		public void TestReasonCodePopulatesReason()
		{
			BizObj.ReasonCode = ReasonCodeDescriptionPairList.Codes.BA_InadequateDescription;
			AssertEquals("Inadequate Goods Description", BizObj.ReasonText);
			BizObj.ReasonCode = ReasonCodeDescriptionPairList.Codes.FF_RTSAuthorisationRequired;
			AssertEquals("Return to Sender Supervisor Authorisation Required", BizObj.ReasonText);
			BizObj.ReasonCode = ReasonCodeDescriptionPairList.Codes.NY_MissingInvoice;
			AssertEquals("Invoice Required", BizObj.ReasonText);
			BizObj.ReasonCode = ReasonCodeDescriptionPairList.Codes.S1_ShipperConsigneeDetailsInsufficient;
			AssertEquals("Shipper / Consignee name and/or address details incomplete or insufficient", BizObj.ReasonText);
			BizObj.ReasonCode = ReasonCodeDescriptionPairList.Codes.XN_PhoneNumberInvalid;
			AssertEquals("Phone Number Invalid", BizObj.ReasonText);
		}

		public void TestQueueForBatchPrint_TrueByDefault()
		{
			AssertEquals("QueueForBatchPrint default value", true, BizObj.QueueForBatchPrint);
		}

		public void TestValidateQueueForBatchPrint_MandatoryForConsignee()
		{
			BizObj.RecipientForValidation = ShipmentHeldLetterRecipient.Consignee;
			BizObj.DeliverByEmailFax = true;
			BizObj.QueueForBatchPrint = false;
			AssertHasErrors("QueueForBatchPrint must be true for Consignee", BizObj.QueueForBatchPrintInfo);
			BizObj.RecipientForValidation = ShipmentHeldLetterRecipient.Consignor;
			BizObj.DeliverByEmailFax = true;
			BizObj.QueueForBatchPrint = false;
			AssertNoErrors("QueueForBatchPrint can be false for the shipper", BizObj.QueueForBatchPrintInfo);
		}

		public void TestValidateQueueForBatchPrint_MustChooseDeliveryMethod()
		{
			BizObj.RecipientForValidation = ShipmentHeldLetterRecipient.Consignor;
			BizObj.QueueForBatchPrint = false;
			BizObj.DeliverByEmailFax = false;
			AssertHasErrors("Neither selected", BizObj.QueueForBatchPrintInfo);
			BizObj.QueueForBatchPrint = true;
			BizObj.DeliverByEmailFax = true;
			AssertNoErrors("Both selected", BizObj.QueueForBatchPrintInfo);
			BizObj.QueueForBatchPrint = true;
			BizObj.DeliverByEmailFax = false;
			AssertNoErrors("QueueForBatchPrint selected", BizObj.QueueForBatchPrintInfo);
			BizObj.QueueForBatchPrint = false;
			BizObj.DeliverByEmailFax = true;
			AssertNoErrors("DeliverByEmailFax selected", BizObj.QueueForBatchPrintInfo);
			BizObj.QueueForBatchPrint = false;
			BizObj.DeliverByEmailFax = false;
			AssertHasErrors("Neither selected", BizObj.QueueForBatchPrintInfo);
		}

		#endregion
		#region Lookups
		public void TestReasonList()
		{
			foreach (CodeDescriptionPair pair in BizObj.ReasonList)
			{
				AssertEquals("Should not include non-uploaded reasons", false, pair.Code.StartsWith("_"));
			}

			AssertEquals("Should include 'other'", "Other", BizObj.ReasonList.GetDescriptionFromCode("OTH"));
			AssertEquals("Should not include SR", false, BizObj.ReasonList.ContainsCode(ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease));
			AssertEquals("Should not include X2", false, BizObj.ReasonList.ContainsCode(ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration));
			AssertEquals("Should not include UD", false, BizObj.ReasonList.ContainsCode(ReasonCodeDescriptionPairList.Codes.UD_AlternateDeliveryAddress));
			AssertEquals("Should not include DN", false, BizObj.ReasonList.ContainsCode(ReasonCodeDescriptionPairList.Codes.DN_SplitShipment));
			AssertEquals("Should not include _34", false, BizObj.ReasonList.ContainsCode(ReasonCodeDescriptionPairList.Codes._34_Missort));
			AssertEquals("Should not include _C1", false, BizObj.ReasonList.ContainsCode(ReasonCodeDescriptionPairList.Codes._C1_SubsequentSplitShipment));
			AssertEquals("Should not include _R0", false, BizObj.ReasonList.ContainsCode(ReasonCodeDescriptionPairList.Codes._R0_Rebill));
			AssertEquals("Should not include _R1", false, BizObj.ReasonList.ContainsCode(ReasonCodeDescriptionPairList.Codes._R1_Abandon));
			AssertEquals("Should not include _R2", false, BizObj.ReasonList.ContainsCode(ReasonCodeDescriptionPairList.Codes._R2_Transhipment));
			AssertEquals("Should not include _R3", false, BizObj.ReasonList.ContainsCode(ReasonCodeDescriptionPairList.Codes._R3_RTS));
		}

		#endregion
		#region Implementation
		UPECusHAWB CusHAWB
		{
			get
			{
				if (fCusHAWB == null)
				{
					fCusHAWB = Factory.NewWithValidTestData<UPECusHAWB>();
				}

				return fCusHAWB;
			}
		}

		UPECusHAWB fCusHAWB;
		ShipmentHeldLetterBusinessObject BizObj
		{
			get
			{
				return CusHAWB.ShipmentHeldLetterDetails;
			}
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ShipmentHeldLetterBusinessObject(CusHAWB);
		}
		#endregion
	}
}
