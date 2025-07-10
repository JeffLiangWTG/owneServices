using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAPivotSeaCargoReportLine : ISeaCargoReportLine
	{
		public CusSCAPivotSeaCargoReportLine(CusSCAPivot pivot)
		{
			this.pivot = pivot;
			container = pivot.Container;
		}

		public ZString ContainerMode
		{
			get
			{
				return container.CN_ContainerMode;
			}
		}

		public ZString ContainerNumber
		{
			get
			{
				if (container != null && (container.IsBulk || container.IsBreakBulk))
				{
					return ZString.Empty;
				}

				return container.CN_ContainerNumber;
			}
		}

		public ZString SealNumber
		{
			get
			{
				return container.CN_SealNumber;
			}
		}

		public ZString MarksAndNumbers
		{
			get
			{
				return pivot.CV_MarksAndNumbers;
			}
		}

		public bool ShippingOwnedContainerIndicator
		{
			get
			{
				return container.CN_ShipperOwnedContainer;
			}
		}

		public bool FumigationCertificateIndicator
		{
			get
			{
				return pivot.CV_FumigationCert;
			}
		}

		public bool HazardousGoodsIndicator
		{
			get
			{
				return pivot.CV_HazardousGoods;
			}
		}

		public bool DocumentsIndicator
		{
			get
			{
				return pivot.CV_IsDocuments;
			}
		}

		public bool SACIndication
		{
			get
			{
				return pivot.CV_IsSAC;
			}
		}

		public bool PerishableGoodsIndicator
		{
			get
			{
				return pivot.CV_PerishableGoods;
			}
		}

		public bool TimberIndicator
		{
			get
			{
				return pivot.CV_Timber;
			}
		}

		public bool PersonalEffectsIndicator
		{
			get
			{
				return pivot.CV_PersonalEffects;
			}
		}

		public ZDecimal Volume
		{
			get
			{
				return pivot.CV_Volume;
			}
		}

		public ZDecimal Weight
		{
			get
			{
				if (pivot.CV_WeightUQ == Core.Constants.Weight.Kilotonnes)
				{
					return Core.Constants.Weight.ConvertSafe(pivot.CV_Weight, Core.Constants.Weight.Kilotonnes, Core.Constants.Weight.Tonnes);
				}
				return pivot.CV_Weight;
			}
		}

		public ZDecimal NetWeight
		{
			get
			{
				if (pivot.CV_WeightUQ == Core.Constants.Weight.Kilotonnes)
				{
					return Core.Constants.Weight.ConvertSafe(pivot.CV_NetWeight, Core.Constants.Weight.Kilotonnes, Core.Constants.Weight.Tonnes);
				}
				return pivot.CV_NetWeight;
			}
		}

		public ZString WeightUQ
		{
			get
			{
				if (pivot.CV_WeightUQ == Core.Constants.Weight.Kilotonnes)
				{
					return Core.Constants.Weight.Tonnes;
				}
				return pivot.CV_WeightUQ;
			}
		}

		public ZInt PackageCount
		{
			get
			{
				return pivot.CV_PackageCount;
			}
		}

		public ZString PackageType
		{
			get
			{
				return pivot.CV_PackageType;
			}
		}

		public ZString GoodsDescription
		{
			get
			{
				return pivot.CV_GoodsDescription;
			}
		}

		public ZString ContainerType
		{
			get
			{
				return container.CN_TypeOfContainer;
			}
		}

		public ZString ContainerSize
		{
			get
			{
				return container.CN_ContainerSizeOrISOCode;
			}
		}

		public ZString ConsignorVendor => pivot.HouseBill.CA_VendorIdentifier;

		#region Implementation

		readonly CusSCAPivot pivot;
		readonly CusSCAContainer container;

		#endregion
	}
}
