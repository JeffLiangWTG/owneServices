using CargoWise.BuildTools.Testing;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Module.Testing
{
	[TestedType(typeof(RefDocSourceController))]
	internal sealed class RefDocSourceControllerTest : ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			RefDocSource docSource = Factory.New<RefDocSource>();
			docSource.RDS_Code = "QQQ";
			docSource.RDS_Desc = "This is a test doc Source";
			Factory.Save();
			return docSource;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.RefDocSource;
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			MockSourceControl.Setup();
		}

		protected override void TearDown()
		{
			base.TearDown();
			MockSourceControl.TearDown();
		}

		#endregion
	}
}
