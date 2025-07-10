using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.DocumentWrappers.Customs.EU.TemporaryStorage;

public class TemporaryStoragePackedItemWrapper : DocBaseWrapper
{
	public static TemporaryStoragePackedItemWrapper New(TemporaryStoragePackedItem packedItem, BusinessObjectFactory factoryToWrap) => new TemporaryStoragePackedItemWrapper(packedItem, factoryToWrap);

	public TemporaryStoragePackedItemWrapper(object objectToWrap, BusinessObjectFactory factory) : base(objectToWrap, factory)
	{
	}

	new TemporaryStoragePackedItem ParentBusinessObject => (TemporaryStoragePackedItem)base.ParentBusinessObject;

	public ZString LineNo => ParentBusinessObject.API_LineNo.ToString();
	public ZString Tariff => ParentBusinessObject.API_FormattedTariff;
	public ZString GoodsDescription => ParentBusinessObject.API_GoodsDescription;
	public ZString GrossWeight => ParentBusinessObject.API_GrossWeight.ToString();
	public ZString GrossWeightUQ => ParentBusinessObject.API_GrossWeightUQ;
	public ZString NetWeight => ParentBusinessObject.API_NetWeight.ToString();
	public ZString NetWeightUQ => ParentBusinessObject.API_NetWeightUQ;
	public ZString Missing => ParentBusinessObject.API_PackStatus;
	public ZString Origin => ParentBusinessObject.API_RN_NKGoodsOrigin;
	public ZString MonetaryValue => ParentBusinessObject.API_GoodsValue.ToString();
	public ZString Currency => ParentBusinessObject.API_RX_NKGoodsValueCurrency;

	public DocBaseWrapperCollection<TemporaryStorageLinkPackageWrapper> Packs => packs ??= GetPackCollection();
	DocBaseWrapperCollection<TemporaryStorageLinkPackageWrapper> packs;

	protected virtual DocBaseWrapperCollection<TemporaryStorageLinkPackageWrapper> GetPackCollection()
	{
		var linkPackages = ParentBusinessObject.TemporaryStorageLinkPackages.Where(lp => lp.Package.PackedItems.Cast<AsycudaPackPackedItemPivot>().Any(ip => ip.APP_API_Item == ParentBusinessObject.PK));
		return new TemporaryStorageLinkPackageWrapperCollection(linkPackages, Factory);
	}
}
