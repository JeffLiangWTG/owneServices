using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.TNT
{
	public abstract class IQDownBaseRecord : FlatFileDataRow
	{
		protected IQDownBaseRecord(ZString rawData, int fieldCount)
			: base(fieldCount)
		{
			FillFromRawData(rawData);
		}

		public bool IsValid(INotifications notify)
		{
			bool result = true;

			if (RecordDelimiter != ".")
			{
				notify.Notify(new ErrorNotification(TNTErrorType.InvalidRecordDelimiter, " Error was encouter on " + HumanReadable));
				result = false;
			}

			return result;
		}

		protected abstract void FillFromRawData(ZString rawData);

		protected void SetProperty(FixedWidthFlatFileFieldProperty property, ZString rawData)
		{
			this[property.Name] = rawData.SubstringSafe(property.Position, property.Length);
		}

		public abstract ZString RecordDelimiter { get; }
		public abstract ZString HumanReadable { get; }
			}
}
