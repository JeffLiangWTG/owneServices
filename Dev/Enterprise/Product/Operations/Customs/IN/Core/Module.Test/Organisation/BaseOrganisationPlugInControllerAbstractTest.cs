using System;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.IN.Module.Testing;

abstract class BaseOrganisationPlugInControllerAbstractTest : ZControllerBasherTest
{
	public void TestPlugIn()
	{
		using var tabControl = new ZTabControl();
		using var plugIns = new PlugIns(Factory.New<OrgHeader>(), tabControl);
		plugIns.Add(Controller.ID);
		using var plugIn = plugIns.GetPlugIn(Controller.ID);
		AssertType(ExpectPlugInType, plugIn);
	}

	public void TestPlugInCaption()
	{
		AssertEquals(ExpectedPlugInTabCaption, Controller.PluginTabPageCaption.Caption);
	}

	public void TestCheckPointForNew()
	{
		AssertEquals(Env.Security.OrganisationNew, Controller.CheckPointForNewExposedForTest);
	}

	public void TestCheckPointForView()
	{
		AssertEquals(Env.Security.OrganisationView, Controller.CheckPointForViewExposedForTest);
	}

	public void TestCheckPointForEdit()
	{
		AssertEquals(Env.Security.OrganisationModify, Controller.CheckPointForEditExposedForTest);
	}

	public void TestCheckPointForDelete()
	{
		AssertEquals(Env.Security.OrganisationDelete, Controller.CheckPointForDeleteExposedForTest);
	}

	protected abstract Type ExpectPlugInType { get; }

	protected virtual string ExpectedPlugInTabCaption => "Customs Defaults";
}
