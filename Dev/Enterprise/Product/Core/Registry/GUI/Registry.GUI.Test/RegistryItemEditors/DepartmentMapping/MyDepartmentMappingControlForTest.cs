using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(DepartmentMappingRegistryItemEditor))]
	sealed class MyDepartmentMappingControlForTest : DepartmentMappingRegistryItemEditor.MyDepartmentMappingControl
	{
		public new DepartmentMappingCollectionWrapper Data
		{
			get { return base.Data; }
		}
	}
}
