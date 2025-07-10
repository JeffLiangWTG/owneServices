using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class JPAFRContainerCollection : ActiveBusinessObjectCollection<JPAFRContainer>, ISailingSynchronisationTargetCollection<BillOfLadingContainer, JPAFRContainer>
	{
		public JPAFRContainerCollection(JPAFRBills bill)
			: base(bill.Factory, new DependentRelationship(bill, typeof(JPAFRContainer), new ZQuery(JPAFRContainerSchema.JPC_JPB_Bill, bill.PK), JPAFRContainerSchema.JPC_JPB_Bill))
		{
		}

		public JPAFRBills Bill
		{
			get { return (JPAFRBills)Relationship.Master; }
		}

		public JPAFRContainer AddNew(ZString containerNumber)
		{
			var result = AddNew();
			result.JPC_ContainerNum = containerNumber;
			return result;
		}

		public JPAFRContainer this[ZString containerNumber]
		{
			get
			{
				JPAFRContainer result = null;
				foreach (JPAFRContainer container in this)
				{
					if (!container.IsDeleted && container.JPC_ContainerNum == containerNumber)
					{
						result = container;
						break;
					}
				}
				return result;
			}
		}

		#region Implementation

		protected override bool AllowNew
		{
			get
			{
				var master = Bill;
				return master != null && !master.IsDeleted && !master.ShouldSynchronise;
			}
		}

		#endregion
	}
}
