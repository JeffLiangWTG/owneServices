using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Barcode.Business;
using Enterprise.eTail.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class FreightWrapperFromHVLVOuterPackage : FreightWrapper
	{
		public FreightWrapperFromHVLVOuterPackage(HVLVOuterPackage outerPackage, BusinessObjectFactory factory)
			: base(outerPackage, factory)
		{
			Argument.NotNull(factory, "factory");

			OuterPackageBO = outerPackage ?? factory.GetNull<HVLVOuterPackage>();
		}

		readonly HVLVOuterPackage OuterPackageBO;

		protected override TextBarcode DocManagerBarcode => new TextBarcode(OuterPackageBO.HVO_PackageBarcode);

		protected override ZString GetMasterBill()
		{
			if (OuterPackageBO.LoadList != null)
			{
				return OuterPackageBO.LoadList.HVL_MasterBillNumber;
			}
			else
			{
				return ZString.Empty;
			}
		}

		#region Destination Depot

		protected override AddressWrapper GetDeliveryAddress()
		{
			return new AddressWrapper(OuterPackageBO.DestinationDepot, ContactType.All, Factory);
		}

		#endregion

		#region Last Mile Carrier

		protected override OrganisationWrapper GetCarrier()
		{
			return new OrganisationWrapper(OrganisationUsageType.Carrier, OuterPackageBO.LastMileCarrier, ContactType.All, Factory);
		}

		#endregion

		#region Last Mile Carrier Service Level

		protected override CarrierServiceLevelWrapper GetCarrierServiceLevel()
		{
			return new CarrierServiceLevelWrapper(OuterPackageBO.HVO_PL_NKLastMileCarrierServiceLevel, OuterPackageBO.Lookups?.HVO_PL_NKLastMileCarrierServiceLevel_List, Factory);
		}

		#endregion

		protected override PackQTYWrapper GetShipmentOuterPacksQty()
		{
			return new PackQTYWrapper(
				OuterPackageBO.ActiveItems.Count(),
				OuterPackageBO.HVO_F3_NKPackageType,
				BindToLists.GetCachedLists(Factory).OuterPackTypes.GetAsCodeDescriptionPair(),
				Factory);
		}

		protected override WeightWrapper GetWeight()
		{
			var weight = OuterPackageBO.ActiveItems.Sum(item => item.HVI_ActualWeight <= 0 ? item.HVI_ManifestedWeight : item.HVI_ActualWeight);
			var weightUQ = OuterPackageBO.ActiveItems.Any() ? OuterPackageBO.ActiveItems.First().Consignment.HVC_WeightUQ : new ZString(Core.Constants.Weight.Kilograms);
			return new WeightWrapper(
				weight,
				weightUQ,
				HVLVConsignmentSchema.HVC_ActualWeight.Scale,
				Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight),
				Factory);
		}
	}
}
