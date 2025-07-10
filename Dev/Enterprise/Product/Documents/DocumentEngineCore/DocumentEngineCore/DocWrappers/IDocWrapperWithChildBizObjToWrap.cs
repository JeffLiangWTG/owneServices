using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngineCore.DocWrappers
{
	public interface IDocWrapperWithChildBizObjToWrap
	{
		BusinessObject WrappedBusinessObject { get; }
		BusinessObject WrappedChildBusinessObject { get; }
	}
}
