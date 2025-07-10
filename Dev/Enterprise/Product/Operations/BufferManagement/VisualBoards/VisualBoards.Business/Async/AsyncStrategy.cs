using CargoWise.Async;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.VisualBoards.Business
{
	public static class AsyncStrategy
	{
		public static IAsyncStrategy Default
		{
			get
			{
				return
#if DEBUG
 OverriddenStrategy_ForTest.IsOverriden ? OverriddenStrategy_ForTest.Value : null ??
#endif
DBConnectionDisposalAsyncStrategy.Get();
			}
		}

#if DEBUG
		public static readonly Overridable<IAsyncStrategy> OverriddenStrategy_ForTest = new Overridable<IAsyncStrategy>();
#endif
	}
}
