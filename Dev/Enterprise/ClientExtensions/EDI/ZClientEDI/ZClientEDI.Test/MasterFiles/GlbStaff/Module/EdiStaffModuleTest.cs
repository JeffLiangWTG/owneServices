using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Module.Testing
{
	[TestedType(typeof(EdiStaffModule))]
	public class EdiStaffModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.GlbStaff;
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (var module = (EdiStaffModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				Assert(module.FilterBusinessObject is EdiGlbStaffFilterBusinessObject);
			}
		}
	}
}
