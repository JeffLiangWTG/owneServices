using System;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.BR.GUI;
using Enterprise.Customs.Common.BR;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Module.Testing
{
	[TestedType(typeof(LicenseController))]
	class LicenseControllerTest : ZControllerBasherTest
	{
		public void TestCreateNewBusinessObject()
		{
			var controller = (ZControllerInternals)new LicenseController();
			var declaration = controller.GetNewBusinessEntityInFactory(Factory) as JobDeclaration;
			AssertEquals("JE_MessageType is LIC", BRJobMessageTypeList.Codes.ImportLicense, declaration.JE_MessageType);
			AssertEquals("FixedJobMessageType", BRJobMessageTypeList.Codes.ImportLicense, declaration.FixedJobMessageType);
		}

		public void TestShowEditForm()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			Factory.Save();

			var controller = new LicenseController();
			using (var form = controller.ShowEditForm(declaration))
			{
				AssertType<JobDeclarationForm>(form);
				declaration = (form as JobDeclarationForm).Declaration as JobDeclaration;

				AssertEquals("JE_MessageType is LIC", BRJobMessageTypeList.Codes.ImportLicense, declaration.JE_MessageType);
				AssertEquals("FixedJobMessageType", BRJobMessageTypeList.Codes.ImportLicense, declaration.FixedJobMessageType);
			}
		}

		public void TestSecurityCheckpoints()
		{
			var controller = new LicenseController();
			AssertEquals(Env.Security.BRLicenseNew, controller.CheckPointForNewExposedForTest);
			AssertEquals(Env.Security.BRLicenseView, controller.CheckPointForViewExposedForTest);
			AssertEquals(Env.Security.BRLicenseEdit, controller.CheckPointForEditExposedForTest);
			AssertEquals(Env.Security.BRLicenseDelete, controller.CheckPointForDeleteExposedForTest);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.BR.License;
		}

		public override Type ControllerToBashType
		{
			get { return typeof(LicenseController); }
		}

		protected override Type GetBusinessObjectType()
		{
			return typeof(JobDeclaration);
		}

		protected override string CountryCode
		{
			get { return Core.Constants.CountryCodes.Brazil; }
		}
	}
}
