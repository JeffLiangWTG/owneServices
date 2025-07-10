#if DEBUG

using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Accounting.DataTransfer.GLHeadersAndChargeCodes
{
	public partial class GLHeaderAndChargeCodeFlatFileDataImporter
	{
		public IFlatFileConverter CreateConverter_ForTestOnly(INotifications notificationSubscriber)
		{
			return CreateConverter(notificationSubscriber);
		}

		public Enterprise.DataTransfer.Xml.IValueObject CreateXsd_ForTestOnly()
		{
			return CreateXsd();
		}

		public IFlatFileFormat FlatFileFormat_ForTestOnly => FlatFileFormat;
	}
}

#endif
