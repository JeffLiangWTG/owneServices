using CargoWise.BuildTools.Testing;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Module.Testing
{
	[TestedType(typeof(RefDocTypeController))]
	internal sealed class RefDocTypeControllerTest : ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			RefDocType docType = Factory.New<RefDocType>();
			docType.RT_DocType = "QQQ";
			docType.RT_ReferenceType = Core.Constants.DocManagerCodes.Shipment;
			docType.RT_Desc = "This is a test doc type";
			Factory.Save();
			return docType;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.RefDocType;
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
