using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Registry.Business
{
	public class ApplicationCodeObjCollectionMergeManager
	{
		public ApplicationCodeObjCollection MergeApplicationCodeObjCollections(ApplicationCodeObjCollection primaryCollection, ApplicationCodeObjCollection secondaryCollection)
		{
			var mergedCollection = new ApplicationCodeObjCollection();
			var allApplicationCodes = new Dictionary<ZString, ApplicationCodeObj>();

			foreach (ApplicationCodeObj appCodeObj in secondaryCollection)
			{
				allApplicationCodes[appCodeObj.ApplicationCode] = appCodeObj;
			}

			foreach (ApplicationCodeObj appCodeObj in primaryCollection)
			{
				if (!appCodeObj.IsUnpurgable && allApplicationCodes.ContainsKey(appCodeObj.ApplicationCode))
				{
					var mergedAppCodeObj = MergeApplicationCodeObjs(appCodeObj, allApplicationCodes[appCodeObj.ApplicationCode]);
					allApplicationCodes[appCodeObj.ApplicationCode] = mergedAppCodeObj;
				}
				else
				{
					allApplicationCodes[appCodeObj.ApplicationCode] = appCodeObj;
				}
			}

			foreach (var appCodeObj in allApplicationCodes.Values)
			{
				mergedCollection.Add(appCodeObj);
			}

			return mergedCollection;
		}

		ApplicationCodeObj MergeApplicationCodeObjs(ApplicationCodeObj primaryObj, ApplicationCodeObj secondaryObj)
		{
			var primaryInterchangeObj = (InterchangeObj)primaryObj.Interchanges.FirstOrDefault();
			var secondaryInterchangeObj = (InterchangeObj)secondaryObj.Interchanges.FirstOrDefault();
			primaryObj.Interchanges.RemoveAll();
			if (primaryInterchangeObj != null && secondaryInterchangeObj != null)
			{
				primaryObj.Interchanges.Add(MergeInterchangeObjs(primaryInterchangeObj, secondaryInterchangeObj));
			}

			if (primaryObj.LatestPurgedMessageTimeUtc != secondaryObj.LatestPurgedMessageTimeUtc)
			{
				primaryObj.LatestPurgedMessageTimeUtc = secondaryObj.LatestPurgedMessageTimeUtc;
			}

			if (primaryObj.Selected != secondaryObj.Selected)
			{
				primaryObj.Selected = secondaryObj.Selected;
			}

			if (primaryObj.PurgeType.Equals(PurgeTypeList.ApplicationCode))
			{
				if (secondaryObj.PurgeTime > 0 && secondaryObj.PurgeTimeUnit != ZGuid.Empty)
				{
					if (primaryObj.PurgeTime != secondaryObj.PurgeTime)
					{
						primaryObj.PurgeTime = secondaryObj.PurgeTime;
					}

					if (primaryObj.PurgeTimeUnit != secondaryObj.PurgeTimeUnit)
					{
						primaryObj.PurgeTimeUnit = secondaryObj.PurgeTimeUnit;
					}
				}
				else
				{
					SetPurgeSettingFromMessageType(primaryObj, secondaryObj);
				}
			}

			if (!primaryObj.PurgeType.Equals(PurgeTypeList.ApplicationCode))
			{
				primaryObj.SetMessageTypes(MergeMessageTypeObjCollections(primaryObj.MessageTypes, secondaryObj.MessageTypes));
			}
			else
			{
				primaryObj.SetMessageTypes(new MessageTypeObjCollection());
			}

			return primaryObj;
		}

		void SetPurgeSettingFromMessageType(ApplicationCodeObj primaryObj, ApplicationCodeObj secondaryObj)
		{
			var messageType = (MessageTypeObj)secondaryObj.MessageTypes.FirstOrDefault();
			if (messageType != null && messageType.PurgeTime > 0 && messageType.PurgeTimeUnit != ZGuid.Empty)
			{
				primaryObj.PurgeTime = messageType.PurgeTime;
				primaryObj.PurgeTimeUnit = messageType.PurgeTimeUnit;
				primaryObj.Selected = messageType.Selected;
			}
		}

		InterchangeObj MergeInterchangeObjs(InterchangeObj primaryObj, InterchangeObj secondaryObj)
		{
			if (primaryObj.Selected != secondaryObj.Selected)
			{
				primaryObj.Selected = secondaryObj.Selected;
			}

			if (primaryObj.PurgeTime != secondaryObj.PurgeTime)
			{
				primaryObj.PurgeTime = secondaryObj.PurgeTime;
			}

			if (primaryObj.PurgeTimeUnit != secondaryObj.PurgeTimeUnit)
			{
				primaryObj.PurgeTimeUnit = secondaryObj.PurgeTimeUnit;
			}

			if (primaryObj.LatestPurgedMessageTimeUtc != secondaryObj.LatestPurgedMessageTimeUtc)
			{
				primaryObj.LatestPurgedMessageTimeUtc = secondaryObj.LatestPurgedMessageTimeUtc;
			}

			return primaryObj;
		}

		MessageTypeObjCollection MergeMessageTypeObjCollections(MessageTypeObjCollection primaryCollection, MessageTypeObjCollection secondaryCollection)
		{
			var mergedCollection = new MessageTypeObjCollection();
			var allMessageTypes = new Dictionary<(ZString MessageType, ZString MessageSubType), MessageTypeObj>();

			foreach (MessageTypeObj msgTypeObj in secondaryCollection)
			{
				var key = (msgTypeObj.MessageType, msgTypeObj.MessageSubType);
				allMessageTypes[key] = msgTypeObj;
			}

			foreach (MessageTypeObj msgTypeObj in primaryCollection)
			{
				var key = (msgTypeObj.MessageType, msgTypeObj.MessageSubType);
				if (allMessageTypes.ContainsKey(key))
				{
					var mergedMsgTypeObj = MergeMessageTypeObjs(msgTypeObj, allMessageTypes[key]);
					allMessageTypes[key] = mergedMsgTypeObj;
				}
				else
				{
					allMessageTypes[key] = msgTypeObj;
				}
			}

			foreach (var msgTypeObj in allMessageTypes.Values)
			{
				mergedCollection.Add(msgTypeObj);
			}

			return mergedCollection;
		}

		MessageTypeObj MergeMessageTypeObjs(MessageTypeObj primaryObj, MessageTypeObj secondaryObj)
		{
			if (primaryObj.Selected != secondaryObj.Selected)
			{
				primaryObj.Selected = secondaryObj.Selected;
			}

			if (primaryObj.PurgeTime != secondaryObj.PurgeTime)
			{
				primaryObj.PurgeTime = secondaryObj.PurgeTime;
			}

			if (primaryObj.PurgeTimeUnit != secondaryObj.PurgeTimeUnit)
			{
				primaryObj.PurgeTimeUnit = secondaryObj.PurgeTimeUnit;
			}

			if (primaryObj.LatestPurgedMessageTimeUtc != secondaryObj.LatestPurgedMessageTimeUtc)
			{
				primaryObj.LatestPurgedMessageTimeUtc = secondaryObj.LatestPurgedMessageTimeUtc;
			}

			return primaryObj;
		}
	}
}
