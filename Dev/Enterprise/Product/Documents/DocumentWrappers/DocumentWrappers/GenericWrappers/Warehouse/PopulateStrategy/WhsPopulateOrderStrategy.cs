using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WhsPopulateOrderStrategy : WhsPopulatePickableDocketStrategy
	{
		#region Constructor

		public WhsPopulateOrderStrategy(BusinessObject docket)
			: base(docket)
		{
		}

		#endregion

		#region Properties

		#region ConsigneeAddress

		public override AddressWrapper ConsigneeAddress
		{
			get
			{
				if (consigneeAddress == null)
				{
					consigneeAddress = new AddressWrapper(OrderBO.ConsigneeDocAddress, Factory);
				}
				return consigneeAddress;
			}
		}
		AddressWrapper consigneeAddress;

		#endregion

		#region Destination

		public override PlaceAndDateWrapper Destination
		{
			get
			{
				if (destination == null)
				{
					JobDocAddress consigneeDocAddress = ConsigneeAddress.WrappedObject as JobDocAddress;
					if (consigneeDocAddress != null && !consigneeDocAddress.E2_AddressOverride && consigneeDocAddress.Address != null)
					{
						destination = new PlaceAndDateWrapper(consigneeDocAddress.Address.OA_RL_NKRelatedPortCode, ZDateTime.Empty, ZDateTime.Empty, Factory);
					}
				}
				return destination;
			}
		}
		PlaceAndDateWrapper destination;

		#endregion

		#region SecondaryReference

		public override LabelValuePairWrapper SecondaryReference
		{
			get
			{
				return new LabelValuePairWrapper(Res.GetString("c6d5fdaa-de2c-479c-8ff8-b8480a89b3f5", "Order Number"), OrderBO.WD_ExternalReference, Factory);
			}
		}

		#endregion

		#region TotalNumberOfPackageLabels

		public override ZInt TotalNumberOfPackageLabels
		{
			get
			{
				return DocketLabel != null ? DocketLabel.NumberOfPackageLabels : ZInt.Zero;
			}
		}

		#endregion

		#region TotalNumberOfLabels

		public override ZInt TotalNumberOfLabels
		{
			get
			{
				return DocketLabel != null ? DocketLabel.NumberOfLabels : ZInt.Zero;
			}
		}

		#endregion

		#endregion

		#region Implementation

		WhsOrder OrderBO
		{
			get { return orderBO ?? (orderBO = (WhsOrder)WrappedBO); }
		}
		WhsOrder orderBO;

		#endregion
	}
}
