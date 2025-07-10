using CargoWise.Common;
using CargoWise.Customs.JP.MessageDefinitions;
using Enterprise.Customs.JP.Common;

namespace Enterprise.Customs.JP.Manifest.Business
{
	sealed class MeasurementProvider : IMeasurement
	{
		public MeasurementProvider(AsycudaBill bill, string type)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
			this.type = Argument.NotNull(type, nameof(type));
		}

		readonly AsycudaBill bill;
		readonly string type;

		public decimal Quantity
		{
			get
			{
				switch (type)
				{
					case nameof(BillProvider.TotalWeight):
						return bill.CustomsWeight;
					case nameof(BillProvider.Quantity):
						return bill.ABL_ManifestQty;
					case nameof(BillProvider.NetWeight):
						return bill.CustomsNetWeight;
					case nameof(BillProvider.Volume):
						return bill.CustomsVolume;
					default:
						return 0;
				}
			}
		}

		public string Unit
		{
			get
			{
				if (bill.IsDummySendingObjectForHCH01End)
				{
					return string.Empty;
				}

				switch (type)
				{
					case nameof(BillProvider.TotalWeight):
						return bill.ABL_GrossWeightUQ == Core.Constants.Weight.Pounds ? CustomsUnitOfMeasureList.CodeLBR : CustomsUnitOfMeasureList.CodeKGM;
					case nameof(BillProvider.Quantity):
						return bill.ABL_ManifestUQ;
					case nameof(BillProvider.NetWeight):
						return bill.CustomsNetWeightUQ;
					case nameof(BillProvider.Volume):
						return bill.CustomsVolumeUQ;
					default:
						return string.Empty;
				}
			}
		}
	}
}
