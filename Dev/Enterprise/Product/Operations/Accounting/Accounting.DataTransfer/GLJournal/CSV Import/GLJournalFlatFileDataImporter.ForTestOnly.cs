#if DEBUG

using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.Accounting.DataTransfer.GLJournals
{
	public partial class GLJournalFlatFileDataImporter
	{
		public IFlatFileConverter CreateConverter_ForTestOnly(INotifications notificationSubscriber)
		{
			return CreateConverter(notificationSubscriber);
		}

		public IValueObject CreateXsd_ForTestOnly()
		{
			return CreateXsd();
		}

		public IFlatFileFormat FlatFileFormat_ForTestOnly => FlatFileFormat;
	}
}

#endif
