using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.OperationalActions
{
	public class CreditCODDataObjectCollection : NonPersistentBusinessObjectCollection<CreditCODDataObject>
	{
		public CreditCODDataObjectCollection(FrCreditCODApplicator header) : base(header.Factory)
		{
			this.header = header;
		}

		readonly FrCreditCODApplicator header;

		protected override BusinessObject CreateNonPersistentBusinessObject() => new CreditCODDataObject(header, header.Factory);
	}
}
