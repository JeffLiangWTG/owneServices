using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CH.Business;

public abstract class BasePassarResponsePrettyFormatter<TResponseDetail>
{
	protected BasePassarResponsePrettyFormatter(BusinessObjectFactory factory, TResponseDetail responseDetail)
	{
		Factory = Argument.NotNull(factory, nameof(factory));
		ResponseDetail = Argument.NotNull(responseDetail, nameof(responseDetail));
	}
	protected BusinessObjectFactory Factory { get; }
	protected TResponseDetail ResponseDetail { get; }
}
