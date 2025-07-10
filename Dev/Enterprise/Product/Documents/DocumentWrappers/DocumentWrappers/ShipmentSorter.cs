using System.Collections;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentWrappers
{
	/// <summary>
	/// Comparer class for Shipment collection, put all custom sort classes functions here.
	/// </summary>
	public class ShipmentHBLSorter : IComparer
	{
		#region IComparer Members

		public int Compare(object x, object y)
		{
			int result = 0;
			DocShipment shipmentX = x as DocShipment;
			DocShipment shipmentY = y as DocShipment;

			if (shipmentX != null && shipmentY != null)
			{
				if (shipmentX.HouseBill.IsEmpty && !shipmentY.HouseBill.IsEmpty)
				{
					return -1;
				}
				else if (!shipmentX.HouseBill.IsEmpty && shipmentY.HouseBill.IsEmpty)
				{
					return 1;
				}
				else if (ZInt.CanParse(shipmentX.HouseBill) || ZInt.CanParse(shipmentY.HouseBill))
				{
					ZInt shipmentXHBLNumber;
					ZInt shipmentYHBLNumber;

					ZInt.TryParse(shipmentX.HouseBill, out shipmentXHBLNumber);
					ZInt.TryParse(shipmentY.HouseBill, out shipmentYHBLNumber);

					return shipmentXHBLNumber.CompareTo(shipmentYHBLNumber);
				}
				else
				{
					result = shipmentX.HouseBill.ToUpper().CompareTo(shipmentY.HouseBill.ToUpper());
					if (result == 0)
					{
						result = shipmentX.ShipmentNumber.ToUpper().CompareTo(shipmentY.ShipmentNumber.ToUpper());
					}
				}
			}

			return result;
		}
		#endregion
	}

	public class InterimReceiptComparer : IComparer
	{
		public int Compare(object x, object y)
		{
			DocShipment shipmentA = (DocShipment)x;
			DocShipment shipmentB = (DocShipment)y;
			int result = shipmentA.InterimReceipt.CompareTo(shipmentB.InterimReceipt);
			if (result == 0)
			{
				result = shipmentA.ShipmentNumber.CompareTo(shipmentB.ShipmentNumber);
			}
			return result;
		}
	}

	/// <summary>
	/// Compare StmNotes collection based on ST_Description.
	/// MarksAndNumbers comes first, then DetailedDescriptionOfGoods,
	/// then everything else in ascending order.
	/// </summary>
	public class NoteSorter : IComparer
	{
		#region IComparer Members

		public int Compare(object x, object y)
		{
			int result = 0;
			StmNote noteA = x as StmNote;
			StmNote noteB = y as StmNote;

			if (noteA != null && noteB != null)
			{
				result = noteA.ST_Description.CompareTo(noteB.ST_Description);
				if (result != 0)
				{
					if (IsMarksAndNumbers(noteA))
					{
						return -1;
					}

					if (IsMarksAndNumbers(noteB))
					{
						return 1;
					}

					if (IsDetailedGoodsDescription(noteA))
					{
						return -1;
					}

					if (IsDetailedGoodsDescription(noteB))
					{
						return 1;
					}
				}
			}
			return result;
		}

		protected ZBool IsMarksAndNumbers(StmNote note)
		{
			return (note.ST_Description == PredefinedNoteTypes.Instance.MarksAndNumbers.Description);
		}

		protected ZBool IsDetailedGoodsDescription(StmNote note)
		{
			return (note.ST_Description == PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description);
		}

		#endregion
	}
}
