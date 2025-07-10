using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIOrgOpportunityValueCollection : OrgOpportunityValueCollection
	{
		public EDIOrgOpportunityValueCollection(EDIOrgOpportunity opportunity)
			: base(opportunity)
		{
		}

		public new EDIOrgOpportunityValue this[int i]
		{
			get { return (EDIOrgOpportunityValue)base[i]; }
		}

		public new EDIOrgOpportunityValue AddNew()
		{
			return (EDIOrgOpportunityValue)base.AddNew();
		}
	}
}

