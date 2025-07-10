using System;
using System.Collections.Generic;
using CargoWise.Common;

namespace Enterprise.xTMessaging.Shared
{
	public class MetaDataHelper
	{
		public Dictionary<string, string> MetaData { get; }

		public MetaDataHelper(Dictionary<string, string> metaData)
		{
			MetaData = metaData;

			if (metaData.ContainsKey(Constants.xTMsgAttributes.refexternal))
			{
				if (ulong.TryParse(metaData[Constants.xTMsgAttributes.refexternal], out var refexternalLong))
				{
					OriginalMessageId = Shared.Utils.ConvertUnsignedLongToLong(refexternalLong);
				}
				else if (metaData[Constants.xTMsgAttributes.refexternal].StartsWith(xTMsgUriPrefix))
				{
					OriginalMessageUri = metaData[Constants.xTMsgAttributes.refexternal];
				}
			}

			if (MessageTrackingId == Guid.Empty && OriginalMessageId.Equals(0) && string.IsNullOrEmpty(OriginalMessageUri))
			{
				throw new Exception("Missing Key Message Information");
			}
		}

		public string ApplicationCode => MetaData.GetValueSafe(Constants.CustomMsgAttributes.ApplicationCode) ?? string.Empty;

		public string SourceParty => MetaData.GetValueSafe(Constants.CustomMsgAttributes.SourceParty) ?? string.Empty;

		public string DestinationParty => MetaData.GetValueSafe(Constants.CustomMsgAttributes.DestinationParty) ?? string.Empty;

		public string MessageType => MetaData.GetValueSafe(Constants.CustomMsgAttributes.MessageType) ?? string.Empty;

		public int ReceivingRetryCount
		{
			get
			{
				int.TryParse(MetaData.GetValueSafe(Constants.CustomMsgAttributes.ReceivingRetryCount), out var result);
				return result;
			}
		}
		public Guid MessageTrackingId
		{
			get
			{
				if (Guid.TryParse(MetaData.GetValueSafe(Constants.CustomMsgAttributes.MessageTrackingID), out var guid))
				{
					return guid;
				}
				return Guid.Empty;
			}
		}

		public long OriginalMessageId { get; }

		public string OriginalMessageUri { get; }

		public string EDIMessageCreatorId => MetaData.GetValueSafe(Constants.CustomMsgAttributes.EDIMessageCreatorID) ?? string.Empty;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public bool CreateEDIMessage
		{
			get
			{
				var valueInStr = MetaData.GetValueSafe(Constants.CustomMsgAttributes.CreateEDIMessage);
				return valueInStr != null && (valueInStr.Equals("Y", System.StringComparison.OrdinalIgnoreCase) || valueInStr.Equals("True", System.StringComparison.OrdinalIgnoreCase));
			}
		}

		public bool ContainsKey(string key) => MetaData.ContainsKey(key);

		public void MergeMessageAttributesFromOriginalInfo(Dictionary<string, string> orgMsgAttr)
		{
			if (MetaData != null && orgMsgAttr != null)
			{
				MergeIfNeeded(Constants.CustomMsgAttributes.SourceParty, Constants.CustomMsgAttributes.DestinationParty);
				MergeIfNeeded(Constants.CustomMsgAttributes.DestinationParty, Constants.CustomMsgAttributes.SourceParty);
				MergeIfNeeded(Constants.CustomMsgAttributes.ApplicationCode, Constants.CustomMsgAttributes.ApplicationCode);
				MergeIfNeeded(Constants.CustomMsgAttributes.MessageType, Constants.CustomMsgAttributes.MessageType);
				MergeIfNeeded(Constants.CustomMsgAttributes.MessageTrackingID, Constants.CustomMsgAttributes.MessageTrackingID);
			}
			return;

			void MergeIfNeeded(string receivingKey, string originalKey)
			{
				if ((!MetaData.ContainsKey(receivingKey) || string.IsNullOrEmpty(MetaData[receivingKey])) && orgMsgAttr.ContainsKey(originalKey))
				{
					MetaData[receivingKey] = orgMsgAttr[originalKey];
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		internal string xTMsgUriPrefix => "xt-msg:";
	}
}
