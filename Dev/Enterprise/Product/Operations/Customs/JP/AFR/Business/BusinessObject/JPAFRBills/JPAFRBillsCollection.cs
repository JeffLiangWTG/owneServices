using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class JPAFRBillsCollection : ActiveBusinessObjectCollection<JPAFRBills>, ISailingSynchronisationTargetCollection<BillOfLading, JPAFRBills>
	{
		public JPAFRBillsCollection(JPAFRHeader header)
			: base(header.Factory, new DependentRelationship(header, typeof(JPAFRBills), new ZQuery(JPAFRBillsSchema.JPB_JPH_Header, header.PK), JPAFRBillsSchema.JPB_JPH_Header))
		{
		}

		public JPAFRHeader Header
		{
			get { return (JPAFRHeader)Relationship.Master; }
		}

		public JPAFRBills AddNew(ZString billNumber)
		{
			var result = AddNew();
			result.JPB_BillNumber = billNumber;
			return result;
		}

		public JPAFRBills this[ZString billNumber]
		{
			get
			{
				JPAFRBills result = null;
				foreach (JPAFRBills bill in this)
				{
					if (!bill.IsDeleted && bill.JPB_BillNumber == billNumber)
					{
						result = bill;
						break;
					}
				}
				return result;
			}
		}

		#region Implementation

		protected override void OnLoadedIntoCollectionCore(JPAFRBills loadedObject)
		{
			loadedObject.InBondDetailInitiator = Header.InBondDetailInitiator;
			base.OnLoadedIntoCollectionCore(loadedObject);
		}

		protected override bool AllowNew
		{
			get
			{
				var master = Header;
				return master != null && !master.ShouldSynchroniseWithConsol;
			}
		}

		#endregion
	}
}
