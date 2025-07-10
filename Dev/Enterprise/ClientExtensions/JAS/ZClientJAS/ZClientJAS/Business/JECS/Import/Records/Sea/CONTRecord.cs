
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.JAS.Business.Utilities;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.JXC.Import
{
	public class CONTRecord : JXCRecord
	{
		public CONTRecord(ZString lineType, ZString lineContent)
			: base(lineType, lineContent)
		{
		}

		public void UpdateContainerAndPackLine(JASForwardingConsol consol, JASForwardingShipment shipment)
		{
			ForwardingContainer container = null;
			if (consol != null)
			{
				container = FindContainer(consol);
				if (container == null)
				{
					container = consol.Containers.AddNew();
				}
				PopulateContainerData(container);
			}

			if (shipment != null)
			{
				JASForwardingPackLine packLine = (JASForwardingPackLine)shipment.OuterPackLines.AddNew();
				if (container != null)
				{
					packLine.JL_JC = container.PK;
				}
				PopulatePackLine(packLine);
			}
		}

		#region Implementation

		void PopulateContainerData(ForwardingContainer container)
		{
			container.JC_ContainerNum = CleanedUpContainerNo;
			container.JC_SealNum = SealNo.Left(ForwardingContainer.Schema.JC_SealNumMaxLength);
			container.JC_RC = GetContainerType(container.Factory);
			UnitConverter.AssignJASTypeOfServiceToContainerAndDeliveryMode(TypeOfService, container);
		}

		void PopulatePackLine(JASForwardingPackLine packLine)
		{
			if (GrossWeightInKgs.IsWithinSqlPrecisionAndScale(JobPackLinesSchema.JL_ActualWeight.Precision, JobPackLinesSchema.JL_ActualWeight.Scale))
			{
				packLine.JL_ActualWeight = GrossWeightInKgs;
			}
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			if (VolumeInCubicMetres.IsWithinSqlPrecisionAndScale(JobPackLinesSchema.JL_ActualVolume.Precision, JobPackLinesSchema.JL_ActualVolume.Scale))
			{
				packLine.JL_ActualVolume = VolumeInCubicMetres;
			}
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_PackageCount = NoOfPackages;
			packLine.JL_Description = GoodsDescription.Left(JASForwardingPackLine.Schema.JL_DescriptionMaxLength);
			packLine.JL_HarmonisedCode = HarmonisedCode.Left(JASForwardingPackLine.Schema.JL_HarmonisedCodeMaxLength);
			if (CommodityValue.IsWithinSqlPrecisionAndScale(JobPackLinesSchema.JL_LinePrice.Precision, JobPackLinesSchema.JL_LinePrice.Scale))
			{
				packLine.JL_LinePrice = CommodityValue;
			}
			packLine.LinePriceCurrency = CommodityCurrency.Left(RefCurrency.Schema.RX_CodeMaxLength);
		}

		ZGuid GetContainerType(BusinessObjectFactory factory)
		{
			RefContainer refContainer = UnitConverter.GetRefContainerFromJASContainerType(factory, JASContainerType);
			return (refContainer != null) ? refContainer.PK : ZGuid.Empty;
		}

		ForwardingContainer FindContainer(JASForwardingConsol consol)
		{
			ForwardingContainer result = null;

			if (consol != null)
			{
				ZQuery filter = new ZQuery(JobContainerSchema.JC_ContainerNum, CleanedUpContainerNo);
				filter.AddToFilter(JoinCondition.Or, JobContainerSchema.JC_ContainerNum, SQLComparisonOperator.Equal, ContainerNo);
				ForwardingContainer[] containers = (ForwardingContainer[])consol.Containers.Find(filter);
				result = (containers.Length > 0) ? containers[0] : null;
			}

			return result;
		}

		ZString CleanedUpContainerNo
		{
			get { return ContainerNo.KeepAlphanumericCharacters().Left(ForwardingContainer.Schema.JC_ContainerNumMaxLength); }
		}

		ZString ContainerNo
		{
			get { return Fields.GetFieldValue(JXCConstants.CONTFieldPositions.ContainerNo); }
		}

		ZString SealNo
		{
			get { return Fields.GetFieldValue(JXCConstants.CONTFieldPositions.SealNo); }
		}

		ZString JASContainerType
		{
			get { return Fields.GetFieldValue(JXCConstants.CONTFieldPositions.ContainerType); }
		}

		ZDecimal GrossWeightInKgs
		{
			get { return Fields.GetDecimalFieldValue(JXCConstants.CONTFieldPositions.GrossWeightInKgs); }
		}

		ZDecimal VolumeInCubicMetres
		{
			get { return Fields.GetDecimalFieldValue(JXCConstants.CONTFieldPositions.MeasurementInCBM); }
		}

		ZInt NoOfPackages
		{
			get { return Fields.GetIntFieldValue(JXCConstants.CONTFieldPositions.NoOfPackages); }
		}

		ZString GoodsDescription
		{
			get { return Fields.GetFieldValue(JXCConstants.CONTFieldPositions.GoodsDescription); }
		}

		ZString TypeOfService
		{
			get { return Fields.GetFieldValue(JXCConstants.CONTFieldPositions.TypeOfService); }
		}

		ZString HarmonisedCode
		{
			get { return Fields.GetFieldValue(JXCConstants.CONTFieldPositions.HTSNo); }
		}

		ZDecimal CommodityValue
		{
			get { return Fields.GetDecimalFieldValue(JXCConstants.CONTFieldPositions.CommodityValue); }
		}

		ZString CommodityCurrency
		{
			get { return Fields.GetFieldValue(JXCConstants.CONTFieldPositions.CommodityCurrency); }
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

		JASUnitConverter fUnitConverter;

		#endregion
	}
}
