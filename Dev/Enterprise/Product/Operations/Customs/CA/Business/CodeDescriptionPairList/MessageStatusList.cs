using System;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class MessageStatusList : Common.Shared.MessageStatusList, IStatusList
	{
		public MessageStatusList()
		{
		}

		public MessageStatusList(MultilingualString messageDescription)
			: base(messageDescription)
		{
		}

		#region IStatusList Members
		public string GetFirstClearStatusFor(MessageType messageType)
		{
			switch (messageType)
			{
				case MessageType.DataLoadingModule:
					return Codes.Sent;
				case MessageType.G7ExportDeclaration:
				case MessageType.EDIRelease:
				case MessageType.B3Cusdec:
					return Codes.ClearOriginal;
				case MessageType.Undefined:
					return "";
				default:
					throw new NotSupportedException(string.Format("Message type '{0}' is not supported in {1}", messageType, GetType().FullName));
			}
		}
		#endregion

		internal static bool IsMessageStatusAllowCancellation(ZString status)
		{
			return status == Codes.NotSent ||
				status == Codes.ClearDelete ||
				status == Codes.ErrorOriginal;
		}

		internal static bool IsMessageAccepted(ZString status)
		{
			return status == Codes.ClearOriginal ||
				status == Codes.ClearDelete ||
				status == Codes.ClearChange;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		internal static ZString GetAppropriateErrorCode(ZString status)
		{
			switch (status)
			{
				case Codes.AwaitingOriginal:
				case Codes.ClearOriginal:
				case Codes.AcknowledgedOriginal:
					return Codes.ErrorOriginal;
				case Codes.AwaitingChange:
				case Codes.ClearChange:
				case Codes.AcknowledgedChange:
					return Codes.ErrorChange;
				case Codes.AwaitingDelete:
				case Codes.ClearDelete:
				case Codes.AcknowledgedDelete:
					return Codes.ErrorDelete;
				case Codes.AwaitingReplace:
				case Codes.ClearReplace:
				case Codes.AcknowledgedReplace:
					return Codes.ErrorReplace;
				default:
					return ZString.Empty;
			}
		}

		internal static ZString GetAppropriateClearCode(ZString status)
		{
			switch (status)
			{
				case Codes.AwaitingOriginal:
					return Codes.ClearOriginal;
				case Codes.AwaitingChange:
					return Codes.ClearChange;
				case Codes.AwaitingReplace:
					return Codes.ClearReplace;
				case Codes.AwaitingDelete:
					return Codes.ClearDelete;
				default:
					return ZString.Empty;
			}
		}

		protected override void AddDefaultPairs(MultilingualString multilingualDescription)
		{
			base.AddDefaultPairs(multilingualDescription);
			if (multilingualDescription != null && !multilingualDescription.IsEmpty)
			{
				AddOverwriteIfExists(new CodeDescriptionPair(Codes.AcknowledgedReplace, ResString.GetMultilingualString("CAMessageStatusList|AcknowledgedReplace", "Acknowledged {0} Amendment", multilingualDescription)));
				AddOverwriteIfExists(new CodeDescriptionPair(Codes.AwaitingReplace, ResString.GetMultilingualString("CAMessageStatusList|AwaitingReplace", "Awaiting {0} Amendment", multilingualDescription)));
				AddOverwriteIfExists(new CodeDescriptionPair(Codes.ClearReplace, ResString.GetMultilingualString("CAMessageStatusList|ClearReplace", "Accepted {0} Amendment", multilingualDescription)));
				AddOverwriteIfExists(new CodeDescriptionPair(Codes.ErrorReplace, ResString.GetMultilingualString("CAMessageStatusList|ErrorReplace", "Error {0} Amendment", multilingualDescription)));
			}
			else
			{
				AddOverwriteIfExists(new CodeDescriptionPair(Codes.AcknowledgedReplace, ResString.GetMultilingualString("CAMessageStatusList|AcknowledgedReplace2", "Acknowledged Amendment")));
				AddOverwriteIfExists(new CodeDescriptionPair(Codes.AwaitingReplace, ResString.GetMultilingualString("CAMessageStatusList|AwaitingReplace2", "Awaiting Amendment")));
				AddOverwriteIfExists(new CodeDescriptionPair(Codes.ClearReplace, ResString.GetMultilingualString("CAMessageStatusList|ClearReplace2", "Accepted Amendment")));
				AddOverwriteIfExists(new CodeDescriptionPair(Codes.ErrorReplace, ResString.GetMultilingualString("CAMessageStatusList|ErrorReplace2", "Error Amendment")));
			}
			Sort();
		}
	}
}
