using Enterprise.Customs.Common.IE;
using Enterprise.Customs.IE.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business
{
	public class AESMessageSendingActionLookups : CusEntryHeaderMessageSendingActionLookups
	{
		public AESMessageSendingActionLookups(AESMessageSendingAction parent) : base(parent) { }

		protected new AESMessageSendingAction Parent => (AESMessageSendingAction)base.Parent;

		public override CodeDescriptionPairList SendingActionTypeList
		{
			get
			{
				var messageType = Parent.EntryHeader.Declaration.JE_MessageType;

				return Factory.GetCachedValue($"Enterprise.Customs.IE.Business.AESMessageSendingActionLookups.SendingActionTypeList|{messageType}", () =>
				{
					var result = new CodeDescriptionPairList();

					if (messageType == IEJobMessageTypeList.Codes.Export)
					{
						result.AddPair(AESOutgoingMessageTypeList.Codes.ExportPresentation, AESOutgoingMessageTypeList.Descriptions.ExportPresentation);
						result.AddPair(AESOutgoingMessageTypeList.Codes.ExportAmendment, AESOutgoingMessageTypeList.Descriptions.ExportAmendment);
						result.AddPair(AESOutgoingMessageTypeList.Codes.ExportCancellation, AESOutgoingMessageTypeList.Descriptions.ExportCancellation);
						result.AddPair(AESOutgoingMessageTypeList.Codes.ExportOriginal, AESOutgoingMessageTypeList.Descriptions.ExportOriginal);
						result.AddPair(AESOutgoingMessageTypeList.Codes.ReleaseAmendment, AESOutgoingMessageTypeList.Descriptions.ReleaseAmendment);
					}
					else if (messageType == IEJobMessageTypeList.Codes.ReExport)
					{
						result.AddPair(AESOutgoingMessageTypeList.Codes.ReExport, AESOutgoingMessageTypeList.Descriptions.ReExport);
						result.AddPair(AESOutgoingMessageTypeList.Codes.ReExportAmendment, AESOutgoingMessageTypeList.Descriptions.ReExportAmendment);
						result.AddPair(AESOutgoingMessageTypeList.Codes.ExitCancellation, AESOutgoingMessageTypeList.Descriptions.ExitCancellation);
					}
					else if (messageType == IEJobMessageTypeList.Codes.ExitSummary)
					{
						result.AddPair(AESOutgoingMessageTypeList.Codes.ExitOriginal, AESOutgoingMessageTypeList.Descriptions.ExitOriginal);
						result.AddPair(AESOutgoingMessageTypeList.Codes.ExitAmendment, AESOutgoingMessageTypeList.Descriptions.ExitAmendment);
						result.AddPair(AESOutgoingMessageTypeList.Codes.ExitCancellation, AESOutgoingMessageTypeList.Descriptions.ExitCancellation);
					}

					result.Sort();
					return result;
				});
			}
		}

		public override CodeDescriptionPairList SendingActionTypeListForDisplay
		{
			get
			{
				var messageType = Parent.EntryHeader.Declaration.JE_MessageType;

				return Factory.GetCachedValue($"Enterprise.Customs.IE.Business.AESMessageSendingActionLookups.SendingActionTypeListForDisplay|{messageType}", () =>
				{
					var result = new CodeDescriptionPairList();

					if (messageType == IEJobMessageTypeList.Codes.Export)
					{
						result.AddPair(AESOutgoingMessageTypeList.Codes.ExportPresentation, AESOutgoingMessageTypeListForDisplay.Codes.ExportPresentation, AESOutgoingMessageTypeListForDisplay.Descriptions.ExportPresentation);
						result.AddPair(AESOutgoingMessageTypeList.Codes.ExportAmendment, AESOutgoingMessageTypeListForDisplay.Codes.ExportAmendment, AESOutgoingMessageTypeListForDisplay.Descriptions.ExportAmendment);
						result.AddPair(AESOutgoingMessageTypeList.Codes.ExportCancellation, AESOutgoingMessageTypeListForDisplay.Codes.ExportCancellation, AESOutgoingMessageTypeListForDisplay.Descriptions.ExportCancellation);
						result.AddPair(AESOutgoingMessageTypeList.Codes.ExportOriginal, AESOutgoingMessageTypeListForDisplay.Codes.ExportOriginal, AESOutgoingMessageTypeListForDisplay.Descriptions.ExportOriginal);
						result.AddPair(AESOutgoingMessageTypeList.Codes.ReleaseAmendment, AESOutgoingMessageTypeListForDisplay.Codes.ReleaseAmendment, AESOutgoingMessageTypeListForDisplay.Descriptions.ReleaseAmendment);
					}
					else if (messageType == IEJobMessageTypeList.Codes.ReExport)
					{
						result.AddPair(AESOutgoingMessageTypeList.Codes.ReExport, AESOutgoingMessageTypeListForDisplay.Codes.ReExport, AESOutgoingMessageTypeListForDisplay.Descriptions.ReExport);
						result.AddPair(AESOutgoingMessageTypeList.Codes.ReExportAmendment, AESOutgoingMessageTypeListForDisplay.Codes.ReExportAmendment, AESOutgoingMessageTypeListForDisplay.Descriptions.ReExportAmendment);
						result.AddPair(AESOutgoingMessageTypeList.Codes.ExitCancellation, AESOutgoingMessageTypeListForDisplay.Codes.ExitCancellation, AESOutgoingMessageTypeListForDisplay.Descriptions.ExitCancellation);
					}
					else if (messageType == IEJobMessageTypeList.Codes.ExitSummary)
					{
						result.AddPair(AESOutgoingMessageTypeList.Codes.ExitOriginal, AESOutgoingMessageTypeListForDisplay.Codes.ExitOriginal, AESOutgoingMessageTypeListForDisplay.Descriptions.ExitOriginal);
						result.AddPair(AESOutgoingMessageTypeList.Codes.ExitAmendment, AESOutgoingMessageTypeListForDisplay.Codes.ExitAmendment, AESOutgoingMessageTypeListForDisplay.Descriptions.ExitAmendment);
						result.AddPair(AESOutgoingMessageTypeList.Codes.ExitCancellation, AESOutgoingMessageTypeListForDisplay.Codes.ExitCancellation, AESOutgoingMessageTypeListForDisplay.Descriptions.ExitCancellation);
					}

					result.Sort();
					return result;
				});
			}
		}
	}
}
