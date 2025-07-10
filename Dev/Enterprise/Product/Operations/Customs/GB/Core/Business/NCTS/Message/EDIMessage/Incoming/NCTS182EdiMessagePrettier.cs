using System.Globalization;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class NCTS182EdiMessagePrettier : NCTS182ResponsePrettyFormatter
	{
		public NCTS182EdiMessagePrettier(Enterprise.Messaging.Business.EDIMessage message) : base(message)
		{
		}

		protected override string GetDateFormat(ZString dateInput)
		{
			var dateParsed = ZDateTime.ParseISO8601DateSafe(dateInput);
			return dateParsed.IsEmpty ? dateInput : dateParsed.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
		}

		protected override string UNLocoCaption => Res.GetString("B4B47A52-D1A7-4FC3-AD71-271F49D4CFC5", "UNLOCO:");
	}
}
