using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(ProjectCollection))]
	class ProjectCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestModuleIDAttribute()
		{
			ModuleIDAttribute[] attributes = (ModuleIDAttribute[])GetCollectionToTest().GetType().GetCustomAttributes(typeof(ModuleIDAttribute), true);
			AssertEquals(ModuleIDs.Project, attributes[0].ModuleIdentifier);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ProjectCollection(Factory);
		}

		#endregion
	}
}
