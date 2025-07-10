using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using static Enterprise.Integration.Customs.CA;

namespace Enterprise.Customs.CA.Business
{
	public class CACusRulingConfigCollection : CusRulingConfigCombinedCollection, ICACusRulingConfigCollection
	{
		public CACusRulingConfigCollection(CACusRuling cusRuling)
			: base(cusRuling)
		{
		}

		public CACusRulingConfigCollection(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		public new CACusRulingConfig AddNew()
		{
			return (CACusRulingConfig)base.AddNew();
		}

		ICACusRulingConfig ICACusRulingConfigCollection.AddNew()
		{
			return AddNew();
		}

		public new CACusRulingConfig AddNew(ZString category, ZString type, ZDecimal rate, ZString value) => (CACusRulingConfig)base.AddNew(category, type, rate, value);

		public new CACusRulingConfig this[int index]
		{
			get { return (CACusRulingConfig)Elements[index]; }
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			if (!IsValidationSuspended)
			{
				RunPreSaveValidation();
			}
		}
	}
}
