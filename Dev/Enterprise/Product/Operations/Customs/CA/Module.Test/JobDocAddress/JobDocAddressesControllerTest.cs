using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(JobDocAddressesController))]
	sealed class JobDocAddressesControllerTest : ZControllerBasherTest
	{
		public void TestCheckPoints()
		{
			var jobDocAddress = Factory.New<JobDocAddress>();
			var controller = new JobDocAddressesController();
			AssertEquals(Env.Security.CAJobDocAddressesView, controller.GetCheckPointForView(jobDocAddress));
			AssertEquals(Env.Security.CAJobDocAddressesDelete, controller.GetCheckPointForDelete(jobDocAddress));
			AssertEquals(Env.Security.CAJobDocAddressesEdit, controller.GetCheckPointForEdit(jobDocAddress));
			AssertEquals(Env.Security.CAJobDocAddressesNew, controller.GetCheckPointForNew(jobDocAddress));
		}

		public void TestCorrectForm()
		{
			AssertType<JobDeclarationForm>(Controller.ShowNewForm());
		}

		public override Type ControllerToBashType => typeof(JobDocAddressesController);

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.CA.CAJobDocAddresses;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var bizO = Factory.NewWithValidTestData<JobDocAddress>();
			bizO.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			bizO.E2_ParentID = declaration1.PK;
			bizO.E2_ParentTableCode = "JE";
			Factory.Save();
			return bizO;
		}
	}
}
