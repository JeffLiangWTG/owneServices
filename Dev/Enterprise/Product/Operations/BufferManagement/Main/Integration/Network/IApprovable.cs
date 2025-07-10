namespace Enterprise.BufferManagement.Integration
{
	public interface IApprovable
	{
		bool IsApproved { get; }
		void Approve(string approverCode);
		void UnApprove();
	}

	public static class IApprovableExtensions
	{
		public static void ToggleApproval(this IApprovable approvable, string approverCode = null)
		{
			if (string.IsNullOrEmpty(approverCode))
			{
				approvable.UnApprove();
			}
			else
			{
				approvable.Approve(approverCode);
			}
		}
	}
}
