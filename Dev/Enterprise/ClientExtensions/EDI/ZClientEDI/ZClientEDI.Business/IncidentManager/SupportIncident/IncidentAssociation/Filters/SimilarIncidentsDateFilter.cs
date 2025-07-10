using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation
{
	public class SimilarIncidentsDateFilter : ModuleDateFilter
	{
		public SimilarIncidentsDateFilter(ZString description)
			: base(description, Schema.GenericDateTimeColumn, true)
		{
			isNullable = false;
			HideFutureDates = true;
			HideOffsetFilters = true;
		}

		public new ZDateTime FromDate => base.FromDate;

		public new ZDateTime ToDate => base.ToDate;
	}
}