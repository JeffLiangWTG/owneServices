
using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.JXC.Export
{
	public class SHMKLine : MessageLine
	{
		public SHMKLine(JASForwardingShipment shipment)
		{
			this.Shipment = shipment;
		}

		protected override ZString LineType
		{
			get { return JXCConstants.LineTypes.SHMK; }
		}

		protected override int FieldCount
		{
			get { return JXCConstants.SHMKFieldCount; }
		}

		protected override void SetFieldValues(JXCFlatFileDataRow dataRow)
		{
			ZString marksAndNumbers = (Shipment.JS_MarksAndNumbers.IsEmpty) ? (ZString)JXCConstants.NotAvailable : Shipment.JS_MarksAndNumbers;
			dataRow.SetField(JXCConstants.SHMKFieldPositions.ShippingMarksAndNumbers, marksAndNumbers.Left(JXCConstants.SHMKFieldBoundaries.ShippingMarksAndNumbersMaxLength));
			dataRow.SetField(JXCConstants.SHMKFieldPositions.FreeTextDescription, marksAndNumbers.Left(JXCConstants.SHMKFieldBoundaries.FreeTextDescriptionMaxLength));
			dataRow.SetField(JXCConstants.SHMKFieldPositions.NumberOfCartons, Shipment.JS_OuterPacks.ToString());
		}

		public readonly JASForwardingShipment Shipment;
	}
}
