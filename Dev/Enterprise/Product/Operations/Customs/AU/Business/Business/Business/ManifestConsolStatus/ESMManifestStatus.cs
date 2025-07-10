using System;
using System.Collections;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageBuilders;
using Enterprise.Edifact;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ESMManifestStatus : AUCustomsManifestStatus
	{
		public abstract new class Schema : Customs.Business.CustomsManifestStatus.Schema
		{
			public const string ContingencyCAN = "ContingencyCAN";
		}

		public ESMManifestStatus(ForwardingConsol consol)
			: base(consol)
		{
		}

		protected override string MessageApplicationCode
		{
			get { return EDIMessage.ApplicationCodes.CMR; }
		}

		protected override ZString[] MessageTypes
		{
			get { return new ZString[] { CMRMessage.CMRMessageTypes.ESM, CMRMessage.CMRMessageTypes.IDL }; }
		}

		public override ZString E2_CustomsEntryNumber => Factory.GetValue(ref customsEntryNumberCached, delegate
		{
			var entryNum = new FreightConsolWrapper(ManifestProvider).GetPermit();
			return entryNum != null ? entryNum.CE_EntryNum : new ZString(EmptyCustomsEntryNumberString);
		});

		CachedProperty<ZString> customsEntryNumberCached;

		public override ZString E2_CustomsEntryNumberHumanReadableName
		{
			get { return "CRN"; }
		}

		protected override void DoResetToOriginal()
		{
			foreach (var message in TransmittedMessages)
			{
				message.EM_Status = EDIMessage.Status.Discarded;
			}

			foreach (var message in ReceivedMessages)
			{
				message.EM_Status = EDIMessage.Status.Discarded;
			}

			var consolWrapper = new FreightConsolWrapper(ManifestProvider);
			if (consolWrapper != null)
			{
				consolWrapper.RemoveAllESMLineNumbers();
				var entryNum = consolWrapper.GetPermit();
				if (entryNum != null)
				{
					entryNum.Delete();
				}
			}

			customsEntryNumberCached = null;
			ManifestProvider.HasChanges = true;
		}

		protected override bool IsContingencyCANInfoReadonly()
		{
			return E2_CustomsEntryNumber != EmptyCustomsEntryNumberString;
		}

		protected override Customs.Business.ManifestStatus GetMessageStatus(EDIMessage message)
		{
			var result = ManifestStatus.NotSent;
			if (message.EM_MessageType == CMRMessage.CMRMessageTypes.ESM)
			{
				var edifactMessage = new CUSRESMessage();
				try
				{
					edifactMessage.Parse(new UNOCCMRCharacterSet(), message.EM_MessageText);
					if (edifactMessage.FTX[0].TextLiteral.FreeTextValue1 == "CLEAR")
					{
						result = ManifestStatus.Cleared;
					}
					else
					{
						result = ManifestStatus.FromString(edifactMessage.FTX[0].TextLiteral.FreeTextValue1);
					}
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					ErrorReporter.ReportOnce("An exception occured trying to find the status of an ESM consol", e);
				}
			}
			else if (message.EM_MessageType == CMRMessage.CMRMessageTypes.IDL)
			{
				result = ManifestStatus.Idle;
			}
			else
			{
				ErrorReporter.ReportOnce("ESMManifestStatus.GetMessageStatus.UnknownMessage", "Attempted to process an unknown message type (" + message.EM_MessageType + ") in the ESMManifestStatus.GetMessageStatus() method. This is probably due to a new message type being added to the MessageTypes override, without any code being added to handle it in the status processing."); // Column name used in error message, not key
			}
			return result;
		}

		public override IManifestMessageBuilder NewCreateOrReplaceMessageBuilder()
		{
			var builder = new ESMMessageBuilder(ManifestProvider, false);
			builder.MessageSubType = ESMMessageBuilder.GetMessageSubType(ManifestProvider);
			return builder;
		}

		protected override IManifestMessageBuilder[] NewMessageBuilders(Common.MessageBuilders.MessageSubTypes messageSubType)
		{
			var result = new ArrayList();
			var builder = new ESMMessageBuilder(ManifestProvider, false);
			builder.MessageSubType = messageSubType;

			if (messageSubType == Common.MessageBuilders.MessageSubTypes.Withdraw)
			{
				builder.SetStatusToPending = true;
				builder.VersionOffset = 1;

				var zeroLineReplacement = new ESMMessageBuilder(ManifestProvider, false);
				zeroLineReplacement.MessageSubType = Common.MessageBuilders.MessageSubTypes.Replace;
				zeroLineReplacement.DontSendAnyLines = true;
				result.Add(zeroLineReplacement);
			}
			result.Add(builder);
			return (IManifestMessageBuilder[])result.ToArray(typeof(IManifestMessageBuilder));
		}
	}
}
