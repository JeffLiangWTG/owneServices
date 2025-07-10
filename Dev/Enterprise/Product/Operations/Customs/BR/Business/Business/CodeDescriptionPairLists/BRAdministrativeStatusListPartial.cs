using CargoWise.Types;

namespace Enterprise.Customs.BR.Business
{
	public partial class BRAdministrativeStatusList
	{
		public static string MapToCWCode(ZString eventId)
		{
			switch (eventId)
			{
				case "DEFERIDO":
					return Codes.Deferred;
				case "DISPENSADO":
					return Codes.Dispensed;
				case "PENDENTE":
					return Codes.Pending;
				case "EM_PROCESSAMENTO":
					return Codes.InProcess;
				case "IMPEDIDO":
					return Codes.Blocked;
				default:
					return string.Empty;
			}
		}
	}
}
