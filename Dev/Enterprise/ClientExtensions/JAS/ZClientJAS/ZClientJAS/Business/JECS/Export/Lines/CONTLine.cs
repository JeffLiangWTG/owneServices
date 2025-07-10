using CargoWise.Types;
using Enterprise.Client.JAS.Business.Utilities;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Client.JAS.Business.JXC.Export
{
	public class CONTLine : MessageLine
	{
		public CONTLine(ForwardingContainer container, JASForwardingPackLine packLine)
		{
			this.Container = container;
			this.PackLine = packLine;
		}

		#region Overrides

		protected override int FieldCount
		{
			get { return JXCConstants.CONTFieldCount; }
		}

		protected override ZString LineType
		{
			get { return JXCConstants.LineTypes.CONT; }
		}

		protected override void SetFieldValues(JXCFlatFileDataRow dataRow)
		{
			if (PackLine != null)
			{
				dataRow.SetField(JXCConstants.CONTFieldPositions.ContainerType, JASContainerType);
				if (Container != null)
				{
					dataRow.SetField(JXCConstants.CONTFieldPositions.ContainerNo, Container.JC_ContainerNum);
					dataRow.SetField(JXCConstants.CONTFieldPositions.SealNo, Container.JC_SealNum);
					dataRow.SetField(JXCConstants.CONTFieldPositions.TypeOfService, TypeOfService);
				}
				else
				{
					dataRow.SetField(JXCConstants.CONTFieldPositions.ContainerNo, JXCConstants.NotAvailable);
				}

				dataRow.SetField(JXCConstants.CONTFieldPositions.GrossWeightInKgs, GrossWeightInKgs);
				dataRow.SetField(JXCConstants.CONTFieldPositions.MeasurementInCBM, VolumeInCubicMetres);
				dataRow.SetField(JXCConstants.CONTFieldPositions.NoOfPackages, PackLine.JL_PackageCount);
				dataRow.SetField(JXCConstants.CONTFieldPositions.GoodsDescription, PackLine.JL_Description, JXCConstants.CONTFieldBoundaries.DescriptionOfGoodsMaxLength);
				dataRow.SetField(JXCConstants.CONTFieldPositions.HTSNo, PackLine.JL_HarmonisedCode);
				dataRow.SetField(JXCConstants.CONTFieldPositions.CommodityValue, PackLine.JL_LinePrice);
				dataRow.SetField(JXCConstants.CONTFieldPositions.CommodityCurrency, PackLine.LinePriceCurrency);
			}
		}

		#endregion

		#region Implementation

		ZString JASContainerType
		{
			get
			{
				ZString result = null;

				if (Container != null)
				{
					result = UnitConverter.GetJASContainerType(Container.RefContainer);
				}

				if (result.IsEmpty)
				{
					result = JXCConstants.ContainerTypes.Other;
				}

				return result;
			}
		}

		ZDecimal GrossWeightInKgs
		{
			get
			{
				return (PackLine.JL_ActualWeightUQ != Constants.Weight.Kilograms)
					? (ZDecimal)Constants.Weight.Convert(PackLine.JL_ActualWeight, PackLine.JL_ActualWeightUQ, Constants.Weight.Kilograms)
					: PackLine.JL_ActualWeight;
			}
		}

		ZDecimal VolumeInCubicMetres
		{
			get
			{
				return (PackLine.JL_ActualVolumeUQ != Constants.Volume.CubicMetres)
					? (ZDecimal)Constants.Volume.Convert(PackLine.JL_ActualVolume, PackLine.JL_ActualVolumeUQ, Constants.Volume.CubicMetres)
					: PackLine.JL_ActualVolume;
			}
		}

		ZString TypeOfService
		{
			get { return new JASUnitConverter().GetJASTypeOfServiceFromContainerAndDeliveryMode(Container); }
		}

		JASUnitConverter UnitConverter
		{
			get
			{
				if (fUnitConverter == null)
				{
					fUnitConverter = new JASUnitConverter();
				}
				return fUnitConverter;
			}
		}

		readonly JASForwardingPackLine PackLine;
		readonly ForwardingContainer Container;
		JASUnitConverter fUnitConverter;

		#endregion
	}
}
