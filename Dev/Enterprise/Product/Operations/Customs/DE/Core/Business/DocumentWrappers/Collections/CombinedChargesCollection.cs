using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.DE.Business.DocumentWrappers
{
	public class CombinedChargesCollection : DocumentWrapperCollection<CombinedCharges>
	{
		public CombinedChargesCollection(IEnumerable<InvoiceLineCharge> collectionSource, BusinessObjectFactory factoryToWrap)
			: base(GetWrappedObjects(collectionSource), factoryToWrap)
		{
		}

		public CombinedChargesCollection(IEnumerable<InvoiceLineApportionCharge> collectionSource, BusinessObjectFactory factoryToWrap)
			: base(GetWrappedObjects(collectionSource), factoryToWrap)
		{
		}

		static IEnumerable<BaseJobComInvHeaderCharge[]> GetWrappedObjects<T>(IEnumerable<T> charges) where T : BaseJobComInvHeaderCharge
			=> charges.GroupBy(e => e.J7_ChargeType).Select(groupedByType => groupedByType.ToArray<BaseJobComInvHeaderCharge>());
	}
}

