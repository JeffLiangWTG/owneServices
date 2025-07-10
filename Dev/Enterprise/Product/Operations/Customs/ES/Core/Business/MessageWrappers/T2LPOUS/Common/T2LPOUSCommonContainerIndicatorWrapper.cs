using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class T2LPOUSCommonContainerIndicatorWrapper : IT2LPOUSCommonContainerIndicator
	{
		public T2LPOUSCommonContainerIndicatorWrapper(ZBool indicator)
		{
			IsContainerised = indicator;
		}

		public ZBool IsContainerised { get; }
	}
}
