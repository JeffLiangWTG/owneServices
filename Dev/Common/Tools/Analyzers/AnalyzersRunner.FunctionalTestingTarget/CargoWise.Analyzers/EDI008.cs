using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class EDI008
	{
		public void Method()
		{
			//EDI008:Log Reference Values In English Only Rule
			var message = Res.GetString("resource key", "english text");

			GlbStaff.CurrentUser.Logs.AddNew(Events.Logout, message);
		}
	}
}
