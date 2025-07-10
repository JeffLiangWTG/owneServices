using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class QueryOnGuaranteeSendingActionLookups : ZLookups
	{
		public QueryOnGuaranteeSendingActionLookups(QueryOnGuaranteeSendingAction sendingAction) : base(sendingAction) { }

		protected new QueryOnGuaranteeSendingAction Parent => (QueryOnGuaranteeSendingAction)base.Parent;

		public ICollection QueryIdentifier =>
				RefCusCodeListTypes.GetCachedList(
					Parent.Factory,
					Core.Constants.CountryCodes.Ireland,
					UniversalReferenceConstants.RefCusCodeListTypes.Codes.NGUAQ,
					ZDateTime.Today,
					includeParentDataGrouping: false
				);
	}
}
