using CargoWise.EntityFramework;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Module.Testing
{
	[TestedType(typeof(EDIGlbStaffControllerOverride))]
	public class EDIGlbStaffControllerOverrideTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.GlbStaff;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			EDIGlbStaff staff = Factory.NewWithValidTestData<EDIGlbStaff>();
			Factory.Save();
			return staff;
		}
	}
}
