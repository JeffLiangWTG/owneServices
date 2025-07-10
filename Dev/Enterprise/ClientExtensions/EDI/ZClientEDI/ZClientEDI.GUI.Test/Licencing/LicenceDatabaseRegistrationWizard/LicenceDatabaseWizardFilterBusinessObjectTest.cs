using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using ZClientEDI.GUI.Licencing;

namespace Enterprise.Client.EDI.Licencing.GUI.Testing
{
	[TestedType(typeof(LicenceDatabaseWizardFilterBusinessObject))]
	public class LicenceDatabaseWizardFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestLayoutContext()
		{
			var bizo = GetNewFilterStripBusinessObject() as LicenceDatabaseWizardFilterBusinessObject;
			AssertEquals("LicenceDatabase", ((IFilterStripBusinessObjectInternals)bizo).LayoutContext);
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new LicenceDatabaseWizardFilterBusinessObject();

		#endregion
	}
}
