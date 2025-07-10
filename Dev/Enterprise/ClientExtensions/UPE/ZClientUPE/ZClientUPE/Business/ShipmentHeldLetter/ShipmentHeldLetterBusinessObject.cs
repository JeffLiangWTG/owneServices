using System.Collections;
using System.IO;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.UPE.Business
{
	public class ShipmentHeldLetterBusinessObject : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ShipmentHeldLetterBusinessObject(UPECusHAWB cusHAWB)
			: base(cusHAWB.Factory)
		{
			this.CusHAWB = cusHAWB;
		}

		public readonly UPECusHAWB CusHAWB;
		public ShipmentHeldLetterRecipient RecipientForValidation;

		public void QueueForBatchPrintAndSave(ShipmentHeldLetterRecipient recipient)
		{
			UPEPrintBatch currentPrintBatch = new UPEPrintBatch.Loader(Factory).CreateOrLoadLatestBatch(UPEPrintBatchTypes.Codes.ShipmentHeldLetter);
			currentPrintBatch.QueueForBatchPrintAndSave(CusHAWB, new UPEDocumentMenuItemLoader(Factory).LoadCusHAWBHeldLetter(recipient).PK);
		}

		#region QueueMovementRequiresAutoDelivery / SetDefaultsForAutoDelivery

		public bool QueueMovementRequiresAutoDelivery()
		{
			bool result = false;
			if (IsQueueInEIRAndHasChanges(CusHAWB.CurrentQueue))
			{
				result = CusHAWBQueueReasonCodeRequiringAutoDelivery.Contains(CusHAWB.CurrentQueue.P4_CustomsStatus);
			}
			if (!result && CusHAWB.Declaration != null && IsQueueInEIRAndHasChanges(CusHAWB.Declaration.CurrentQueue))
			{
				result = DeclarationQueueReasonCodeRequiringAutoDelivery.Contains(CusHAWB.Declaration.CurrentQueue.P4_CustomsStatus);
			}
			return result;
		}

		public void SetDefaultsForAutoDelivery()
		{
			if (CusHAWB.Declaration != null && DeclarationQueueReasonCodeRequiringAutoDelivery.Contains(CusHAWB.Declaration.CurrentQueue.P4_CustomsStatus))
			{
				ReasonCode = CusHAWB.Declaration.CurrentQueue.P4_CustomsStatus;
			}
			else
			{
				ReasonCode = CusHAWB.CurrentQueue.P4_CustomsStatus;
			}
		}

		bool IsQueueInEIRAndHasChanges(UPEProcessQueue queue)
		{
			bool queueHasChanges = false;
			queueHasChanges |= !queue.IsInDatabase;
			queueHasChanges |= queue.P4_CustomsStatusInfo.HasChanges;
			queueHasChanges |= queue.P4_CustomsQueueInfo.HasChanges;
			return queueHasChanges && (queue.P4_CustomsQueue == CustomsQueueCodeDescriptionPairList.Codes.EIR);
		}

		static IList CusHAWBQueueReasonCodeRequiringAutoDelivery
		{
			get
			{
				return new ZString[]
				{
					ReasonCodeDescriptionPairList.Codes.BA_InadequateDescription,
					ReasonCodeDescriptionPairList.Codes.BP_PhoneNumberMissing,
					ReasonCodeDescriptionPairList.Codes.NY_MissingInvoice,
					ReasonCodeDescriptionPairList.Codes.S1_ShipperConsigneeDetailsInsufficient,
					ReasonCodeDescriptionPairList.Codes.XH_DocumentInsufficient,
					ReasonCodeDescriptionPairList.Codes.XN_PhoneNumberInvalid,
				};
			}
		}

		static IList DeclarationQueueReasonCodeRequiringAutoDelivery
		{
			get
			{
				return new ZString[]
				{
					ReasonCodeDescriptionPairList.Codes.BA_InadequateDescription,
					ReasonCodeDescriptionPairList.Codes.BP_PhoneNumberMissing,
					ReasonCodeDescriptionPairList.Codes.NY_MissingInvoice,
					ReasonCodeDescriptionPairList.Codes.XH_DocumentInsufficient,
					ReasonCodeDescriptionPairList.Codes.XN_PhoneNumberInvalid,
				};
			}
		}

		#endregion

		#region Load/Save from Note

		public void LoadFromNote()
		{
			ZString noteText = Note.Text;
			if (!noteText.IsEmpty)
			{
				using (StringReader reader = new StringReader(noteText))
				{
					using (XmlTextReader xmlReader = new XmlTextReader(reader))
					{
						ReadXml(xmlReader);
					}
				}
				ClearAllNotifications();
			}
		}

		public void SaveToNote()
		{
			using (StringWriter writer = new StringWriter())
			{
				using (XmlTextWriter xmlWriter = new XmlTextWriter(writer))
				{
					xmlWriter.Formatting = Formatting.Indented;

					WriteXml(xmlWriter);
					Note.Text = writer.GetStringBuilder().ToString();
				}
			}
		}

		void ReadXml(XmlReader reader)
		{
			reader.ReadStartElement();
			UPSContactName = reader.ReadElementString("UPSContactName");
			UPSContactPhone = reader.ReadElementString("UPSContactPhone");
			ReasonCode = reader.ReadElementString("ReasonCode");
			ReasonText = reader.ReadElementString("ReasonText");
			QueueForBatchPrint = XmlConvert.ToBoolean(reader.ReadElementString("QueueForBatchPrint"));
			DeliverByEmailFax = XmlConvert.ToBoolean(reader.ReadElementString("DeliverByEmailFax"));
			reader.ReadEndElement();
		}

		void WriteXml(XmlWriter writer)
		{
			writer.WriteStartElement("ShipmentHeldLetter");
			writer.WriteElementString("UPSContactName", UPSContactName);
			writer.WriteElementString("UPSContactPhone", UPSContactPhone);
			writer.WriteElementString("ReasonCode", ReasonCode);
			writer.WriteElementString("ReasonText", ReasonText);
			writer.WriteElementString("QueueForBatchPrint", XmlConvert.ToString(QueueForBatchPrint));
			writer.WriteElementString("DeliverByEmailFax", XmlConvert.ToString(DeliverByEmailFax));
			writer.WriteEndElement();
		}

		protected override IBusinessObjectFetchStrategy GetFetchStrategy()
		{
			return new Strategy(this);
		}

		class Strategy : BusinessObjectFetchStrategy
		{
			public Strategy(ShipmentHeldLetterBusinessObject bizO)
				: base(bizO)
			{
			}

			protected override void FetchForFactorySaveCore()
			{
				base.FetchForFactorySaveCore();
				object dummyLoadForPerformance = ((ShipmentHeldLetterBusinessObject)BusinessObject).Note;
			}
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			SaveToNote();
		}

		SavedHiddenTextNote Note
		{
			get
			{
				if (fNote == null)
				{
					fNote = new SavedHiddenTextNote(CusHAWB);
				}
				return fNote;
			}
		}
		SavedHiddenTextNote fNote;

		class SavedHiddenTextNote : HiddenTextNote
		{
			public SavedHiddenTextNote(UPECusHAWB parent)
				: base(parent)
			{
			}

			protected override ZString Description
			{
				get { return (Parent is Callout ? "Finance" : "Customs") + " Shipment Held Letter"; }
			}
		}

		#endregion

		#region New Properties

		#region UPSContactName

		[CargoWise.ComponentModel.MaxLength(50)]
		public ZString UPSContactName
		{
			get { return fUPSContactName; }
			set
			{
				CheckMaximumLength(UPSContactNameInfo, value);
				fUPSContactName = value;
				if (!IsValidationSuspended)
				{
					ValidateUPSContactName();
				}
				UPSContactNameInfo.RefreshBinding();
			}
		}
		ZString fUPSContactName;

		public ZPropertyInfo UPSContactNameInfo
		{
			get { return GetZPropertyInfo(nameof(UPSContactName)); }
		}

		public void ValidateUPSContactName()
		{
			UPSContactNameInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(UPSContactNameInfo);
		}

		#endregion

		#region UPSContactPhone

		[CargoWise.ComponentModel.MaxLength(25)]
		public ZString UPSContactPhone
		{
			get { return fUPSContactPhone; }
			set
			{
				CheckMaximumLength(UPSContactPhoneInfo, value);
				fUPSContactPhone = value;
				if (!IsValidationSuspended)
				{
					ValidateUPSContactPhone();
				}
				UPSContactPhoneInfo.RefreshBinding();
			}
		}
		ZString fUPSContactPhone;

		public ZPropertyInfo UPSContactPhoneInfo
		{
			get { return GetZPropertyInfo(nameof(UPSContactPhone)); }
		}

		public void ValidateUPSContactPhone()
		{
			UPSContactPhoneInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(UPSContactPhoneInfo);
			new PhoneNumberFormatAndValidation().PerformNumberValidation(UPSContactPhoneInfo, null, false);
		}

		#endregion

		#region ReasonCode

		[CargoWise.ComponentModel.MaxLength(3)]
		public ZString ReasonCode
		{
			get { return fReasonCode; }
			set
			{
				CheckMaximumLength(ReasonCodeInfo, value);

				ZString descriptionToDefault = GetReasonDescriptionFromCode(value);
				if (value != OtherReasonCode && !descriptionToDefault.IsEmpty)
				{
					ReasonText = descriptionToDefault;
				}

				fReasonCode = value;
				if (!IsValidationSuspended)
				{
					ValidateReasonCode();
				}
				ReasonCodeInfo.RefreshBinding();
			}
		}
		ZString fReasonCode;

		ZString GetReasonDescriptionFromCode(ZString code)
		{
			ZString result = ReasonList.GetDescriptionFromCode(code);
			switch (code)
			{
				case ReasonCodeDescriptionPairList.Codes.BA_InadequateDescription:
					result = "Inadequate Goods Description";
					break;
				case ReasonCodeDescriptionPairList.Codes.FF_RTSAuthorisationRequired:
					result = "Return to Sender Supervisor Authorisation Required";
					break;
				case ReasonCodeDescriptionPairList.Codes.NY_MissingInvoice:
					result = "Invoice Required";
					break;
				case ReasonCodeDescriptionPairList.Codes.S1_ShipperConsigneeDetailsInsufficient:
					result = "Shipper / Consignee name and/or address details incomplete or insufficient";
					break;
			}
			return result;
		}

		public ZPropertyInfo ReasonCodeInfo
		{
			get { return GetZPropertyInfo(nameof(ReasonCode)); }
		}

		public void ValidateReasonCode()
		{
			ReasonCodeInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(ReasonCodeInfo, ReasonList);
		}

		#endregion

		#region ReasonText

		[CargoWise.ComponentModel.MaxLength(200)]
		public ZString ReasonText
		{
			get { return fReasonText; }
			set
			{
				if (ReasonText != value)
				{
					CheckMaximumLength(ReasonTextInfo, value);
					fReasonText = value;

					ReasonCode = OtherReasonCode;
				}
				if (!IsValidationSuspended)
				{
					ValidateReasonText();
				}
				ReasonTextInfo.RefreshBinding();
			}
		}
		ZString fReasonText;

		public ZPropertyInfo ReasonTextInfo
		{
			get { return GetZPropertyInfo(nameof(ReasonText)); }
		}

		public void ValidateReasonText()
		{
			ReasonTextInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ReasonTextInfo);
		}

		#endregion

		#region QueueForBatchPrint

		public ZBool QueueForBatchPrint
		{
			get { return fQueueForBatchPrint; }
			set
			{
				fQueueForBatchPrint = value;
				if (!IsValidationSuspended)
				{
					ValidateQueueForBatchPrint();
				}
				QueueForBatchPrintInfo.RefreshBinding();
			}
		}
		ZBool fQueueForBatchPrint = true;

		public ZPropertyInfo QueueForBatchPrintInfo
		{
			get { return GetZPropertyInfo(nameof(QueueForBatchPrint)); }
		}

		public void ValidateQueueForBatchPrint()
		{
			QueueForBatchPrintInfo.ClearAllNotifications();
			if (!QueueForBatchPrint && RecipientForValidation == ShipmentHeldLetterRecipient.Consignee)
			{
				QueueForBatchPrintInfo.AddError("The hold letter to the consignee must be queued for batch print.");
			}
			else if (!QueueForBatchPrint && !DeliverByEmailFax)
			{
				QueueForBatchPrintInfo.AddError("You must choose a delivery method");
			}
		}

		#endregion

		#region DeliverByEmailFax

		public ZBool DeliverByEmailFax
		{
			get { return fDeliverByEmailFax; }
			set
			{
				fDeliverByEmailFax = value;
				if (!IsValidationSuspended)
				{
					ValidateQueueForBatchPrint();
				}
				DeliverByEmailFaxInfo.RefreshBinding();
			}
		}
		ZBool fDeliverByEmailFax;

		public ZPropertyInfo DeliverByEmailFaxInfo
		{
			get { return GetZPropertyInfo(nameof(DeliverByEmailFax)); }
		}

		#endregion

		#endregion

		#region Lookups

		public CodeDescriptionPairList ReasonList
		{
			get
			{
				if (fReasonList == null)
				{
					fReasonList = new CodeDescriptionPairList();
					AddToCodeDescriptionPairList(fReasonList, new AutoReasonCodeDescriptionPairList.AHLD());
					AddToCodeDescriptionPairList(fReasonList, new AutoReasonCodeDescriptionPairList.AHLD());
					AddToCodeDescriptionPairList(fReasonList, new AutoReasonCodeDescriptionPairList.AEIR());
					AddToCodeDescriptionPairList(fReasonList, new AutoReasonCodeDescriptionPairList.CHLD());
					AddToCodeDescriptionPairList(fReasonList, new AutoReasonCodeDescriptionPairList.CEIR());
					AddToCodeDescriptionPairList(fReasonList, new AutoReasonCodeDescriptionPairList.DBCA());
					AddToCodeDescriptionPairList(fReasonList, new AutoReasonCodeDescriptionPairList.DBCO());
					AddToCodeDescriptionPairList(fReasonList, new AutoReasonCodeDescriptionPairList.DEIR());

					fReasonList.RemoveCode(ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease);
					fReasonList.RemoveCode(ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration);
					fReasonList.RemoveCode(ReasonCodeDescriptionPairList.Codes.DN_SplitShipment);
					fReasonList.RemoveCode(ReasonCodeDescriptionPairList.Codes._34_Missort);
					fReasonList.AddPair(OtherReasonCode, "Other");
				}
				return fReasonList;
			}
		}
		CodeDescriptionPairList fReasonList;

		const string OtherReasonCode = "OTH";

		void AddToCodeDescriptionPairList(CodeDescriptionPairList listToAddTo, CodeDescriptionPairList newCodePairs)
		{
			foreach (CodeDescriptionPair newCodePair in newCodePairs)
			{
				if (!listToAddTo.ContainsCode(newCodePair.Code))
				{
					listToAddTo.Add(newCodePair);
				}
			}
		}

		#endregion
	}
}
