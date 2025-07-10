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
	[TestedType(typeof(LPCODeclarationController))]
	class LPCODeclarationControllerTest : ZControllerBasherTest
	{
		public void TestCreateNewBusinessObject()
		{
			var controller = (ZControllerInternals)new LPCODeclarationController();
			var declaration = controller.GetNewBusinessEntityInFactory(Factory) as JobDeclaration;
			AssertEquals("JE_MessageType is LPC", BRJobMessageTypeList.Codes.LPCO, declaration.JE_MessageType);
			AssertEquals("FixedJobMessageType", BRJobMessageTypeList.Codes.LPCO, declaration.FixedJobMessageType);
		}

		public void TestShowEditForm()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
			Factory.Save();

			var controller = new LPCODeclarationController();
			using (var form = controller.ShowEditForm(declaration))
			{
				AssertType<JobDeclarationForm>(form);
				declaration = (form as JobDeclarationForm).Declaration as JobDeclaration;

				AssertEquals("JE_MessageType is LPC", BRJobMessageTypeList.Codes.LPCO, declaration.JE_MessageType);
				AssertEquals("FixedJobMessageType", BRJobMessageTypeList.Codes.LPCO, declaration.FixedJobMessageType);
			}
		}

		public void TestSecurityCheckpoints()
		{
			var controller = new LPCODeclarationController();
			AssertEquals(Env.Security.BRLPCODeclarationNew, controller.CheckPointForNewExposedForTest);
			AssertEquals(Env.Security.BRLPCODeclarationView, controller.CheckPointForViewExposedForTest);
			AssertEquals(Env.Security.BRLPCODeclarationEdit, controller.CheckPointForEditExposedForTest);
			AssertEquals(Env.Security.BRLPCODeclarationDelete, controller.CheckPointForDeleteExposedForTest);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.BR.LPCODeclaration;
		}

		public override Type ControllerToBashType
		{
			get { return typeof(LPCODeclarationController); }
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
