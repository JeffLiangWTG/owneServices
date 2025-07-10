
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.JAS.Module
{
	public static class ClientControllerRegistration
	{
		public static readonly ClientControllerID ExportAccountingDataForPreMatching = new ClientControllerID("ExportAccountingDataForPreMatching");
		public static readonly ClientControllerID ImportMatchedTransactions = new ClientControllerID("ImportMatchedTransactions");
		public static readonly ClientControllerID ExportCognosCsv = new ClientControllerID("ExportCognosCsv");
		public static readonly ClientControllerID ImportJXCFile = new ClientControllerID("ImportJXCFile");
	}
}