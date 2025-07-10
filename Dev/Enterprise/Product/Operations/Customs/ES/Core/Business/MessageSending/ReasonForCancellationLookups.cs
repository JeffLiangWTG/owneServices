using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business
{
	public class ReasonForCancellationLookups : ZLookups
	{
		public ReasonForCancellationLookups(ReasonForCancellation parent) : base(parent)
		{
		}

		public CodeDescriptionPairList ReasonForCancellationList => Factory.GetCachedValue<ReasonForCancellationList>();
	}
}
