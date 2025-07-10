using System.Windows.Forms;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.GUI.Testing
{
	[TestedType(typeof(EDISalesInquiryForm))]
	internal class EDISalesInquiryFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			EDISalesInquiry inquiry = Factory.New<EDISalesInquiry>();
			EDISalesInquiryForm form = new EDISalesInquiryForm(inquiry);
			form.Size = new System.Drawing.Size(1200, 770);
			form.ControllerID = ControllerIDs.SalesEnquiry;
			return form;
		}
	}
}
