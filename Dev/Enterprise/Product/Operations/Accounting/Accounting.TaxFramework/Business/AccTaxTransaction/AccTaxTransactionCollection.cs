using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using IntegrationAccounting = Enterprise.Integration.Accounting;

namespace Enterprise.Accounting.TaxFramework.Business
{
	public class AccTaxTransactionCollection : ActiveBusinessObjectCollection<AccTaxTransaction>
	{
		public AccTaxTransactionCollection(BusinessObjectFactory factory, AccTransactionHeader master, ITaxRecordParent taxRecordParent) : base(factory, master)
		{
			this.taxRecordParent = taxRecordParent;
		}

		protected AccTaxTransactionCollection(BusinessObjectFactory factory, ICollectionRelationship relationship, ITaxRecordParent taxRecordParent) : base(factory, relationship)
		{
			this.taxRecordParent = taxRecordParent;
		}

		readonly ITaxRecordParent taxRecordParent;

		protected override bool AllowNew => false;

		public override void Delete(AccTaxTransaction taxRecord)
		{
			using (Factory.SetTempContext(IntegrationAccounting.BusinessContext.MakingChangesToOtherTaxes))
			{
				ObjectFactory.Get<ITaxProcessor>().DeleteTaxRecordNotInDB(taxRecordParent, taxRecord);
			}
		}

		protected override object[] GetCollectionState()
		{
			return new object[] { taxRecordParent };
		}
	}
}
