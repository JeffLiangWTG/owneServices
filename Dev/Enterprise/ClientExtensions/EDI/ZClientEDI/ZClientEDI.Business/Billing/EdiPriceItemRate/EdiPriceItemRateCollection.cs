using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class EdiPriceItemRateCollection : ActiveBusinessObjectCollection<EdiPriceItemRate>
	{
		public EdiPriceItemRateCollection(ClientLicencePriceItem master)
			: base(master.Factory, master, new ZQuery(), EdiPriceItemRateSchema.PIR_L7)
		{
			Master = master;
		}

		public readonly ClientLicencePriceItem Master;

		#region Implementation

		protected override void SetRelationshipDefaultsForElementCore(EdiPriceItemRate newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
			newElement.PIR_L7 = Master.PK;
		}

		#endregion
	}
}

