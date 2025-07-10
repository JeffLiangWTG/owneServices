using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Testing
{
	[TestedType(typeof(EdiGlbStaffFilterBusinessObject))]
	public class EdiGlbStaffFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestDomesticNameFilter()
		{
			var staff1 = Factory.NewWithValidTestData<EDIGlbStaff>();
			var staff1Ex = Factory.NewWithValidTestData<EdiGlbStaffEx>();
			staff1Ex.GS9_DomesticName = "Domestic Name";
			staff1Ex.GS9_GS = staff1.PK;
			var staff2 = Factory.NewWithValidTestData<EDIGlbStaff>();
			var staff2Ex = Factory.NewWithValidTestData<EdiGlbStaffEx>();
			staff2Ex.GS9_DomesticName = "Test name";
			staff2Ex.GS9_GS = staff2.PK;
			var staff3 = Factory.NewWithValidTestData<EDIGlbStaff>();
			Factory.Save();
			var glbStaffFilter = new EdiGlbStaffFilterBusinessObject();
			((ModuleTextFilter)glbStaffFilter["Domestic Name"]).Property = "Domestic Name";
			((ModuleTextFilter)glbStaffFilter["Domestic Name"]).IsActive = true;
			var collection1 = new GlbStaffCollection(Factory, glbStaffFilter.Filter);
			Assert("Collection should contain Staff1", collection1.Contains(staff1));
			Assert("Collection should not contain Staff2", !collection1.Contains(staff2));
			Assert("Collection should not contain Staff3", !collection1.Contains(staff3));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new EdiGlbStaffFilterBusinessObject();
	}
}
