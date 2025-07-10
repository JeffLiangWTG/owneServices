using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRAqisProducerCollection : BusinessObjectCollection<CMRAqisProducer>
	{
		public CMRAqisProducerCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public override void Load()
		{
			base.Load();
			Sort(CMRAqisProducerSchema.Constants.QR_AQISProducerName, ListSortDirection.Ascending);
		}
	}
}
