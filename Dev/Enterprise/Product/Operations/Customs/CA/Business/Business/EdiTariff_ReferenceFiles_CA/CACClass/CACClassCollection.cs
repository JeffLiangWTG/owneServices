using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Business
{
	[ModuleID(ModuleId.CACClass)]
	public class CACClassCollection : BusinessObjectCollection<CACClass>
	{
		public CACClassCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return new TariffFindBoxListProvider(this); }
		}
	}
}
