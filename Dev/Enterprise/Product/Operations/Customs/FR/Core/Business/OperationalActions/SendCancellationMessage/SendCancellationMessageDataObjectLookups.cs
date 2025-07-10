using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.OperationalActions
{
	public class SendCancellationMessageDataObjectLookups : ZLookups
	{
		public SendCancellationMessageDataObjectLookups(BusinessObject parent) : base(parent)
		{
		}

		public new SendCancellationMessageDataObject Parent => (SendCancellationMessageDataObject)base.Parent;

		public CodeDescriptionPairList MotivationList => RefCusCodeListTypes.GetCachedList(new BusinessObjectFactory(), Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, UniversalReferenceConstants.RefCusCodeListTypes.Codes.MotivationForInvalidationRequest, ZDate.Today, includeParentDataGrouping: false);
	}
}
