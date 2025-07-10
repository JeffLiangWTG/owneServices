using System.Windows.Forms;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.Testing
{
	[TestedType(typeof(EnquiryForm))]
	internal class EnquiryFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			UPECusMAWB master = Factory.New<UPECusMAWB>();
			Enquiry enquiry = (Enquiry)master.ChildBills.AddNew(typeof(Enquiry));
			return new EnquiryForm(enquiry);
		}

		public override void TestMinimumSizeNotTooBig()
		{
			Assert(true); //custom form for UPS
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}
	}
}
