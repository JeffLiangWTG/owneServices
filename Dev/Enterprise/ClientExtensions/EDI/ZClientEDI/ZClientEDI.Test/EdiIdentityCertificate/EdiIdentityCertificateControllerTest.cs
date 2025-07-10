using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IdentityCertificate.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IdentityCertificate.Module.Testing
{
	[TestedType(typeof(EdiIdentityCertificateController))]
	public class EdiIdentityCertificateControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return Modules.ClientControllerRegistration.EdiIdentityCertificate;
		}

		public void TestCheckPointForNew()
		{
			AssertEquals(EDISecurityCheckpoints.EdiIdentityCertificatesNew, Controller.CheckPointForNewExposedForTest);
		}

		public override void TestNewForm()
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

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return Factory.NewWithValidTestData<EdiIdentityCertificate>();
		}
	}
}
