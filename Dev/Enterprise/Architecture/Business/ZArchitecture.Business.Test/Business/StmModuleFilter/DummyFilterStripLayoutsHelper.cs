using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public sealed class DummyFilterStripLayoutsHelper : FilterStripLayoutsHelper
	{
		public ZGuid CurrentUserPkForTest;
		public ZString CurrentUserTablePrefixForTest;

		protected override ZGuid GetCurrentUserPk()
		{
			return CurrentUserPkForTest.IsValid ? CurrentUserPkForTest : base.GetCurrentUserPk();
		}

		protected override ZString GetCurrentUserTablePrefix()
		{
			return !CurrentUserTablePrefixForTest.IsEmpty ? CurrentUserTablePrefixForTest : base.GetCurrentUserTablePrefix();
		}
	}
}
