using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.WCB
{
	public enum FileFormat { Freightliner, Mercedes, Unknown }

	public class WCBDataConverter : FlatFileConverter
	{
		public WCBDataConverter(INotifications notification, BusinessObjectFactory factory)
			: base(notification, factory)
		{
		}
	}
}