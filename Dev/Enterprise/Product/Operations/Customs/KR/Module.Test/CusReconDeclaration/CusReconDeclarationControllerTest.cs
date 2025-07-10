using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Module.Testing
{
	[TestedType(typeof(CusReconDeclarationController))]
	sealed class CusReconDeclarationControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.KR.CusReconDeclaration;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var request = Factory.New<CusReconDeclaration>();
			request.CRD_ApplicationCode = "KRC";
			request.CRD_JobReferenceNumber = "11598210000001U";
			request.CRD_CustomsOffice = "010";
			request.CRD_MessageStatus = CustomsMessageStatusTypeList.Codes.OriginalSent;
			request.CRD_GB_Branch = GlbBranch.CurrentBranch.PK;
			Factory.Save();
			return request;
		}
	}
}
