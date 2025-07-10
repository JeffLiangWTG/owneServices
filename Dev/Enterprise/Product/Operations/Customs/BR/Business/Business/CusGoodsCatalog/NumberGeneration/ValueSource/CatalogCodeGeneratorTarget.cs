using CargoWise.Types;
using Enterprise.Customs.BR.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.BR.Business
{
	public class CatalogCodeGeneratorTarget : NumberGeneratorTarget
	{
		protected override BillOfLadingNumberCustomisation GetNumberCustomisationCore() => Context.AccessRegistry(BRCustomsDataRegistry.Instance.CatalogCodeCustomization);
		protected override int GetMaxLengthCore() => CusGoodsCatalog.Schema.CGC_CatalogCodeMaxLength;
		protected override ZString GetNameCore() => Res.GetString("6DA30103-09C8-4273-8E92-FE6D3F666BF0", "Catalog Code");
		public override string NumberCustomisationLocation => ((IRegistryItemInternals)BRCustomsDataRegistry.Instance.CatalogCodeCustomization).Location;
	}
}
