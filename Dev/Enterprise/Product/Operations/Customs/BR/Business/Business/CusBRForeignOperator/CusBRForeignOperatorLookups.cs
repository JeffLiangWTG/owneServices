using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Business
{
	public class CusBRForeignOperatorLookups : AutoCusBRForeignOperatorLookups
	{
		public CusBRForeignOperatorLookups(AutoCusBRForeignOperator parent) : base(parent)
		{
		}

		public CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<BRMessageStatusList>();

		public CodeDescriptionPairList CustomsStatusTypeList => Factory.GetCachedValue<ForeignOperatorCustomsStatusTypeList>();
	}
}
