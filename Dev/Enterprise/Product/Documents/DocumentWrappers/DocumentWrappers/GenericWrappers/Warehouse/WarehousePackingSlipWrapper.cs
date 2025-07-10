using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WarehousePackingSlipWrapper : WarehousePickableDocketWrapper
	{
		public WarehousePackingSlipWrapper(WhsPickableDocket packingDocket, BusinessObjectFactory factory)
			: base(packingDocket, factory)
		{
			OrderWrapper = new Lazy<WarehouseOrderWrapper>(() => packingDocket is WhsOrder order ? new WarehouseOrderWrapper(order, factory) : null);
		}

		readonly Lazy<WarehouseOrderWrapper> OrderWrapper;

		#region Properties 

		#region DocumentTitleCore

		protected override ZString DocumentTitleCore
		{
			get
			{
				var result = WarehouseDataRegistry.Instance.PackingSlipTitles.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToString();
				if (PackingDocketBO != null && PackingDocketBO.Warehouse != null)
				{
					result = PackingDocketBO.Warehouse.PackingSlipTitle;
				}
				return result;
			}
		}

		#endregion

		#region SecondaryReference

		protected override LabelValuePairWrapper SecondaryReferenceCore
		{
			get
			{
				if (DocketBO != null)
				{
					if (IsWorkOrder)
					{
						return new LabelValuePairWrapper(Res.GetString("31f53031-d332-4f51-88f7-249522adb9da", "Work Order No"), DocketBO.WD_ExternalReference, Factory);
					}
					else
					{
						return new LabelValuePairWrapper(Res.GetString("3263b05d-5dbb-4993-a079-e5a854614b8a", "Order Number"), DocketBO.WD_ExternalReference, Factory);
					}
				}
				else
				{
					return LabelValuePairWrapper.Empty;
				}
			}
		}

		#endregion

		#region ABN

		public override ZString ABN => DocketBO.GetClientCode(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber);

		#endregion

		#region SupplierBuyerLink

		public override SupplierBuyerLinkWrapper SupplierBuyerLink => OrderWrapper.Value?.SupplierBuyerLink;

		#endregion

		#region LoadNumber

		protected override ZString LoadNumberCore => GetLoadNumber();

		#endregion

		#endregion

		#region Page Header Info

		#region Barcodes

		public override LabelValuePairWrapper PrimaryBarcode => SecondaryReference;

		#endregion

		#region Generic and Common References

		public override LabelValuePairWrapper CustomerReference
		{
			get
			{
				if (PackingDocketBO != null)
				{
					if (!IsWorkOrder)
					{
						return new LabelValuePairWrapper(Res.GetString("525e32e4-53b4-48fb-90e6-7581363b4b84", "Customer Ref"), DocketBO.WD_CustomerReference, Factory);
					}
					else
					{
						return new LabelValuePairWrapper(Factory);
					}
				}
				else
				{
					return LabelValuePairWrapper.Empty;
				}
			}
		}

		public override LabelValuePairWrapper PickNumberReference
		{
			get
			{
				if (PackingDocketBO != null && !IsWorkOrder && PackingDocketBO.Pick != null)
				{
					return new LabelValuePairWrapper(Res.GetString("859279c7-7ae0-487b-be4a-a51816d8211f", "Pick No"), PackingDocketBO.Pick.WP_PickNo, Factory);
				}
				else
				{
					return LabelValuePairWrapper.Empty;
				}
			}
		}

		#endregion

		#region Status

		public override LabelValuePairWrapper Status => LabelValuePairWrapper.Empty;

		#endregion

		#endregion

		#region Bizo Header  Info

		public override ZBool EnableDangerousGoodsDetails
		{
			get
			{
				ZBool result = ZBool.False;
				if (PackingLines != null)
				{
					foreach (WarehousePackingSlipLineWrapper packingLine in PackingLines)
					{
						if (packingLine.DangerousGoodsSubstance != null)
						{
							result = ZBool.True;
							break;
						}
					}
				}
				return result;
			}
		}

		#endregion

		#region Bizo Body Sections

		protected override ContainerWrapperCollection NewWarehouseContainerLineWrapperCollection()
		{
			return new ContainerWrapperCollection(PackingDocketBO, Factory);
		}

		#region Service Lines

		protected override ServiceWrapperCollection NewServiceLineWrapperCollection()
		{
			return new ServiceWrapperCollection(DocketBO.Services, Factory);
		}

		#endregion

		#endregion
	}
}
