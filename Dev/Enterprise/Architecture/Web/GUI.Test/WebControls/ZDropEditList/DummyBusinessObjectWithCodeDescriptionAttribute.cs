using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	[TestExcludeBusinessObjectsAllHaveTestCases]
	[CodeProperty("Z0_Code"), DescriptionProperty("Z0_Description")]
	class DummyBusinessObjectWithCodeDescriptionAttribute : DummyBusinessObject
	{
		public DummyBusinessObjectWithCodeDescriptionAttribute(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
