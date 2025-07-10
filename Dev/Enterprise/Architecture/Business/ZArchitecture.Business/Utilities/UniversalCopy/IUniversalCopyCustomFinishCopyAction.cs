using System.Collections.Generic;

namespace Enterprise.ZArchitecture.Business.Utilities.UniversalCopy
{
	public interface IUniversalCopyCustomFinishCopyAction
	{
		public int Priority { get; }

		public void FinishCopyAction(Dictionary<object, object> copiedEntities);
	}
}
