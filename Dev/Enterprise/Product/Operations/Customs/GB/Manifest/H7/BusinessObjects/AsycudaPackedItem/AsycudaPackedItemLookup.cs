using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.GB.H7.Business
{
	public class AsycudaPackedItemLookups : EU.H7.Business.AsycudaPackedItemLookups
	{
		public AsycudaPackedItemLookups(ASYCUDA.Business.AsycudaPackedItem parent) : base(parent)
		{
		}

		public new TariffViewCollection TariffList
		{
			get
			{
				ZString dataGrouping = Parent.Header != null
					? Parent.Header.PortOfDischarge?.IsInNorthernIreland == true
						? Constants.DataGrouping.EuropeanUnion
						: Parent.Header.ApplicationBusinessProvider.PackedItemTariffDataGrouping
					: Constants.DataGrouping.EuropeanUnion;
				var effectiveDate = Parent.EffectiveDateForDutyRate;
				var tariffType = Parent.Header?.ApplicationBusinessProvider.PackedItemTariffType ?? UniversalReferenceConstants.CusTariffTypes.ImportTariff;

				return Factory.GetCachedValue("ManifestPackageItemTariff" + dataGrouping + effectiveDate + tariffType, delegate
				{
					var collection = new TariffViewCollection(Factory, dataGrouping, tariffType, effectiveDate);
					return collection;
				});
			}
		}
	}
}
