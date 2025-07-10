using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Module.Testing
{
	[TestedType(typeof(MiscRequestMessagesController))]
	sealed class MiscRequestMessagesControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.KR.MiscRequestMessages;

		public override void TestEditForm()
		{
			Assert("Not Implemented", true);
		}

		public override void TestViewForm()
		{
			Assert("Not Implemented", true);
		}

		public override void TestNewForm()
		{
			Assert("Not Implemented", true);
		}

		public override void TestDeleteForm()
		{
			Assert("Not Implemented", true);
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert("Not Implemented", true);
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var request = Factory.New<CusMiscRequestHeader>();
			request.CMR_JobNumber = "11598210000001U";
			request.CMR_CustomsOffice = "01010";
			request.CMR_MessageType = ElectronicDocumentTypeList.Codes._5GW;
			request.CMR_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;
			request.CMR_RequestDate = ZDateTime.Today;
			request.CMR_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();
			return request;
		}
	}
}
