using System.Collections;
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business.BISI
{
	public class BISIUploadRecordList
	{
		public int TotalLineCount
		{
			get
			{
				int result = 0;
				foreach (BISIUploadRecord record in Records)
				{
					result += record.LineCount;
				}
				return result;
			}
		}

		internal virtual bool IsEmpty
		{
			get { return RecordHashtable.Count == 0; }
		}

		public IReadOnlyList<BISIUploadRecord> Records
		{
			get
			{
				BISIUploadRecord[] result = new BISIUploadRecord[RecordHashtable.Values.Count];
				RecordHashtable.Values.CopyTo(result, 0);
				return result;
			}
		}

		public RecordHeaderLine GetHeaderLine(int batchNumber)
		{
			return GetHeaderLine(batchNumber, 0, 0);
		}

		public RecordHeaderLine GetHeaderLine(int batchNumber, int extraRows, int linesExcluded)
		{
			return new RecordHeaderLine(batchNumber, RecordHashtable.Count + extraRows, TotalLineCount + extraRows - linesExcluded);
		}

		public void AddFromShipment(IShipmentData shipmentData)
		{
			BISIUploadRecord record = GetOrCreateBISIUploadRecord(shipmentData.ShipmentRef);
			record.SetShipmentDetailsLine(shipmentData);
		}

		public bool ContainsShipmentDetailRecord(ZString shipmentRef)
		{
			BISIUploadRecord record = GetBISIUploadRecord(shipmentRef);
			return (record != null && record.HasShipmentDetailLine());
		}

		public void AddFromShipmentStatus(IShipmentStatusData statusData)
		{
			BISIUploadRecord record = GetOrCreateBISIUploadRecord(statusData.ShipmentRef);
			record.AddShipmentStatusLine(statusData);
		}

		#region Implementation

		Hashtable RecordHashtable
		{
			get
			{
				if (fRecordHashtable == null)
				{
					fRecordHashtable = new Hashtable();
				}
				return fRecordHashtable;
			}
		}

		BISIUploadRecord GetOrCreateBISIUploadRecord(ZString shipmentRef)
		{
			BISIUploadRecord result = GetBISIUploadRecord(shipmentRef);
			if (result == null)
			{
				result = new BISIUploadRecord();
				RecordHashtable.Add(shipmentRef, result);
			}
			return result;
		}

		BISIUploadRecord GetBISIUploadRecord(ZString shipmentRef)
		{
			return RecordHashtable[shipmentRef] as BISIUploadRecord;
		}

		Hashtable fRecordHashtable;

		#endregion
	}
}
