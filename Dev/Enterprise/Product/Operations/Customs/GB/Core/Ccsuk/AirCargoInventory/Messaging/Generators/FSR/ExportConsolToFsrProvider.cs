using CargoWise.Types;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class ExportConsolToFsrProvider : IFSR
	{
		public ExportConsolToFsrProvider(MawbExportAddInfo exportConsolHelper, string fsrRequestType)
		{
			this.exportConsolHelper = exportConsolHelper;
			this.requestType = fsrRequestType;
		}

		public ZString AirwaybillPrefixAndAirwaybillNumber
		{
			get { return exportConsolHelper.Consol.JK_MasterBillNum; }
		}

		public ZString HousewaybillNumber
		{
			get { return ""; }
		}

		public ZString SplitReference
		{
			get { return ""; }
		}

		public ZString ResponseRequiredIndicator
		{
			get { return requestType; }
		}

		public ZString Airport
		{
			get { return exportConsolHelper.ME_ExportLocation; }
		}

		public ZString ShedOperatorIdentity
		{
			get { return exportConsolHelper.ME_ExportShed; }
		}

		public ZString RecipientID
		{
			get { return ""; }
		}

		readonly MawbExportAddInfo exportConsolHelper;
		readonly string requestType;
	}
}
