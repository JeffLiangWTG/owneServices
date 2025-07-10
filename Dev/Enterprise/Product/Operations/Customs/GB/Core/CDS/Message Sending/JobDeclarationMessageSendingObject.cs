using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageSending;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Common;
using Enterprise.Customs.GB.Registry;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.GB.CDS.Constants;
using CusEntryHeader = Enterprise.Customs.GB.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.GB.CDS
{
	public class JobDeclarationMessageSendingObject : WCOJobDeclarationMessageSendingObject
	{
		public new CusEntryHeader Header => (CusEntryHeader)base.Header;

		public JobDeclarationMessageSendingObject(CusEntryHeader header)
			: base(header)
		{
		}

		public new sealed class Schema : Customs.Business.AutoJobDeclarationMessageSendingObject.Schema
		{
			Schema()
			{
			}
		}

		#region MessageType

		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.Customs.GB.Business.JobDeclarationMessageSendingObject|MessageType", ShortCaption = "Msg. Type", Caption = "Message Type")]
		[List(nameof(MessageTypesList))]
		public new ZString MessageType
		{
			get => base.MessageType;
			set
			{
				var oldValue = MessageType;
				base.MessageType = value;

				if (oldValue != MessageType)
				{
					ClearAmendmentReasonIfNeeded();
					SetDefaultAmendmentReasonCode();
				}
			}
		}

		void SetDefaultAmendmentReasonCode()
		{
			if (IsFECOrNilAmendment)
			{
				ChangeAcknowledgementIndicator = AmendmentCancellationReasonCode.Codes.A_Nil;
			}
			else if (MessageType == CDSEDIMessageTypeList.Codes.ArrivalNotification)
			{
				ChangeAcknowledgementIndicator = AmendmentCancellationReasonCode.Codes.C_GoodsPresentationNotice;
			}
		}

		void ClearAmendmentReasonIfNeeded()
		{
			if (!IsAmendOrDelete)
			{
				if (!VOCReason.IsEmpty)
				{
					VOCReason = ZString.Empty;
				}

				if (!ChangeAcknowledgementIndicator.IsEmpty)
				{
					ChangeAcknowledgementIndicator = ZString.Empty;
				}
			}
		}

		protected override bool MessageType_ReadOnly => false;

		public ZString MessageTypeDescription => MessageTypesList.GetDescriptionFromCode(MessageType);

		public bool CanParticipateEnhancedValidation
		{
			get
			{
				var result = true;
				switch (MessageType)
				{
					case CDSEDIMessageTypeList.Codes.CancelDeclaration:
					case CDSEDIMessageTypeList.Codes.FecChallenge:
					case CDSEDIMessageTypeList.Codes.NilAmendment:
					case CDSEDIMessageTypeList.Codes.ArrivalNotification:
					case CDSEDIMessageTypeList.Codes.InventoryLinkingQueryRequest:
					case CDSEDIMessageTypeList.Codes.MasterQueryDeclaration:
					case GbCusDecMessageFunctionsList.Codes.Associate:
					case GbCusDecMessageFunctionsList.Codes.Disassociate:
					case GbCusDecMessageFunctionsList.Codes.Close:
					case GbCusDecMessageFunctionsList.Codes.ArrivalAtLocation:
					case GbCusDecMessageFunctionsList.Codes.DepartureFromLocation:
					case GbCusDecMessageFunctionsList.Codes.AnticipatedArrivalAtLocation:
						result = false;
						break;
				}
				return result;
			}
		}

		#endregion

		#region MovementReferenceNumber

		[CargoWiseOne.ResourceStrings.ResourceStringData("Enterprise.Customs.GB.CDS.JobDeclarationMessageSendingObject|MovementReferenceNumber", ShortCaption = "MRN", Caption = "Movement Reference Number")]
		public override ZString MovementReferenceNumber
		{
			get => base.MovementReferenceNumber;
			set => base.MovementReferenceNumber = value;
		}

		#endregion

		#region VocReason

		[CargoWiseOne.ResourceStrings.ResourceStringData("Enterprise.Customs.GB.CDS.JobDeclarationMessageSendingObject|AmendmentReason", ShortCaption = "Reason", Caption = "Amendment Reason")]
		[MaxLength(nameof(AmendmentReasonMaxLength))]
		public override ZString VOCReason
		{
			get => base.VOCReason;
			set => base.VOCReason = value;
		}

		protected override bool VOCReason_ReadOnly => !IsAmendDeleteFecOrNil && !IsArrival;

		int AmendmentReasonMaxLength => Header.CH_CustomsMessageRemarks_MaxLength;

		#endregion

		#region ChangeAcknowledgementIndicator

		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.Customs.GB.CDS.JobDeclarationMessageSendingObject|ChangeAcknowledgementIndicator", ShortCaption = "Reason Code", Caption = "Amendment Reason Code")]
		[MaxLength(Schema.ChangeAcknowledgementIndicatorMaxLength)]
		[List(nameof(AmendmentReasonCodesList))]
		public override ZString ChangeAcknowledgementIndicator
		{
			get => base.ChangeAcknowledgementIndicator;
			set => base.ChangeAcknowledgementIndicator = value;
		}

		protected override bool ChangeAcknowledgementIndicator_ReadOnly => !IsAmendDeleteFecOrNil && !IsArrival;

		#endregion

		#region SetDefaultValuesFromEntry

		protected override void SetMessageSendingObjectDefaultValues()
		{
			base.SetMessageSendingObjectDefaultValues();
			ShouldSend = IsOneAndOnlyEntry
							|| MovementReferenceNumber.IsEmpty
							|| !IsPreLodgedWithCustoms
							|| (AreAllEntriesPreLodged && !IsCleared);

			if (ShouldSend)
			{
				ShouldSend = !Header.IsCancelledWithCustoms;
			}

			ChangeAcknowledgementIndicator = Header.ZG_AmendmentReasonCode;

			VOCReason = Header.CH_CustomsMessageRemarks;

			LocalReferenceNumber = Header.LRN;
		}

		protected override ZString GetDefaultMessageType()
		{
			string result;
			if (MovementReferenceNumber.IsEmpty)
			{
				result = CDSEDIMessageTypeList.Codes.NewDeclaration;
			}
			else if (!IsPreLodgedWithCustoms)
			{
				result = CDSEDIMessageTypeList.Codes.NewDeclaration;
			}
			else if (!IsCleared)
			{
				result = CDSEDIMessageTypeList.Codes.AmendDeclaration;
			}
			else
			{
				result = CDSEDIMessageTypeList.Codes.CancelDeclaration;
			}

			return result;
		}

		bool IsPreLodgedWithCustoms => Header.IsPreLodgedOrLodgedWithCustoms;

		bool IsOneAndOnlyEntry => Header.Declaration?.ActiveEntryHeaders.Count == 1;

		bool AreAllEntriesPreLodged => Header.Declaration?.ActiveEntryHeaders.Cast<CusEntryHeader>().All(x => x.IsPreLodgedOrLodgedWithCustoms) ?? false;

		bool IsCleared => EntryStatus == EntryStatusList.Codes.Clear || EntryStatus == ThreeCharFunctionCodes.DeclarationCleared;

		public bool IsAmendOrDelete => IsAmend || IsCancel;

		protected override bool IsCancelCore { get => MessageType == CDSEDIMessageTypeList.Codes.CancelDeclaration; }

		protected override bool IsAmendCore { get => MessageType == CDSEDIMessageTypeList.Codes.AmendDeclaration; }

		bool IsFEC => MessageType == CDSEDIMessageTypeList.Codes.FecChallenge;
		bool IsNil => MessageType == CDSEDIMessageTypeList.Codes.NilAmendment;
		bool IsArrival => MessageType == CDSEDIMessageTypeList.Codes.ArrivalNotification;
		bool IsFECOrNilAmendment => MessageType == CDSEDIMessageTypeList.Codes.NilAmendment || MessageType == CDSEDIMessageTypeList.Codes.FecChallenge;
		public bool IsAmendDeleteFecOrNil => IsAmendOrDelete || IsFEC || IsNil;

		#endregion

		#region Lookups

		public CodeDescriptionPairList MessageTypesList
		{
			get
			{
				var declaration = Header.Declaration;

				return ((declaration?.JE_MessageType ?? ZString.Empty) == EU.Business.MessageTypeList.Codes.Export) ?
					Factory.GetCachedValue("MessageTypesCodeList_GB_Export_" + declaration.JE_CustomsProfile, () =>
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(CDSEDIMessageTypeList.Codes.NewDeclaration, CDSEDIMessageTypeList.Descriptions.NewDeclaration);
						result.AddPair(CDSEDIMessageTypeList.Codes.AmendDeclaration, CDSEDIMessageTypeList.Descriptions.AmendDeclaration);
						result.AddPair(CDSEDIMessageTypeList.Codes.CancelDeclaration, CDSEDIMessageTypeList.Descriptions.CancelDeclaration);
						result.AddPair(CDSEDIMessageTypeList.Codes.FecChallenge, CDSEDIMessageTypeList.Descriptions.FecChallenge);
						result.AddPair(GbCusDecMessageFunctionsList.Codes.Associate, GbCusDecMessageFunctionsList.Descriptions.Associate);
						result.AddPair(GbCusDecMessageFunctionsList.Codes.Disassociate, GbCusDecMessageFunctionsList.Descriptions.Disassociate);
						result.AddPair(GbCusDecMessageFunctionsList.Codes.Close, GbCusDecMessageFunctionsList.Descriptions.Close);
						result.AddPair(GbCusDecMessageFunctionsList.Codes.QueryDeclaration, GbCusDecMessageFunctionsList.Descriptions.QueryDeclaration);
						result.AddPair(CDSEDIMessageTypeList.Codes.MasterQueryDeclaration, CDSEDIMessageTypeList.Descriptions.MasterQueryDeclaration);

						var credentialsSetting = declaration.GetCredentialsSettingByBadgeCode();
						var isDEPOrLoaderAllowedByRegistry = GBCustomsDataRegistry.Instance.CcsukShowDEPProfiles.Value;
						if (isDEPOrLoaderAllowedByRegistry && (credentialsSetting?.IsMaritimeLoader ?? false))
						{
							result.AddPair(GbCusDecMessageFunctionsList.Codes.ArrivalAtLocation, GbCusDecMessageFunctionsList.Descriptions.ArrivalAtLocation);
							result.AddPair(GbCusDecMessageFunctionsList.Codes.DepartureFromLocation, GbCusDecMessageFunctionsList.Descriptions.DepartureFromLocation);
							result.AddPair(GbCusDecMessageFunctionsList.Codes.AnticipatedArrivalAtLocation, GbCusDecMessageFunctionsList.Descriptions.AnticipatedArrivalAtLocation);
						}
						return result;
					}) :
					Factory.GetCachedValue("MessageTypesCodeList_GB_Import", () =>
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(CDSEDIMessageTypeList.Codes.NewDeclaration, CDSEDIMessageTypeList.Descriptions.NewDeclaration);
						result.AddPair(CDSEDIMessageTypeList.Codes.AmendDeclaration, CDSEDIMessageTypeList.Descriptions.AmendDeclaration);
						result.AddPair(CDSEDIMessageTypeList.Codes.CancelDeclaration, CDSEDIMessageTypeList.Descriptions.CancelDeclaration);
						result.AddPair(CDSEDIMessageTypeList.Codes.NilAmendment, CDSEDIMessageTypeList.Descriptions.NilAmendment);
						result.AddPair(CDSEDIMessageTypeList.Codes.FecChallenge, CDSEDIMessageTypeList.Descriptions.FecChallenge);
						result.AddPair(CDSEDIMessageTypeList.Codes.ArrivalNotification, CDSEDIMessageTypeList.Descriptions.ArrivalNotification);
						return result;
					});
			}
		}

		public CodeDescriptionPairList AmendmentReasonCodesList
		{
			get
			{
				var amendmentReasonList = Header.AddInfoLookups.AmendmentReasonCodeList;

				if (IsFECOrNilAmendment)
				{
					var clonedAmendmentReasonList = new CodeDescriptionPairList(amendmentReasonList);
					clonedAmendmentReasonList.RemoveCode(AmendmentCancellationReasonCode.Codes.C_GoodsPresentationNotice); // Not sure if this needs to be removed just for FEC and Nil amendments, spec was unclear.  Just removing from FEC and Nil for now.
					return clonedAmendmentReasonList;
				}

				return amendmentReasonList;
			}
		}

		#endregion

		#region Validation

		public new JobDeclarationMessageSendingObjectValidation Validation => (JobDeclarationMessageSendingObjectValidation)base.Validation;

		protected override Customs.Business.JobDeclarationMessageSendingObjectValidation GetNewValidation()
		{
			return new JobDeclarationMessageSendingObjectValidation(this);
		}

		#endregion
	}
}
