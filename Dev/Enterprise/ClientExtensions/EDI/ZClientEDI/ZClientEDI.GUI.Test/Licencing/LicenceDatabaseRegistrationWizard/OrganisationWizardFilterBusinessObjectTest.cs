using Enterprise.Client.EDI.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using ZClientEDI.GUI.Licencing;

namespace Enterprise.Client.EDI.Licencing.GUI.Testing
{
	[TestedType(typeof(OrganisationWizardFilterBusinessObject))]
	public class OrganisationWizardFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestLayoutContext()
		{
			var filter = GetNewFilterStripBusinessObject() as OrganisationWizardFilterBusinessObject;
			AssertEquals("Organisation", ((IFilterStripBusinessObjectInternals)filter).LayoutContext);
		}

		public void TestFilterIsEDI()
		{
			var filter = GetNewFilterStripBusinessObject() as EDIOrganisationFilterBusinessObjectCore;
			AssertNotNull(filter["License Database (for Master Organization)"]);
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new OrganisationWizardFilterBusinessObject();

		#endregion
	}
}
