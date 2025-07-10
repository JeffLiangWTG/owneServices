using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Common
{
	public class BorderWiseReturnHookNonPersistentBizo : NonPersistentBusinessObject
	{
		public BorderWiseReturnHookNonPersistentBizo()
			: this(ZGuid.NewZGuid())
		{
		}

		public BorderWiseReturnHookNonPersistentBizo(ZGuid pk)
		{
			this.pk = pk;
		}

		readonly ZGuid pk;

		protected override ZGuid GetPK() => pk;
		public List<BorderWiseInvoiceLine> BorderWiseInvoiceLines { get; set; }
		public string SelectedTariff { get; set; }
		public string SelectedStat { get; set; }
	}
}
