using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Business
{
	[ModuleID(ModuleId.CACExportTariff)]
	public class CACExportTariffCollection : BusinessObjectCollection<CACExportTariff>
	{
		public CACExportTariffCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return new TariffFindBoxListProvider(this); }
		}
	}
}
