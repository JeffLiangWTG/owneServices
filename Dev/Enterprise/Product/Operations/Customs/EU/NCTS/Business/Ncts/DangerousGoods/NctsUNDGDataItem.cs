using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsUNDGDataItem : MasterFiles.Business.UNDGDataItem
	{
		public NctsUNDGDataItem(BusinessObjectFactory factory, System.Data.DataRow row) : base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore => Res.GetString("00E91F8C-B233-4CEB-95EA-063C78C97763", "Dangerous Goods Code");
	}
}
