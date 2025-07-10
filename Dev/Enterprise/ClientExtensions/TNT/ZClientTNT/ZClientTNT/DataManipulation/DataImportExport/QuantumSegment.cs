using System.Collections.Specialized;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.TNT
{
	public class QuantumSegment
	{
		internal QuantumSegment(QuantumConsolRecord consol, QuantumShipmentRecord[] shipments, QuantumShipmentNotesRecord[] shipmentNotes)
		{
			Argument.NotNull(consol, "Consol");
			Argument.NotNull(shipments, "Shipments");
			Argument.NotNull(shipmentNotes, "ShipmentNotes");

			this.Consol = consol;
			this.Shipments = shipments;
			this.ShipmentsNotes = shipmentNotes;
		}

		#region Static Factory Methods

		public static QuantumSegment FromData(ZString branchCode, string[] fileLines, int startLine, out int endLine, INotifications notify)
		{
			QuantumRecordFactory recordFactory = new QuantumRecordFactory();
			return FromData(recordFactory, branchCode, fileLines, startLine, out endLine, notify);
		}

		public static QuantumSegment FromData(QuantumRecordFactory recordFactory, ZString branchCode, string[] fileLines, int startLine, out int endLine, INotifications notify)
		{
			endLine = -1;
			QuantumSegment result = null;

			ListDictionary shipmentRecordList = new ListDictionary();
			ListDictionary shipmentNotesRecordList = new ListDictionary();

			int i = startLine;

			IQDownBaseRecord firstRecord = recordFactory.NewRecord(branchCode, fileLines[i]);
			QuantumConsolRecord consol = firstRecord as QuantumConsolRecord;

			if (consol == null)
			{
				notify.Notify(new ErrorNotification(ErrorType.MissingMasterRecord, ""));
			}
			else
			{
				while (endLine == -1 && i < fileLines.Length - 1)
				{
					i++;
					string nextLine = fileLines[i];
					IQDownBaseRecord record = recordFactory.NewRecord(branchCode, nextLine);
					if (record != null)
					{
						if (record is QuantumConsolRecord)
						{
							endLine = i;
						}
						else if (record is QuantumShipmentRecord || record.GetType().IsSubclassOf(typeof(QuantumShipmentRecord)))
						{
							AddShipmentRecordToList(shipmentRecordList, (QuantumShipmentRecord)record);
						}
						else if (record is QuantumShipmentNotesRecord)
						{
							AddShipmentNotesRecordToList(shipmentNotesRecordList, (QuantumShipmentNotesRecord)record);
						}
					}
				}
				if (endLine == -1)
				{
					endLine = fileLines.Length;
				}

				result = new QuantumSegment(
					consol,
					ShipmentRecordArrayFromList(shipmentRecordList),
					ShipmentNotesRecordArrayFromList(shipmentNotesRecordList));
			}

			return result;
		}

		public static QuantumSegment FromMergingSegments(QuantumSegment segment1, QuantumSegment segment2)
		{
			// Aggregating Shipments
			ListDictionary shipmentRecordList = new ListDictionary();
			for (int i = 0; i < segment1.Shipments.Length; i++)
			{
				AddShipmentRecordToList(shipmentRecordList, segment1.Shipments[i]);
			}
			for (int i = 0; i < segment2.Shipments.Length; i++)
			{
				AddShipmentRecordToList(shipmentRecordList, segment2.Shipments[i]);
			}

			// Aggregating Shipment Notes
			ListDictionary shipmentNotesRecordList = new ListDictionary();
			for (int i = 0; i < segment1.ShipmentsNotes.Length; i++)
			{
				AddShipmentNotesRecordToList(shipmentNotesRecordList, segment1.ShipmentsNotes[i]);
			}
			for (int i = 0; i < segment2.ShipmentsNotes.Length; i++)
			{
				AddShipmentNotesRecordToList(shipmentNotesRecordList, segment2.ShipmentsNotes[i]);
			}

			QuantumSegment result = new QuantumSegment(
				segment1.Consol,
				ShipmentRecordArrayFromList(shipmentRecordList),
				ShipmentNotesRecordArrayFromList(shipmentNotesRecordList));

			return result;
		}

		static void AddShipmentRecordToList(ListDictionary shipmentRecordList, QuantumShipmentRecord shipmentRecord)
		{
			if (shipmentRecordList.Contains(shipmentRecord.HouseBill))
			{
				QuantumShipmentRecord existingShipmentRecord = (QuantumShipmentRecord)shipmentRecordList[shipmentRecord.HouseBill];
				existingShipmentRecord.Merge(shipmentRecord);
			}
			else
			{
				shipmentRecordList.Add(shipmentRecord.HouseBill, shipmentRecord);
			}
		}

		static void AddShipmentNotesRecordToList(ListDictionary shipmentNotesRecordList, QuantumShipmentNotesRecord shipmentNotesRecord)
		{
			if (!shipmentNotesRecordList.Contains(shipmentNotesRecord.HouseBill))
			{
				shipmentNotesRecordList.Add(shipmentNotesRecord.HouseBill, shipmentNotesRecord);
			}
		}

		static QuantumShipmentRecord[] ShipmentRecordArrayFromList(ListDictionary shipmentRecordList)
		{
			QuantumShipmentRecord[] result = new QuantumShipmentRecord[shipmentRecordList.Count];
			shipmentRecordList.Values.CopyTo(result, 0);

			return result;
		}

		static QuantumShipmentNotesRecord[] ShipmentNotesRecordArrayFromList(ListDictionary shipmentNotesRecordList)
		{
			QuantumShipmentNotesRecord[] result = new QuantumShipmentNotesRecord[shipmentNotesRecordList.Count];
			shipmentNotesRecordList.Values.CopyTo(result, 0);

			return result;
		}

		#endregion

		public readonly QuantumConsolRecord Consol;
		internal readonly QuantumShipmentRecord[] Shipments;
		public readonly QuantumShipmentNotesRecord[] ShipmentsNotes;
	}
}
