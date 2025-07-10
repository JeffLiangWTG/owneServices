using System.Collections.Generic;
using System.Text;
using CargoWise.Types;

namespace Enterprise.Client.FSH.TsManifest.Lines
{
	public class CargoFieldLine : BaseLine
	{
		public CargoFieldLine(OceanBillLine oceanBill, FortuneShippingDataRow row) : base(row)
		{
			this.OceanBill = oceanBill;
		}

		#region Properties

		public ZInt CargoSequenceNumber
		{
			get { return ZInt.ParseSafe(Row[Constants.CargoFields.CargoSequenceNo], 0); }
		}

		public ZDecimal NumberOfPackages
		{
			get { return ZDecimal.ParseSafe(Row[Constants.CargoFields.NumberOfPackages], 0); }
		}

		public ZString CargoType
		{
			get { return Row[Constants.CargoFields.CargoType]; }
		}

		public ZDecimal GrossWeight
		{
			get { return ZDecimal.ParseSafe(Row[Constants.CargoFields.CargoGrossWeight], 0); }
		}

		public ZDecimal NetWeight
		{
			get { return ZDecimal.ParseSafe(Row[Constants.CargoFields.CargoNetWeight], 0); }
		}

		public ZDecimal GrossCube
		{
			get { return ZDecimal.ParseSafe(Row[Constants.CargoFields.CargoGrossCube], 0); }
		}

		#endregion

		#region Related Lines

		#region GoodsDescription

		public ZString CompleteGoodsDescription
		{
			get { return GetCompleteDescription(GoodsDescriptions); }
		}

		public CargoDescriptionLine AddNewGoodsDescription(FortuneShippingDataRow row)
		{
			CargoDescriptionLine result = new CargoDescriptionLine(this, row);
			goodsDescriptions.Add(result);
			return result;
		}

		public IReadOnlyList<CargoDescriptionLine> GoodsDescriptions => goodsDescriptions;

		protected List<CargoDescriptionLine> goodsDescriptions = new List<CargoDescriptionLine>();

		#endregion

		#region MarksAndNumbers

		public ZString CompleteMarksAndNumbers
		{
			get { return GetCompleteDescription(MarksAndNumbers); }
		}

		public CargoDescriptionLine AddNewMarksAndNumbers(FortuneShippingDataRow row)
		{
			CargoDescriptionLine result = new CargoDescriptionLine(this, row);
			marksAndNumbers.Add(result);
			return result;
		}

		public IReadOnlyList<CargoDescriptionLine> MarksAndNumbers => marksAndNumbers;

		protected List<CargoDescriptionLine> marksAndNumbers = new List<CargoDescriptionLine>();

		#endregion

		#region Hazardous

		public HazardousLine AddNewHazardous(FortuneShippingDataRow row)
		{
			fHazardous = new HazardousLine(this, row);
			return fHazardous;
		}

		public HazardousLine Hazardous
		{
			get { return fHazardous; }
		}
		protected HazardousLine fHazardous;

		#endregion

		#region ContainerField

		public void AttachContainerField(ContainerFieldLine containerField)
		{
			if (!containerFields.Contains(containerField))
			{
				containerFields.Add(containerField);
			}
		}

		public IReadOnlyList<ContainerFieldLine> ContainerFields => containerFields;

		protected List<ContainerFieldLine> containerFields = new List<ContainerFieldLine>();

		#endregion

		#endregion

		#region Implementation

		protected ZString GetCompleteDescription(IReadOnlyList<CargoDescriptionLine> descriptionLines)
		{
			StringBuilder result = new StringBuilder(); // Perhaps we can do Lines.Length * 2000 ?
			bool firstLine = true;
			foreach (CargoDescriptionLine line in descriptionLines)
			{
				if (!firstLine)
				{
					result.Append("\n");
				}
				else
				{
					firstLine = false;
				}

				result.Append(line.Description);
			}
			return result.ToString();
		}

		#endregion

		protected readonly OceanBillLine OceanBill;
	}
}
