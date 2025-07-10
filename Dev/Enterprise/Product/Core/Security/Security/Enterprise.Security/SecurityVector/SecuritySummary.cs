using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Security
{
	public interface ISecuritySummary<T>
	{
		ISecurityCheckpoint Checkpoint { get; }
		GlbStaff Staff { get; }
		T Summary { get; }
		bool Explicit { get; }
	}

	class SecuritySummary<T> : ISecuritySummary<T>
	{
		public SecuritySummary(ISecurityCheckpoint checkpoint, GlbStaff staff, T summary, bool explicit_)
		{
			Summary = summary;
			Checkpoint = checkpoint;
			Staff = staff;
			Explicit = explicit_;
		}

		public ISecurityCheckpoint Checkpoint { get; private set; }
		public GlbStaff Staff { get; private set; }
		public T Summary { get; private set; }
		public bool Explicit { get; private set; }
	}
}
