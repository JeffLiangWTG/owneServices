using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CusSealLookups : Customs.Business.CusSealLookups
	{
		public CusSealLookups(CusSeal parent) : base(parent)
		{
		}

		public CodeDescriptionPairList UnloadedStates => GetUnloadedStatesCore();

		protected virtual CodeDescriptionPairList GetUnloadedStatesCore()
		{
			var parent = Parent as CusSeal;
			var isNew = parent.BK_UnloadingStateInfo.OriginalValue.ToString() == NctsUnloadedStateList.Codes.NEW;
			return Factory.GetCachedValue(nameof(UnloadedStates) + isNew, () =>
			{
				var result = new NctsUnloadedStateList();
				if (!isNew)
				{
					result.RemoveCode(NctsUnloadedStateList.Codes.NEW);
				}
				result.RemoveCode(NctsUnloadedStateList.Codes.DIF);
				return result;
			});
		}
	}
}
