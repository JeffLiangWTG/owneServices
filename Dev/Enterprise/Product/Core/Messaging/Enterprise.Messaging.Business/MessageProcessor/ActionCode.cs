using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Messaging.MessageProcessors
{
	[Immutable]
	public class ActionCode
	{
		public static ActionCode Get(ZString code)
		{
			switch (code)
			{
				case "4":
					return Rejected;
				case "7":
					return Acknowledged;
				case "8":
					return Received;
				default:
					return new ActionCode("UNKNOWN");
			}
		}

		public ZString Description
		{
			get { return description; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "String used for testing")]
		static readonly ActionCode Rejected = new ActionCode("This level and all lower levels rejected");
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "String used for testing")]
		static readonly ActionCode Acknowledged = new ActionCode("This level acknowledged, next lower level acknowledged if not explicitly rejected");
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "String used for testing")]
		static readonly ActionCode Received = new ActionCode("Interchange received");

		readonly ZString description;

		ActionCode(ZString description)
		{
			this.description = description;
		}
	}
}