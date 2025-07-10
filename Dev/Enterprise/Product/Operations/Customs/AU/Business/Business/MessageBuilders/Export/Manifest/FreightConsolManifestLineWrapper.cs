using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class FreightConsolManifestLineWrapper : IManifestLineWrapper
	{
		public FreightConsolManifestLineWrapper(FreightConsolManifestHeaderWrapper consolWrapper)
		{
			this.consolWrapper = consolWrapper;
		}

		public int PackCount
		{
			get
			{
				return consolWrapper.TotalPackageCount;
			}
		}

		public int ContainerCount
		{
			get
			{
				return consolWrapper.TotalContainerCount;
			}
		}

		public ZString CAN
		{
			get
			{
				return ZString.Empty;
			}
		}

		public ZString CCAN
		{
			get
			{
				return consolWrapper.CCAN;
			}
		}

		public ZString CountryOfDestination
		{
			get
			{
				return ZString.Empty;
			}
		}

		public ZString GoodsOwner
		{
			get
			{
				return ZString.Empty;
			}
		}

		public ZString AirWaybillNumber
		{
			get
			{
				return consolWrapper.AirWaybillNumber;
			}
		}

		public ZString GoodsOwnerPartyID
		{
			get
			{
				return ZString.Empty;
			}
		}

		public ZString GoodsDescription
		{
			get
			{
				return ZString.Empty;
			}
		}

		public ZString ExemptionCode
		{
			get
			{
				return ZString.Empty;
			}
		}

		public int LineNumber
		{
			get
			{
				return 1;
			}
		}

		public ZString LineActionCode
		{
			get
			{
				return ZString.Empty;
			}
		}

		public ZString Reference
		{
			get
			{
				return ZString.Empty;
			}
		}

		#region Implementation

		protected FreightConsolManifestHeaderWrapper consolWrapper;

		#endregion

		public ZString HouseBillNumber
		{
			get { return string.Empty; }
		}
	}
}
