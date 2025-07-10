using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture
{
	public interface IZAuditLogsForm : IZForm
	{
		bool IsAuditServerValid { get; }
	}
}
