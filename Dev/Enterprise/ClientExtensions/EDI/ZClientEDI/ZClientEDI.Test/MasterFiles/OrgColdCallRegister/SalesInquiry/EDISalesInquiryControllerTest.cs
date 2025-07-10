using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Module.Testing
{
	[TestedType(typeof(EDISalesInquiryController))]
	internal class EDISalesInquiryControllerTest : ZControllerBasherTest
	{
		public void TestForm()
		{
			EDISalesInquiryController controller = new EDISalesInquiryController();
			EDISalesInquiry inquiry = Factory.New<EDISalesInquiry>();
			using (IZForm form = controller.ShowFormForNewEntity(inquiry))
			{
				AssertEquals(typeof(EDISalesInquiryForm), form.GetType());
			}
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.SalesEnquiry;
		}
	}
}
