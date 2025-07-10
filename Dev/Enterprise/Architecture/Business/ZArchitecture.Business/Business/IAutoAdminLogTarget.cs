using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	public interface IAutoAdminLogTarget : IAutoLog
	{
		void OnCreateAutoAdminLog();
		ZString CustomLogReferenceSuffix { get; }
		bool ShouldCreateAutoLogIfOnlyChildrenHaveChanges { get; }
		Logs Logs { get; }
		bool IsAutoLogged { get; }
	}
}
