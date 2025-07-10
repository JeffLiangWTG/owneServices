using CargoWise.Types;
using Enterprise.DocumentWrappers;
using Enterprise.Environment;

namespace Enterprise.Client.MFI.DocWrappers
{
	public class DocMFIBillofLading : DocBillOfLading
	{
		#region Constructor & Type Overrides

		public DocMFIBillofLading(DocForwardingShipment shipmentWrapper)
			: base(shipmentWrapper)
		{
		}

		#endregion

		#region Client specific Conditionals

		protected ZBool IsMFIBillOfLading
		{
			get
			{
				ZBool result = ZBool.False;

				if (ShipmentWrapper.HouseBillOfLadingType == MFIConstants.BillofLading.BillType.MNZ)
				{
					result = ZBool.True;
				}

				return result;
			}
		}

		#endregion

		#region Overrides

		protected override ZString ContainersColumnHeadersCore
		{
			get
			{
				if (fContainersColumnHeaders.IsEmpty)
				{
					if (IsMFIBillOfLading)
					{
						ZStringBuilder values = new ZStringBuilder();
						values.Append("\n");
						values.Append("Container" + FillWithSpaces(ShipmentWrapper.ContainerNumberWidth - "Container".Length));
						values.Append(FillWithSpaces(ShipmentWrapper.ContainerNumberAndSealGap));
						values.Append("Seal" + FillWithSpaces(ShipmentWrapper.ContainerSealWidth - "Seal".Length));
						values.Append(FillWithSpaces(ShipmentWrapper.ContainerSealAndTypeGap));
						values.Append("Type" + FillWithSpaces(ShipmentWrapper.ContainerTypeWidth - "Type".Length));
						values.Append(FillWithSpaces(ShipmentWrapper.ContainerTypeAndWeightGap));
						values.Append("Weight(KG)" + FillWithSpaces(ShipmentWrapper.ContainerWeightWidth - "Weight(KG)".Length));
						values.Append(FillWithSpaces(ShipmentWrapper.ContainerWeightAndVolumeGap));
						values.Append("Volume(M3)" + FillWithSpaces(ShipmentWrapper.ContainerVolumeWidth - "Volume(M3)".Length));
						values.Append(FillWithSpaces(ShipmentWrapper.ContainerVolumeAndPackagesGap));
						values.Append("Packages" + FillWithSpaces(ShipmentWrapper.ContainerPackagesWidth - "Packages".Length));
						values.Append(FillWithSpaces(ShipmentWrapper.ContainerPackagesAndModeGap));
						values.Append("Mode" + FillWithSpaces(ShipmentWrapper.ContainerModeWidth - "Mode".Length));
						fContainersColumnHeaders = values.ToString();
					}
					else
					{
						fContainersColumnHeaders = base.ContainersColumnHeadersCore;
					}
				}
				return fContainersColumnHeaders;
			}
		}

		protected override ZString GetMainContainerLine(DocBillofLadingContainer currentContainer)
		{
			ZString line = "";
			DocMFIBillofLadingContainer mfiContainer = (DocMFIBillofLadingContainer)currentContainer;

			if (IsMFIBillOfLading)
			{
				line = AlignToWidth(mfiContainer.ContainerNumber, ShipmentWrapper.ContainerNumberWidth)
					+ FillWithSpaces(ShipmentWrapper.ContainerNumberAndSealGap)
					+ AlignToWidth(mfiContainer.ContainerSeal, ShipmentWrapper.ContainerSealWidth)
					+ FillWithSpaces(ShipmentWrapper.ContainerSealAndTypeGap)
					+ AlignToWidth(mfiContainer.ContainerType, ShipmentWrapper.ContainerTypeWidth)
					+ FillWithSpaces(ShipmentWrapper.ContainerTypeAndWeightGap)
					+ AlignToWidth(ShipmentWrapper.FormatNumber(mfiContainer.Weight, Env.Registry.WeightMinimumDecimalPlacesToDisplay), ShipmentWrapper.ContainerWeightWidth)
					+ FillWithSpaces(ShipmentWrapper.ContainerWeightAndVolumeGap)
					+ AlignToWidth(ShipmentWrapper.FormatNumber(mfiContainer.Volume, Env.Registry.VolumeMinimumDecimalPlacesToDisplay), ShipmentWrapper.ContainerVolumeWidth)
					+ FillWithSpaces(ShipmentWrapper.ContainerVolumeAndPackagesGap)
					+ AlignToWidth(mfiContainer.Packs.ToString(), ShipmentWrapper.ContainerPackagesWidth)
					+ FillWithSpaces(ShipmentWrapper.ContainerPackagesAndModeGap)
					+ AlignToWidth(mfiContainer.DeliveryMode, ShipmentWrapper.ContainerModeWidth);
			}
			else
			{
				line = base.GetMainContainerLine(currentContainer);
			}
			return line;
		}

		#endregion

	}
}
