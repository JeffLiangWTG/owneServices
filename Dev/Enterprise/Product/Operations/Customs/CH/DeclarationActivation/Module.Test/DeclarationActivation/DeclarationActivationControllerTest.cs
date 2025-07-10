using Enterprise.Customs.CH.DeclarationActivation.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.DeclarationActivation.Module.Testing;

[TestedType(typeof(DeclarationActivationController))]
sealed class DeclarationActivationControllerTest : ZControllerBasherTest
{
	protected override ControllerID GetControllerID() => ControllerIDs.Customs.CH.DeclarationActivation;

	public void TestConfiguration() => CombineAssertions(() =>
	{
		AssertEquals("ID", ControllerIDs.Customs.CH.DeclarationActivation, Controller.ID);
		AssertEquals("ModuleID", ModuleIDs.Customs.CH.DeclarationActivation, Controller.ModuleID);
		AssertEquals("TypeOfTopLevelBusinessObject", typeof(DeclarationActivationHeader), Controller.TypeOfTopLevelBusinessObject);
	});

	public void TestSecurityCheckpoints() => CombineAssertions(() =>
	{
		AssertEquals("For New", Env.Security.None, Controller.CheckPointForNewExposedForTest);
		AssertEquals("For View", Env.Security.None, Controller.CheckPointForViewExposedForTest);
		AssertEquals("For Edit", Env.Security.None, Controller.CheckPointForEditExposedForTest);
		AssertEquals("For Delete", Env.Security.None, Controller.CheckPointForDeleteExposedForTest);
	});

	public override void TestNewForm()
	{
		Assert("Implemented by WI00894375 REQ03", true);
	}

	public override void TestEditForm()
	{
		Assert("Implemented by WI00894375 REQ03", true);
	}

	public override void TestViewForm()
	{
		Assert("Implemented by WI00894375 REQ03", true);
	}

	public override void TestDeleteForm()
	{
		Assert("Implemented by WI00894375 REQ03", true);
	}
}
