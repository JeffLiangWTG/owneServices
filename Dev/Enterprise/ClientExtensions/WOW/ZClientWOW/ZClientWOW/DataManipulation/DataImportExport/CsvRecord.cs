using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.Wow
{
	public abstract class CsvRecord
	{
		protected CsvRecord(string line, int minimumFieldValueCount)
		{
			CsvLine = new OCsvLine(line);
			this.MinimumFieldValueCount = minimumFieldValueCount;
		}

		public readonly int MinimumFieldValueCount;

		public abstract string DisplayIdentifier
		{ get; }

		public virtual string ToCsvLineString()
		{
			return CsvLine.ToString();
		}

		public virtual bool SupportsUpdateBusinessData()
		{
			return true;
		}

		/// <summary>
		/// Create a new or update an existing business object with the values in this record.
		/// </summary>
		public virtual void UpdateBusinessData(BusinessObjectFactoryProvider factoryProvider, INotifications notify)
		{
			if (MinimumFieldValueCount != -1 &&
				CsvLine.FieldValues.Length < MinimumFieldValueCount)
			{
				notify.Notify(new ErrorNotification(WowErrorType.NotEnoughColumns, " in record '" + ToCsvLineString() + "'"));
			}
			OnUpdateBusinessData(factoryProvider, notify);
		}

		/// <summary>
		/// Create a new or update an existing business object with the values in this record.
		/// </summary>
		protected abstract void OnUpdateBusinessData(BusinessObjectFactoryProvider factoryProvider, INotifications notify);

		/// <summary>
		/// Get the business object type and column name/natural key values
		/// </summary>
		public virtual NKColumnValuePair[] GetBizTypeAndNKsForBatchDownload()
		{
			return System.Array.Empty<NKColumnValuePair>();
		}

		#region Implementation

		protected readonly OCsvLine CsvLine;

		/// <summary>
		/// The raw CSV field values.
		/// </summary>
		internal string[] FieldValues
		{
			get { return CsvLine.FieldValues; }
		}

		#endregion
	}
}
