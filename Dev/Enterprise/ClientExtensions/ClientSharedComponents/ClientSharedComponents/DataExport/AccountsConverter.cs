using CargoWise.EntityFramework;
using Enterprise.Accounting.DataTransfer.Invoices.FlatFile;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.ClientSharedComponents
{
	/// <summary>
	/// Accounting Data Conversion Class to a fixed or delimited text file.
	/// </summary>
	/// <remarks>When data exporting all AR & AP transactions into 1 file, inherit directly from here.</remarks>
	public abstract class AccountsConverter : AccountingFlatFileConverter
	{
		public AccountsConverter(BusinessObjectFactory factory, NotificationBuffer notifications)
			: base(null, factory)
		{
			this.notifications = notifications;
		}

		protected override FlatFileDataRowCollection MapExport(IValueObject valueObject)
		{
			FlatFileDataRowCollection result = new FlatFileDataRowCollection();
			Xsd.TxnHeader xmlHeader = valueObject as Xsd.TxnHeader;

			if (xmlHeader != null && IsOkToProcess(xmlHeader))
			{
				result.Add(ExportAccounts(xmlHeader));
			}
			return result;
		}

		protected NotificationBuffer notifications;
		protected abstract bool IsOkToProcess(Xsd.TxnHeader xmlHeader);
		protected abstract FlatFileDataRowCollection ExportAccounts(Xsd.TxnHeader xmlHeader);
	}
}
