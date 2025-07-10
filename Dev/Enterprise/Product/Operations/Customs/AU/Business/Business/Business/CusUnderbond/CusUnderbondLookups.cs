using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusUnderbondLookups : Customs.Business.CusUnderbondLookups
	{
		public CusUnderbondLookups(CusUnderbond underbond)
			: base(underbond)
		{
		}

		public override CodeDescriptionPairList ModeOfTransportList
		{
			get { return Factory.GetCachedValue<CMRUnderbondModeOfMovement>(); }
		}

		public override CodeDescriptionPairList RequestReasonList
		{
			get
			{
				var result = Factory.GetCachedValue<CMRUnderbondRequestCodes>();
				CusUnderbond underbond = Parent as CusUnderbond;
				if (underbond != null && !underbond.IsAirCargo)
				{
					result = new CMRUnderbondRequestCodes();
					result.RemoveCode(CMRUnderbondRequestCodes.Codes.OnAirToOffAirMovement);
				}
				return result;
			}
		}

		public override CodeDescriptionPairList UnderbondStatusList
		{
			get { return CMRBaseAndUnderbondStatuses.GetStatuses(Factory); }
		}

		public override CodeDescriptionPairList OutturnStatusList
		{
			get { return Factory.GetCachedValue<CMRBaseStatuses>(); }
		}

		public CodeDescriptionPairList AllUnderbondForList
		{
			get
			{
				CusUnderbond underbond = Parent as CusUnderbond;
				ICusUnderbondUnionCollectionParent underbondParent = null;
				if (underbond != null && underbond.LinkedObject != null)
				{
					if (underbond.MAWB != null)
					{
						underbondParent = underbond.MAWB;
					}
					else if (underbond.OceanBill != null)
					{
						underbondParent = underbond.OceanBill;
					}
				}

				CodeDescriptionPairList result = new CodeDescriptionPairList();
				if (underbondParent != null)
				{
					foreach (ICusUnderbondDependentCollectionParent provider in underbondParent.GetAllPossibleCollectionProviders())
					{
						result.Add(new CodeDescriptionPair(provider.UnderbondHumanReadableName.ToString(), provider.UnderbondHumanReadableName.ToString()));
					}
				}
				return result;
			}
		}
	}
}
