using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.DataTransfer
{
	public class CASSHOTFileDataImporter : FlatFileDataImporter
	{
		public CASSHOTFileDataImporter(CASSBilling cassBilling, string fileName)
			: this(cassBilling)
		{
			this.fileName = fileName;
		}

		public CASSHOTFileDataImporter(CASSBilling cassBilling)
			: base(cassBilling)
		{
		}

		readonly string fileName;

		CASSBilling CASSBusinessEntity
		{
			get { return (CASSBilling)base.BusinessEntity; }
		}

		public void ImportData(string localFile, INotifications notifications)
		{
			ImportData(localFile, notifications, new SourceInfo(BillingDataSource.InterfaceConnector, BillingInterfaceName.ImportCASS, ZGuid.Empty, ZGuid.Empty, ZString.Empty, localFile)); // Interface name for billing purposes
		}

		protected override IValueObject CreateXsd()
		{
			return new CASSCostHeader();
		}

		protected override IFlatFileConverter CreateConverter(INotifications notificationSubscriber)
		{
			return new CASSHOTFileConverter(notificationSubscriber, FactoryProvider.Current);
		}

		protected override IFlatFileFormat FlatFileFormat
		{
			get { return new CASSHOTFileFormat(); }
		}

		protected override bool ExtractToDataAdapter(IValueObject xSD, INotifications notifications)
		{
			var valueObject = xSD as CASSCostHeader;
			valueObject.HOTFileName = fileName;
			CASSBusinessEntity.Initialize(valueObject);
			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1135: DoNotUseCountrySpecificBusinessRule", Justification = "Testing")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Used as file filter")]
		public static string GetFileFormat()
		{
			var fileFormat = string.Empty;
			if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.UnitedStates)
			{
				fileFormat = "CASS FIL files (*.fil)|*.fil";
			}
			else
			{
				fileFormat = "CASS HOT files (*.hot)|*.hot";
			}

			fileFormat += "|All files (*.*)|*.*";
			return fileFormat;
		}
	}
}
