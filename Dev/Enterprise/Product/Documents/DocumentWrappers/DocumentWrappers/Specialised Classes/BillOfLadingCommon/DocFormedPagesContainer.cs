using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentWrappers
{
	public class DocFormedPagesContainer : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Constructors

		public DocFormedPagesContainer(IDocContainer docContainer)
		{
			if (docContainer != null)
			{
				ContainerNumber = docContainer.ContainerNumber;
				ContainerCode = docContainer.ContainerCode;

				if (!docContainer.SealNumber.IsEmpty)
				{
					ContainerSeal = docContainer.SealNumber;
				}

				if (!docContainer.SealNumber2.IsEmpty)
				{
					ContainerSeal += ", " + docContainer.SealNumber2;
				}

				if (!docContainer.SealNumber3.IsEmpty)
				{
					ContainerSeal += ", " + docContainer.SealNumber3;
				}

				ContainerSeal = ContainerSeal.TrimStart(',').TrimStart(' ');

				if (ContainerSeal.IsEmpty)
				{
					ContainerSeal = (ZString)"-";
				}

				ContainerType = docContainer.Container == null ? "-" : docContainer.Container.Code.ToString();

				if (docContainer.DeliveryMode.IsEmpty)
				{
					DeliveryMode = "-";
				}
				else
				{
					DeliveryMode = docContainer.DeliveryMode.StartsWith("CY") ? (ZString)(docContainer.DeliveryMode + "*") : docContainer.DeliveryMode;
				}
				ContainerMode = docContainer.ContainerMode;
				ContainerGross = docContainer.GrossWeight;
				ContainerTare = docContainer.TareWeight;
				ContainerNet = ContainerGross - ContainerTare;
				ContainerWeightUQ = docContainer.WeightUQ;

				Packs = docContainer.TotalAllocatedShipmentPackages;
				PackType = docContainer.TotalAllocatedShipmentPackagesPackType;

				Volume = docContainer.TotalVolume;
				VolumeUQ = docContainer.TotalVolumeUnit;

				Temperature = docContainer.IsControlledAtmosphere ? ZString.Format("{0:0.#}{1}", docContainer.SetPointTemp, docContainer.SetPointTempUnit) : ZString.Empty;
				Humidity = docContainer.HumidityPercent.IsEmpty ? ZString.Empty : (ZString)(docContainer.HumidityPercent + '%');
			}
		}

		public DocFormedPagesContainer(DocBillofLadingContainer bolContainer, DocShipment shipmentWrapper)
		{
			if (bolContainer != null)
			{
				ContainerNumber = bolContainer.ContainerNumber;
				ContainerCode = bolContainer.ContainerCode;
				ContainerSeal = bolContainer.ContainerSeal;
				ContainerType = bolContainer.ContainerType;
				DeliveryMode = bolContainer.DeliveryMode;
				ContainerMode = bolContainer.ContainerMode;
				ContainerGross = bolContainer.ContainerGrossAsEntered;
				ContainerTare = bolContainer.ContainerTareAsEntered;

				if (shipmentWrapper.IsMasterCoLoadShipmentWithSubShipments)
				{
					ContainerNet = bolContainer.Weight;
					Volume = bolContainer.Volume;
				}
				else
				{
					ContainerNet = bolContainer.TotalAllocatedShipmentWeight;
					Volume = bolContainer.TotalAllocatedShipmentVolume;
				}

				ContainerWeightUQ = bolContainer.ContainerWeightUQAsEntered;
				VolumeUQ = bolContainer.TotalAllocatedShipmentVolumeUQ;
				Packs = bolContainer.Packs;
				PackType = bolContainer.PackType;
				Temperature = bolContainer.Temperature;
				Humidity = bolContainer.Humidity;
			}
		}

		#endregion

		#region Properties

		public ZString ContainerNumber { get; private set; }

		public ZString ContainerCode { get; private set; }

		public ZString ContainerSeal { get; private set; }

		public ZString ContainerType { get; private set; }

		public ZString DeliveryMode { get; private set; }

		public ZString ContainerMode { get; private set; }

		public ZDecimal ContainerGross { get; private set; }

		public ZDecimal ContainerNet { get; private set; }

		public ZDecimal ContainerTare { get; private set; }

		public ZString ContainerWeightUQ { get; private set; }

		public ZInt Packs { get; private set; }

		public ZString PackType { get; private set; }

		public ZDecimal Volume { get; private set; }

		public ZString VolumeUQ { get; private set; }

		public ZString Temperature { get; private set; }

		public ZString Humidity { get; private set; }

		#endregion
	}
}
