using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation
{
	public class SimilarIncidentsWrapper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public SimilarIncidentsWrapper(SupportIncident parentIncident)
		{
			ParentIncident = parentIncident;
		}

		public SupportIncident ParentIncident { get; }

		public SimilarIncidentsFilter Filter
		{
			get
			{
				if (filter == null)
				{
					filter = new SimilarIncidentsFilter(ParentIncident);
					RegisterEditableChildObject(filter);
				}
				return filter;
			}
		}
		SimilarIncidentsFilter filter;
	}
}
