using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.AU;
using static Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class QuarantineColsHeader : AutoQuarantineColsHeader,
		IDocAddresses,
		IQuarantineColsHeader,
		IClusterKeyWorker,
		ICusStorageDocPivotParent
	{
		public QuarantineColsHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[List(nameof(Lookups) + "." + nameof(QuarantineColsHeaderLookups.LateLodgementReasonsList))]
		public override ZString QCH_LateLodgementReason
		{
			get => base.QCH_LateLodgementReason;
			set => base.QCH_LateLodgementReason = value;
		}

		[RelatedBusinessObject(nameof(QuarantineColsHeader.CusEntryHeader))]
		public override ZGuid QCH_CH_CusEntryHeader
		{
			get { return base.QCH_CH_CusEntryHeader; }
			set { base.QCH_CH_CusEntryHeader = value; }
		}

		public CusEntryHeader CusEntryHeader => Factory.Load<CusEntryHeader>(QCH_CH_CusEntryHeader);

		public JobDeclaration JobDeclaration => Factory.GetValue(ref jobDeclarationCached, () => CusEntryHeader != null ? Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.PK, CusEntryHeader.CH_JE)) : null);
		CachedProperty<JobDeclaration> jobDeclarationCached;

		public bool IsAwaitingResponse => QCH_MessageStatus.StartsWith("A");

		public ZString HeaderStatus => Lookups.COLSHeaderStatusList.GetDescriptionFromCode(QCH_MessageStatus);

		public ZString LodgementStatus => Lookups.COLSLodgementStatusList.GetDescriptionFromCode(QCH_LodgementStatus);

		public ZString LodgementResultMessage => Factory.GetValue(ref lodgementResultMessage, delegate
		{
			var status = ZString.Empty;
			var lodgementStatusMessages = Messages.Cast<EDIMessage>().Where(m => m.EM_MessageType == AUCOLSMessageTypeList.Codes.LodgementStatus && m.EM_ReceiveTransmit == EDIMessage.Direction.Receive && m.EM_Status == EDIMessage.Status.Received)
				.OrderByDescending(m => m.EM_SystemCreateTimeUtc);
			foreach (var message in lodgementStatusMessages)
			{
				var responseObject = JsonSerializer.Deserialize<LodgementStatusResponse>(message.EM_MessageText);
				if (responseObject.result == COLSMessageProcessor.ResponseSuccess)
				{
					status = responseObject.resultMessage;
					break;
				}
			}

			return status;
		});
		CachedProperty<ZString> lodgementResultMessage;

		public ZString IMPNumber => Factory.GetValue(ref impNumberCached, delegate
		{
			return CusEntryHeader?.EntryNumber ?? ZString.Empty;
		});
		CachedProperty<ZString> impNumberCached;

		public ZString LRN => Factory.GetValue(ref lRNCached, delegate
		{
			var entryNumber = LRNCusEntryNumber;
			return entryNumber?.CE_EntryNum ?? ZString.Empty;
		});
		CachedProperty<ZString> lRNCached;

		public ZString LRNStatus => Factory.GetValue(ref lRNStatusCached, delegate
		{
			var entryNumber = LRNCusEntryNumber;
			return entryNumber?.CE_EntryStatus ?? ZString.Empty;
		});
		CachedProperty<ZString> lRNStatusCached;

		public AUCusEntryNumber LRNCusEntryNumber
		{
			get
			{
				var cusEntryNumbers = GetAllLRNNumbers();
				return cusEntryNumbers?.OrderByDescending(x => x.CE_SystemCreateTimeUtc).FirstOrDefault();  // We should only have one LRN number. This just double check that we get the latest one. 
			}
		}

		AUCusEntryNumber[] GetAllLRNNumbers()
		{
			var zQuery = new ZQuery();
			zQuery.AddToFilter(CusEntryNumSchema.CE_ParentID, this.PK);
			zQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.LocalReferenceNumber);
			zQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Australia);
			zQuery.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
			return Factory.Load<AUCusEntryNumber>(zQuery);
		}

		public void AddANewLRN(ZString newLRN, ZString entryStatus)
		{
			if (!(newLRN == LRN && LRNStatus == COLSEntryStatusList.Codes.LrnActive))
			{
				var cusEntryNumbers = GetAllLRNNumbers();
				foreach (var enctryNumber in cusEntryNumbers)
				{
					enctryNumber.CE_EntryType = CusEntryNumberTypes.Australia.InactiveLodgmentReferenceNumber;
				}

				var newLRNNumber = CusEntryNumber.New<CusEntryNumber>(this, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Australia);
				newLRNNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
				newLRNNumber.CE_EntryStatus = entryStatus;
				newLRNNumber.CE_EntryNum = newLRN;

				Logs.CreateOrRecreateEventLog(new EventValue(AutoEvents.StatusChange, reference: "|MST=COLS|NEW=LRNRCV", eventTime: ZDateTimeOffset.Now, isEstimate: false));
			}
		}

		[ChildEditable(true)]
		public QuarantineColsDirectionCollection Directions
		{
			get
			{
				if (fDirections == null)
				{
					fDirections = new QuarantineColsDirectionCollection(this);
					fDirections.Load();
					RegisterEditableChildObject(fDirections);
				}
				return fDirections;
			}
		}
		QuarantineColsDirectionCollection fDirections;

		public JobDocAddress ResponsibleParty
		{
			get
			{
				if (fResponsibleParty == null || fResponsibleParty.IsDeleted)
				{
					fResponsibleParty = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.COLSResponsibleParty);
					var branchOrgProxy = JobDeclaration?.Branch.OrgProxy;
					if (!fResponsibleParty.IsInDatabase && branchOrgProxy != null)
					{
						using (fResponsibleParty.SuspendSettingHasChanges())
						{
							fResponsibleParty.OrganisationPK = branchOrgProxy.PK;
							var brokerName = JobDeclaration.BrokerName;
							if (!brokerName.IsEmpty)
							{
								var contact = branchOrgProxy.Contacts?.Cast<OrgContact>()?.SingleOrDefault(x => x.OC_ContactName.EqualsIgnoringCase(brokerName));
								if (contact != null)
								{
									fResponsibleParty.ContactPK = contact.PK;
								}
							}
						}
					}
				}
				return fResponsibleParty;
			}
		}
		JobDocAddress fResponsibleParty;

		public JobDocAddress DeliveryOrUnpack
		{
			get
			{
				if (fDeliveryOrUnpack == null || fDeliveryOrUnpack.IsDeleted)
				{
					if (fDeliveryOrUnpack != null)
					{
						fDeliveryOrUnpack.E2_OA_AddressInfo.ValueChanged -= DeliveryOrUnpackE2_OA_AddressInfo_ValueChanged;
						fDeliveryOrUnpack.E2_PostcodeInfo.ValueChanged -= DeliveryOrUnpackE2_PostcodeInfo_ValueChanged;
					}
					fDeliveryOrUnpack = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.COLSDeliveryOrUnpack);
					fDeliveryOrUnpack.E2_OA_AddressInfo.ValueChanged += DeliveryOrUnpackE2_OA_AddressInfo_ValueChanged;
					fDeliveryOrUnpack.E2_PostcodeInfo.ValueChanged += DeliveryOrUnpackE2_PostcodeInfo_ValueChanged;
				}
				return fDeliveryOrUnpack;
			}
		}
		JobDocAddress fDeliveryOrUnpack;

		void DeliveryOrUnpackE2_OA_AddressInfo_ValueChanged(object sender, EventArgs e)
		{
			QCH_ApprovedArrangementRefNum = DeliveryOrUnpack.Address?.CustomsCodes?.GetCustomsRegNo(AustraliaCodeTypes.ApprovedArrangementNumber, Core.Constants.CountryCodes.Australia).Left(Schema.QCH_ApprovedArrangementRefNumMaxLength) ?? ZString.Empty;

			UpdateDeliveryClassificationFromDeliveryOrUnpackAddressPostcode();
		}

		void DeliveryOrUnpackE2_PostcodeInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateDeliveryClassificationFromDeliveryOrUnpackAddressPostcode();
		}

		internal ZString DeliveryOrUnpackAddressCompanyName => DeliveryOrUnpack.E2_AddressOverride ? DeliveryOrUnpack.E2_CompanyName : (DeliveryOrUnpack.Address?.CompanyName ?? ZString.Empty);

		string DeliveryOrUnpackAddressPostcode => DeliveryOrUnpack.E2_AddressOverride ? DeliveryOrUnpack.E2_Postcode : DeliveryOrUnpack.Address?.Postcode;

		internal string DeliveryOrUnpackAddressPostcodeClassification => UniversalReferenceHelper.GetPostcodeDeliveryClassificationAttribute(Factory, DeliveryOrUnpackAddressPostcode);

		void UpdateDeliveryClassificationFromDeliveryOrUnpackAddressPostcode()
		{
			var classification = DeliveryOrUnpackAddressPostcodeClassification;
			QCH_DeliveryClassification = Lookups.DeliveryClassification.ContainsCode(classification) ? classification : string.Empty;
		}

		[List(nameof(Lookups) + "." + nameof(QuarantineColsHeaderLookups.DeliveryClassification))]
		public override ZString QCH_DeliveryClassification
		{
			get { return base.QCH_DeliveryClassification; }
			set { base.QCH_DeliveryClassification = value; }
		}

		public static bool IsCOLSFunctionEnabled => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Customs.Universal.Constants.FunctionalityTypes.COLS, Core.Constants.CountryCodes.Australia, ZDateTime.Today, priorityToPilotFunctionality: false)
			|| ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Customs.Universal.Constants.FunctionalityTypes.PCOLS, Core.Constants.CountryCodes.Australia, ZDateTime.Today, priorityToPilotFunctionality: true);

		#region EDIMessageCollection

		[ChildEditable(true)]
		public QuarantineColsHeaderMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = new QuarantineColsHeaderMessageCollection(this);
					RegisterEditableChildObject(fMessages);
					fMessages.Load();
					AddAttachmentMessages(fMessages);
					fMessages.IsManagedForDataRefresh = true;
				}
				return fMessages;
			}
		}
		QuarantineColsHeaderMessageCollection fMessages;

		[ChildEditable(true)]
		public QuarantineColsHeaderMessageCollection NonDiscardedMessages
		{
			get
			{
				if (fNonDiscardedMessages == null)
				{
					fNonDiscardedMessages = new QuarantineColsHeaderMessageCollection(this, EDIMessageStatusList.Codes.Discarded, false);
					RegisterEditableChildObject(fNonDiscardedMessages);
					fNonDiscardedMessages.Load();
					AddAttachmentNonDiscardedMessages(fNonDiscardedMessages);
					fNonDiscardedMessages.IsManagedForDataRefresh = true;
				}
				return fNonDiscardedMessages;
			}
		}
		QuarantineColsHeaderMessageCollection fNonDiscardedMessages;

		[ChildEditable(true)]
		public QuarantineColsHeaderMessageCollection DiscardedMessages
		{
			get
			{
				if (fDiscardedMessages == null)
				{
					fDiscardedMessages = new QuarantineColsHeaderMessageCollection(this, EDIMessageStatusList.Codes.Discarded, true);
					RegisterEditableChildObject(fDiscardedMessages);
					fDiscardedMessages.Load();
					AddAttachmentDiscardedMessages(fDiscardedMessages);
					fDiscardedMessages.IsManagedForDataRefresh = true;
				}
				return fDiscardedMessages;
			}
		}
		QuarantineColsHeaderMessageCollection fDiscardedMessages;

		internal EDIMessage[] PendingAddAttachmentMessages
		{
			get
			{
				return Messages.Where(x =>
					x.EM_ReceiveTransmit == EDIMessage.Direction.Transmit &&
					x.EM_MessageType == AUCOLSMessageTypeList.Codes.AddAttachment &&
					x.EM_Status == EDIMessage.Status.Pending).ToArray();
			}
		}

		public void ReloadMessages()
		{
			if (fMessages != null)
			{
				fMessages.Reload(false);
				AddAttachmentMessages(fMessages);
			}
		}

		void AddAttachmentMessages(QuarantineColsHeaderMessageCollection messages)
		{
			if (EDocPivotCollection.Count > 0)
			{
				var messageQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.COLS);
				messageQuery.AddToFilter(EDIMessageSchema.EM_LinkTable, CusStorageDocPivotSchema.Constants.TableName);
				messageQuery.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, EDocPivotCollection.Select(x => x.PK));

				var attachmentMessages = Factory.Load<EDIMessage>(messageQuery);
				messages.AddRange(attachmentMessages);
			}
		}

		void AddAttachmentNonDiscardedMessages(QuarantineColsHeaderMessageCollection messages)
		{
			if (EDocPivotCollection.Count > 0)
			{
				var messageQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.COLS);
				messageQuery.AddToFilter(EDIMessageSchema.EM_Status, SQLComparisonOperator.NotEqual, EDIMessageStatusList.Codes.Discarded);
				messageQuery.AddToFilter(EDIMessageSchema.EM_LinkTable, CusStorageDocPivotSchema.Constants.TableName);
				messageQuery.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, EDocPivotCollection.Select(x => x.PK));

				var attachmentMessages = Factory.Load<EDIMessage>(messageQuery);
				messages.AddRange(attachmentMessages);
			}
		}

		void AddAttachmentDiscardedMessages(QuarantineColsHeaderMessageCollection messages)
		{
			if (EDocPivotCollection.Count > 0)
			{
				var messageQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.COLS);
				messageQuery.AddToFilter(EDIMessageSchema.EM_Status, EDIMessageStatusList.Codes.Discarded);
				messageQuery.AddToFilter(EDIMessageSchema.EM_LinkTable, CusStorageDocPivotSchema.Constants.TableName);
				messageQuery.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, EDocPivotCollection.Select(x => x.PK));

				var attachmentMessages = Factory.Load<EDIMessage>(messageQuery);
				messages.AddRange(attachmentMessages);
			}
		}
		#endregion

		#region ICusStorageDocPivotParent
		public void ReloadCollection()
		{
			if (IsEDocPivotCollectionLoaded)
			{
				EDocPivotCollection.Reload(true);
			}
		}

		bool IsEDocPivotCollectionLoaded => eDocPivotCollection?.IsLoaded ?? false;

		[ChildEditable(true)]
		public CusStorageDocPivotCollection EDocPivotCollection
		{
			get
			{
				if (eDocPivotCollection == null)
				{
					eDocPivotCollection = new CusStorageDocPivotCollection(this);
					eDocPivotCollection.Load();
					RegisterEditableChildObject(eDocPivotCollection);
				}

				return eDocPivotCollection;
			}
		}
		CusStorageDocPivotCollection eDocPivotCollection;

		public Type CusStorageDocPivotType => typeof(CusStorageDocPivot);

		public IEnumerable<IStorageDocsBaseCollection> EDocCollections => Customs.Business.EDocsHelper.GetEDocCollections(JobDeclaration, JobDeclaration?.Shipment);

		#endregion

		#region IDocAddresses Members

		[ChildEditable(true)]
		public JobDocAddressDependentCollection DocAddresses => fDocAddresses ?? (fDocAddresses = GetDocAddressesCore());
		JobDocAddressDependentCollection fDocAddresses;

		protected JobDocAddressDependentCollection GetDocAddressesCore()
		{
			var docAddresses = new AUJobDocAddressDependentCollection(this);
			docAddresses.Load();
			RegisterEditableChildObject(docAddresses);
			return docAddresses;
		}

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return null;
		}

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return GetCanOverrideCheckpointCore(docAddress);
		}

		protected SecurityCheckpoint GetCanOverrideCheckpointCore(JobDocAddress docAddress)
		{
			return Env.Security.None;
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			var lrnIsEmpty = LRN.IsEmpty;
			return Factory.GetCachedValue($"QuarantineColsHeader.GetDocAddressRequirement_{addressType}_{lrnIsEmpty}", () =>
			{
				return addressType == DocAddressType.COLSResponsibleParty && lrnIsEmpty
					? new COLSResponsiblePartyAddressRequirement(this)
					: null;
			});
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
			if (docAddress.DocAddressType == DocAddressType.COLSDeliveryOrUnpack)
			{
				var addresses = docAddress.Organisation?.Addresses;
				if (addresses != null)
				{
					var matchedAddress = GetBestDeliveryOrUnpackAddress(addresses.Cast<OrgAddress>().ToArray(), true);

					if (matchedAddress != null)
					{
						docAddress.E2_OA_Address = matchedAddress.PK;
					}
				}
			}
		}

		public OrgAddress GetBestDeliveryOrUnpackAddress(OrgAddress[] addresses, bool hasAddressWithoutAANCode)
		{
			OrgAddress matchedAddressWithAANCode = null;
			OrgAddress mainDeliveryAddressWithAANCode = null;
			OrgAddress normalPickupAndDeliveryAddressWithAANCode = null;
			OrgAddress matchedAddressWithoutAANCode = null;
			OrgAddress mainPickupAndDeliveryAddressWithoutAANCode = null;
			OrgAddress mainDeliveryAddressWithoutAANCode = null;
			OrgAddress normalPickupAndDeliveryAddressWithoutAANCode = null;
			foreach (var address in addresses)
			{
				if (hasAddressWithoutAANCode && address.CustomsCodes.GetCustomsRegNo(AustraliaCodeTypes.ApprovedArrangementNumber, Core.Constants.CountryCodes.Australia).IsEmpty)
				{
					if (address.IsMainAddressOfType(OrgAddressType.PickupAndDelivery))
					{
						matchedAddressWithoutAANCode = mainPickupAndDeliveryAddressWithoutAANCode = address;
					}
					else if (mainPickupAndDeliveryAddressWithoutAANCode == null && address.IsMainAddressOfType(OrgAddressType.Delivery))
					{
						matchedAddressWithoutAANCode = mainDeliveryAddressWithoutAANCode = address;
					}
					else if (mainPickupAndDeliveryAddressWithoutAANCode == null && mainDeliveryAddressWithoutAANCode == null && address.IsAddressOfType(OrgAddressType.PickupAndDelivery))
					{
						matchedAddressWithoutAANCode = normalPickupAndDeliveryAddressWithoutAANCode = address;
					}
					else if (mainPickupAndDeliveryAddressWithoutAANCode == null && mainDeliveryAddressWithoutAANCode == null && normalPickupAndDeliveryAddressWithoutAANCode == null && address.IsAddressOfType(OrgAddressType.Delivery))
					{
						matchedAddressWithoutAANCode = address;
					}
					else if (matchedAddressWithoutAANCode == null && address.CapabilitiesCollection.Any(x => x.PZ_IsMainAddress))
					{
						matchedAddressWithoutAANCode = address;
					}
				}
				else
				{
					if (address.IsMainAddressOfType(OrgAddressType.PickupAndDelivery))
					{
						matchedAddressWithAANCode = address;
						break;
					}
					else if (address.IsMainAddressOfType(OrgAddressType.Delivery))
					{
						matchedAddressWithAANCode = mainDeliveryAddressWithAANCode = address;
					}
					else if (mainDeliveryAddressWithAANCode == null && address.IsAddressOfType(OrgAddressType.PickupAndDelivery))
					{
						matchedAddressWithAANCode = normalPickupAndDeliveryAddressWithAANCode = address;
					}
					else if (mainDeliveryAddressWithAANCode == null && normalPickupAndDeliveryAddressWithAANCode == null && address.IsAddressOfType(OrgAddressType.Delivery))
					{
						matchedAddressWithAANCode = address;
					}
					else if (matchedAddressWithAANCode == null && address.CapabilitiesCollection.Any(x => x.PZ_IsMainAddress))
					{
						matchedAddressWithAANCode = address;
					}
				}
			}
			return matchedAddressWithAANCode ?? matchedAddressWithoutAANCode;
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return false;
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return null;
		}

		public IReadOnlyList<DocAddressType> SupportedAddressTypes
		{
			get
			{
				return new DocAddressType[] { DocAddressType.COLSDeliveryOrUnpack, DocAddressType.COLSResponsibleParty };
			}
		}

		#endregion

		#region IClusterKeyWorker

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)QCH_ClusterKeyInfo;
		Type IClusterKeyWorker.ParentBizObjType => typeof(CusEntryHeader);
		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)QCH_CH_CusEntryHeaderInfo;

		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList
		{
			get
			{
				yield return new ClusterKeyChildInfo(typeof(QuarantineColsDirection), QuarantineColsDirectionSchema.QCD_QCH_ColsHeader);
			}
		}

		#endregion

		public void DiscardPendingAttachmentMessages()
		{
			foreach (var message in PendingAddAttachmentMessages)
			{
				message.EM_Status = EDIMessage.Status.Discarded;
				if(message.EM_LinkedObject is CusStorageDocPivot pivot)
				{
					pivot.CSD_MessageStatus = COLSDocumentStatusList.Codes.Discarded;
				}
			}
		}
	}
}
