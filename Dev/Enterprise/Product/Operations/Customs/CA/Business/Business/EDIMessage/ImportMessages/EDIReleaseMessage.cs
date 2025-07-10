using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact.D96A.Segments;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.CustomValues;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	[SystemDefinedValues]
	public class EDIReleaseMessage : EDIMessage
	{
		public EDIReleaseMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodes.CAIMP;
			EM_MessageType = MessageTypeList.Codes.EDIRelease;
		}

		protected override string GetMessageReferenceNumber()
		{
			return Env.NumberFountains.EDIFACTNumberFountain("M", "IMP", ApplicationCodes.CAIMP).GetNextFormatted(Factory);
		}

		protected override string GetEntryNumber()
		{
			var entryHeader = EM_LinkedObject as CusEntryHeader
				?? throw new ArgumentException("CusEntryHeader expected as Linked Object on an EDIReleaseMessage");
			return entryHeader.EntryNumber;
		}

		public override ZString CargoControlNumber
		{
			get { return this.GetSystemDefinedValue<ZString>(Schema.CargoControlNumber); }
		}

		public override ZString TransactionNumber
		{
			get { return this.GetSystemDefinedValue<ZString>(Schema.TransactionNumber); }
		}

		public override ZString SubLocation
		{
			get
			{
				var result = string.Empty;
				if (EdifactMessage is Enterprise.Edifact.D96A.Messages.CUSDEC.CUSDECMessage)
				{
					var cusdec = (Enterprise.Edifact.D96A.Messages.CUSDEC.CUSDECMessage)EdifactMessage;
					return cusdec.LOC[0].RelatedLocationOneIdentification.RelatedPlaceLocationOneIdentification;
				}
				else if (EdifactMessage is Enterprise.Edifact.D96A.Messages.CUSRES.CUSRESMessage)
				{
					var cusres = (Enterprise.Edifact.D96A.Messages.CUSRES.CUSRESMessage)EdifactMessage;
					if (cusres.LOC.Count > 0)
					{
						var loc = cusres.LOC[0];
						result = cusres.LOC[0].LocationIdentification.PlaceLocation;
					}
				}
				return result;
			}
		}

		public override ZString CBSAOffice
		{
			get
			{
				var result = string.Empty;
				if (EdifactMessage is Enterprise.Edifact.D96A.Messages.CUSDEC.CUSDECMessage)
				{
					var cusdec = (Enterprise.Edifact.D96A.Messages.CUSDEC.CUSDECMessage)EdifactMessage;
					return cusdec.LOC[0].LocationIdentification.PlaceLocationIdentification;
				}
				else if (EdifactMessage is Enterprise.Edifact.D96A.Messages.CUSRES.CUSRESMessage)
				{
					var cusres = (Enterprise.Edifact.D96A.Messages.CUSRES.CUSRESMessage)EdifactMessage;
					if (cusres.LOC.Count > 0)
					{
						var loc = cusres.LOC[0];
						result = cusres.LOC[0].LocationIdentification.PlaceLocationIdentification;
					}
				}
				return result;
			}
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var result = (EDIMessage)base.CloneInternal(args);
			result.EM_SystemCreateTimeUtc = EM_SystemCreateTimeUtc;
			foreach (var dynamicValue in this.GetSystemDefinedValues())
			{
				result.SetSystemDefinedValue(dynamicValue.PropertyName, dynamicValue.Value);
			}
			return result;
		}

		#region GetErrorDescription

		public override string GetErrorDescription(string errorCode)
		{
			var result = string.Empty;
			var errorCodeLocal = errorCode;
			if (errorCodeLocal.Length == 2)
			{
				result = RejectReasonCodes.GetDescriptionFromCode(errorCodeLocal);
				if (string.IsNullOrEmpty(result))
				{
					if (errorCodeLocal == "34")
					{
						errorCodeLocal = "W34";
					}
					else
					{
						errorCodeLocal = "R" + errorCodeLocal;
					}
				}
			}
			if (string.IsNullOrEmpty(result))
			{
				result = base.GetErrorDescription(errorCodeLocal);
			}

			return result;
		}

		ACROSSRejectReasonCodes RejectReasonCodes
		{
			get { return Factory.GetCachedValue<ACROSSRejectReasonCodes>(); }
		}

		#endregion

		#region Properties

		protected override CodeDescriptionPairList MessageSubTypeList
		{
			get { return EM_ReceiveTransmit == Direction.Receive ? new EDIReleaseImportEntryStatusList() : base.MessageSubTypeList; }
		}

		protected override bool ShouldUseUnformattedMessageText
		{
			get { return false; }
		}

		public override bool ShouldShowInterpretation
		{
			get { return true; }
		}

		#endregion

		#endregion

		#region System Defined Vales (GenAddOn Columns)

		public override ZDateTime RNSReleaseDate
		{
			get { return this.GetSystemDefinedValue<ZDateTime>(Schema.RNSReleaseDate); }
			set
			{
				var oldValue = RNSReleaseDate;
				this.SetSystemDefinedValue(Schema.RNSReleaseDate, value);
				RNSReleaseDateInfo.RefreshBinding(oldValue);
			}
		}

		public override ZDateTime RNSProcessingDate
		{
			get { return this.GetSystemDefinedValue<ZDateTime>(Schema.RNSProcessingDate); }
			set
			{
				var oldValue = RNSProcessingDate;
				this.SetSystemDefinedValue(Schema.RNSProcessingDate, value);
				RNSProcessingDateInfo.RefreshBinding(oldValue);
			}
		}

		#endregion

		internal static EDIReleaseMessage GetLastReleaseStatusMessage(Enterprise.Messaging.Business.EDIMessageCollection messages, ZString[] releaseSubTypesToIgnore)
		{
			var result = messages.GetLastMessage(EDIMessage.ApplicationCodes.CAIMP, MessageTypeList.Codes.EDIRelease, EDIMessage.Direction.Receive, new ZString[] { EDIMessage.Status.Received }, Array.Empty<ZString>(),
							Array.Empty<ZString>(), releaseSubTypesToIgnore)
				?? messages.GetLastMessage(EDIMessage.ApplicationCodes.CAIMP, MessageTypeList.Codes.EDIRelease, EDIMessage.Direction.Receive, EDIMessage.Status.Received);
			return (EDIReleaseMessage)result;
		}

		#region RNS Status & Notices columns
		ReleaseStatus ReleaseStatus
		{
			get { return releaseStatus ?? (releaseStatus = new ReleaseStatus(this)); }
		}
		ReleaseStatus releaseStatus;

		public override ZString ProcessingIndicator
		{
			get { return ZString.Format("{0} - {1}", ReleaseStatus.RL_ReleaseStatus, new EDIReleaseImportEntryStatusList().GetDescriptionFromCode(ReleaseStatus.RL_ReleaseStatus)); }
		}

		public override ZString ServiceOption
		{
			get { return ZString.Format("{0} - {1}", ReleaseStatus.RL_ServiceOption, new ServiceOptions().GetDescriptionFromCode(ReleaseStatus.RL_ServiceOption)); }
		}

		public override ZString DocumentReference
		{
			get
			{
				var cUsresMessage = EdifactMessage as Enterprise.Edifact.D96A.Messages.CUSRES.CUSRESMessage;
				return (cUsresMessage != null && cUsresMessage.BGM.Count > 0) ? new ZString(cUsresMessage.BGM[0].DocumentMessageNumber).Replace(" ", "") : ZString.Empty;
			}
		}

		public override ZString ReleaseOffice
		{
			get { return this.GetSystemDefinedValue<ZString>(ReleaseStatus.Schema.RL_ReleaseOffice); }
		}

		public override ZString WarehouseCode
		{
			get { return this.GetSystemDefinedValue<ZString>(ReleaseStatus.Schema.RL_WarehouseCode); }
		}

		public override ZString ContainerNumbers
		{
			get
			{
				var result = ZString.Empty;
				var cUsresMessage = EdifactMessage as Enterprise.Edifact.D96A.Messages.CUSRES.CUSRESMessage;
				if (cUsresMessage != null && cUsresMessage.EQD.Count > 0)
				{
					var builder = new ZStringBuilder();
					var lineBuilder = new ZStringBuilder();
					const byte containersOnLine = 6;
					byte count = 0;
					foreach (EQDSegment eqd in cUsresMessage.EQD)
					{
						lineBuilder.AppendIfNotEmpty(eqd.EquipmentIdentification.EquipmentIdentificationNumber);
						if (++count % containersOnLine == 0 || count == cUsresMessage.EQD.Count)
						{
							builder.Append(lineBuilder.ToStringWithDelimiterBetweenAppends(", "));
							lineBuilder = new ZStringBuilder();
						}
					}
					result = builder.ToStringWithDelimiterBetweenAppends(",\r\n");
				}
				return result;
			}
		}

		#endregion
	}
}
