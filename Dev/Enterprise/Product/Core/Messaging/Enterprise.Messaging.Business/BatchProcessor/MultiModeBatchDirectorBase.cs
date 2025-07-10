using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BatchProcessor
{
	public abstract class MultiModeBatchDirectorBase : BatchDirectorBase, IMultiModeBatchDirector
	{
		#region IMultiModeBatchDirector Members

		public virtual void SetMode(string mode)
		{
			this.Mode = mode ?? string.Empty;
		}

		protected string Mode;

		#endregion
	}
}
