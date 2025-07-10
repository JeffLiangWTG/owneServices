using System.Collections.Generic;
using CargoWise.Types;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.FSH.TsManifest.Lines
{
	public class ContainerFieldLine : BaseLine
	{
		public ContainerFieldLine(OceanBillLine oceanBill, FortuneShippingDataRow row) : base(row)
		{
			this.OceanBill = oceanBill;
		}

		#region Properties

		public ZInt CargoSequenceNumber
		{
			get { return ZInt.ParseSafe(Row[Constants.ContainerFieldsRecord.CargoSequenceNo], 0); }
		}

		public ZString ContainerNumber
		{
			get { return Row[Constants.ContainerFieldsRecord.CtnNo]; }
		}

		public Xsd.ContainerMode ContainerMode
		{
			get { return Row[Constants.ContainerFieldsRecord.CtnStatus] == "F" ? Xsd.ContainerMode.FCL : Xsd.ContainerMode.LCL; }
		}

		public ZString SealNumber
		{
			get { return Row[Constants.ContainerFieldsRecord.SealNo1]; }
		}

		public ZString ContainerSizeOrISOCode
		{
			get { return Row[Constants.ContainerFieldsRecord.CtnSizeAndType]; }
		}

		public ZString NumberOfPackages
		{
			get { return Row[Constants.ContainerFieldsRecord.CtnNumbersOfPackages]; }
		}

		public ZDecimal NetWeight
		{
			get { return ZDecimal.ParseSafe(Row[Constants.ContainerFieldsRecord.NetWeight], 0); }
		}

		public ZDecimal Volume
		{
			get { return ZDecimal.ParseSafe(Row[Constants.ContainerFieldsRecord.CtnCargoMeasurement], 0); }
		}

		public ZString ShipperOwned
		{
			get
			{
				ZString result = Row[Constants.ContainerFieldsRecord.ShipperOwnedUnit];
				return result.IsEmpty ? new ZString("N") : result;
			}
		}

		#endregion

		#region RelatedLines

		public void AttachCargoField(CargoFieldLine cargoField)
		{
			if (!cargoFields.Contains(cargoField))
			{
				cargoFields.Add(cargoField);
			}
		}

		public IReadOnlyList<CargoFieldLine> CargoFields => cargoFields;

		protected List<CargoFieldLine> cargoFields = new List<CargoFieldLine>();

		#endregion

		public readonly OceanBillLine OceanBill;
	}
}
