using CargoWise.Common;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Business
{
	public abstract class CusSupportingInfoProvider
	{
		protected CusSupportingInfoProvider(CusSupportingInfo cusSupportingInfo)
		{
			this.cusSupportingInfo = Argument.NotNull(cusSupportingInfo, nameof(cusSupportingInfo));
		}

		protected CusSupportingInfo cusSupportingInfo { get; }
	}
}
