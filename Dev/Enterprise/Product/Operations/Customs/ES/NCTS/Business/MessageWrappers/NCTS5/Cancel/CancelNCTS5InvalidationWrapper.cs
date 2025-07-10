using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class CancelNCTS5InvalidationWrapper : ICancelNCTSInvalidation
	{
		public CancelNCTS5InvalidationWrapper(ZString reasonForCancellation)
		{
			Justification = Argument.NotNullOrEmpty(reasonForCancellation, nameof(reasonForCancellation));
		}

		public ZBool IsInitiatedByCustoms => ZBool.False;

		public ZString Justification { get; }
	}
}
