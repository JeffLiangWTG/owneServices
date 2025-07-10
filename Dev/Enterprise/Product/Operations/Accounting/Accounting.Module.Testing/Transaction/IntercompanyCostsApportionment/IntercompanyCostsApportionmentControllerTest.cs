using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(IntercompanyCostsApportionmentController))]
	public class IntercompanyCostsApportionmentControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.APIntercompanyCostsApportionment;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return new IntercompanyCostsApportionmentInvoice(Factory);
		}

		public override void TestTemplateCopyForm()
		{
			Assert(true);
		}

		public override void TestDeleteForm()
		{
			Assert(true);
		}

		public override void TestEditForm()
		{
			Assert(true);
		}

		public override void TestViewForm()
		{
			Assert(true);
		}
	}
}
