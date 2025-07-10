using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.GLAccountFormat;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(GLAccountFormatController))]
	public class GLAccountFormatControllerTest : ZSingletonControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.GLAccountFormat;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return new GLAccountFormatter();
		}
	}
}
