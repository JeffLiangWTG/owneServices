using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.CN.Business
{
	class BillsSynchroniser : Customs.Business.BillsSynchroniser
	{
		public BillsSynchroniser(JobDeclaration declaration, Func<IBillDetails, ZString> getBillNumber)
			: base(declaration, getBillNumber)
		{
		}

		protected override IEnumerable<BillDetailsWrapper> GetHouseBillsToSynchronise()
		{
			var shipmentsToSynch = declaration.GetShipmentsToSync().Where(s => !s.JS_HouseBill.IsEmpty
				|| s.Numbers.GetAllReferenceNumbersByType(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG).Where(n => !n.IsEmpty).Count() == 1);

			var primary = shipmentsToSynch.FirstOrDefault();
			if (primary != null)
			{
				yield return GetHouseBillToSynchronise(primary, true);
			}

			foreach (var secondary in shipmentsToSynch.Skip(1))
			{
				yield return GetHouseBillToSynchronise(secondary, false);
			}
		}
	}
}
