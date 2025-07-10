using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration
{
	public class UnRelatedConsolsToAddCollection : BusinessObjectCollection<ForwardingConsol>
	{
		public UnRelatedConsolsToAddCollection(ForwardingConsol parentConsol, ChiefRelatedConsolCollection relatedConsols)
			: base(parentConsol.Factory, GetAdditionalFilter(parentConsol, relatedConsols))
		{
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX notifications, BusinessObject selectedBusinessObject)
		{
			var candidateConsol = (ForwardingConsol)selectedBusinessObject;
			if (!candidateConsol.IsExport())
			{
				notifications.Add("You cannot select this item, it not an export consol");
			}
			if (!candidateConsol.IsAir)
			{
				notifications.Add("You cannot select this item, it not an air consol");
			}
		}

		static ZQuery GetAdditionalFilter(ForwardingConsol parentConsol, ChiefRelatedConsolCollection relatedConsols)
		{
			var query = new ZQuery();
			query.AddToFilter(JobConsolSchema.PK, SQLComparisonOperator.NotEqual, parentConsol.PK);
			query.AddToFilter(JobConsolSchema.PK, SQLComparisonOperator.NotEqual, (from BusinessObject c in relatedConsols select c.PK));
			query.AddToFilter(JobConsolSchema.JK_RL_NKLoadPort, SQLComparisonOperator.StartsWith, Core.Constants.CountryCodes.UnitedKingdom);
			query.AddToFilter(JobConsolSchema.JK_TransportMode, SQLComparisonOperator.Equal, TransportTypeList.Codes.Air);
			return query;
		}
	}
}
