
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.HK.Business
{
	/// <summary>
	/// Summary description for TraxonMessageCollection.
	/// </summary>
	public class TraxonMessageCollection : BusinessObjectCollection<TraxonMessage>
	{
		public TraxonMessageCollection(TraxonConsolStatus parent)
			: base(parent.Factory)
		{
			this.parent = parent;
			SetReadOnlyIncludingChildren(true);
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery filter = new ZQuery();
			if (parent != null)
			{
				filter.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, parent.Consol.PK);
				filter.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.Traxon);
			}
			else
			{
				filter = ZQuery.NoResultQuery;
			}
			return filter;
		}

		#region Implementation
		protected TraxonConsolStatus parent;
		#endregion
	}
}
