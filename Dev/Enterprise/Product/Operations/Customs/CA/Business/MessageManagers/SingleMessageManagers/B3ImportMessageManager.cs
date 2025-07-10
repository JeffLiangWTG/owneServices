using CargoWise.Types;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Messaging.MessageBuilders;

namespace Enterprise.Customs.CA.Business.MessageManagers
{
	public class B3ImportMessageManager : B3CADBaseMessageManager
	{
		public B3ImportMessageManager(IB3Header b3Header, IUserNotification notification, B3DeferInstruction deferInstruction = null, bool fromServiceTask = false)
			: base(b3Header, notification, deferInstruction, new B3ImportStatusCalculator(), fromServiceTask)
		{
		}

		protected override ZString MessageTypeForDisplay => "B3";

		protected override IMessageBuilder GetMessageBuilder(MessageSubTypes actionCode)
		{
			return new B3CusdecMessageBuilder<B3Message>(DataWrapper, actionCode);
		}

		protected override string EntryHasBeenLodged(JobDeclaration declaration)
		{
			if (declaration.IsB3Lodged)
			{
				return Res.GetString("D250C58C-2413-4CA1-BF01-73D29CBE3119", "A B3 has already been lodged for this Declaration.");
			}
			return ZString.Empty;
		}

		protected override ZBool PreventSendingMessageWhenEntryStatusIsCleared
		{
			get
			{
				var declaration = BusinessObject as JobDeclaration;
				var b3EntryHeader = declaration?.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);
				return b3EntryHeader != null && (b3EntryHeader.CH_EntryStatus == B3EntryStatusList.Codes.Accepted || b3EntryHeader.CH_EntryStatus == B3EntryStatusList.Codes.Confirmed);
			}
		}
	}
}
