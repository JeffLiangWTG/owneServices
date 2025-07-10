using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class CusContainer : EU.Business.Declaration.CusContainer, Integration.Customs.DE.ICusContainer
	{
		public CusContainer(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new CusContainerLookups Lookups => (CusContainerLookups)base.Lookups;

		protected override Customs.Business.CusContainerLookups GetNewLookups() => new CusContainerLookups(this);
	}
}
