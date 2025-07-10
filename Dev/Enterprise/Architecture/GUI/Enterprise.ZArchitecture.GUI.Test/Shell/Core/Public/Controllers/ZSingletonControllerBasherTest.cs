using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	[TestsSubclassesOf(typeof(ZSingletonController), ExcludeClientDlls = true)]
	public abstract class ZSingletonControllerBasherTest : ZPopupControllerBasherTest
	{
	}
}
