using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.Declaration.Testing;

sealed class SupportingInformationControlTest : TestCaseWithFactory
{
	public void TestGetPreviousDocumentsUserControlType()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		using (var control = new SupportingInformationControlForTest())
		{
			control.SetDataBinding(declaration, string.Empty);
			AssertEquals("Export type", typeof(PlugIn.PreviousDocumentsUserControl), control.GetPreviousDocumentsUserControlType_Exposed());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			control.SetDataBinding(declaration, string.Empty);
			AssertEquals("Import type", typeof(PlugIn.PreviousDocumentsUserControl), control.GetPreviousDocumentsUserControlType_Exposed());
		}
	}

	public void TestGetSupportingDocumentsUserControlType()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		using (var control = new SupportingInformationControlForTest())
		{
			control.SetDataBinding(declaration, string.Empty);
			AssertEquals("Export type", typeof(PlugIn.SupportingDocumentsUserControl), control.GetSupportingDocumentsUserControlType_Exposed());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			control.SetDataBinding(declaration, string.Empty);
			AssertEquals("Import type", typeof(PlugIn.SupportingDocumentsUserControl), control.GetSupportingDocumentsUserControlType_Exposed());
		}
	}

	public void TestGetAdditionalInfosUserControlType()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = Customs.FR.Business.DeclarationApplicationCodeList.Codes.DeltaIE;
		using (var control = new SupportingInformationControlForTest())
		{
			control.SetDataBinding(declaration, string.Empty);
			AssertEquals("Import ApplicationCode DI", typeof(AdditionalInfosUserControlWithGrid), control.GetAdditionalInfosUserControlType_Exposed());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.FR.Business.DeclarationApplicationCodeList.Codes.DeltaG;
			control.SetDataBinding(declaration, string.Empty);
			AssertEquals("Import ApplicationCode DG", typeof(PlugIn.AdditionalInfosUserControl), control.GetAdditionalInfosUserControlType_Exposed());
		}
	}

	public void TestAdditionalInfosTabPage()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

		using (var userControl = new SupportingInformationControl())
		{
			userControl.SetDataBinding(declaration, string.Empty);
			var tabPage = userControl.FindSingle<ZTabPage>("AdditionalInfoTabPage");
			CombineAssertions(() =>
			{
				AssertEquals("AdditionalInfosTabPage visible", true, tabPage.TabVisible);
				AssertEquals("Caption for export", "[44] Special Mentions", tabPage.CaptionResourceString.Caption);
			});
		}

		using (var userControl = new SupportingInformationControl())
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.FR.Business.DeclarationApplicationCodeList.Codes.DeltaG;
			userControl.SetDataBinding(declaration, string.Empty);
			var tabPage = userControl.FindSingle<ZTabPage>("AdditionalInfoTabPage");
			AssertEquals("Caption for import and ApplicationCode is DG", "[44] Special Mentions", tabPage.CaptionResourceString.Caption);
		}

		using (var userControl = new SupportingInformationControl())
		{
			declaration.JE_ApplicationCode = Customs.FR.Business.DeclarationApplicationCodeList.Codes.DeltaIE;
			userControl.SetDataBinding(declaration, string.Empty);
			var tabPage = userControl.FindSingle<ZTabPage>("AdditionalInfoTabPage");
			AssertEquals("Caption for import and ApplicationCode is DI", "[44] Additional Documents", tabPage.CaptionResourceString.Caption);
		}
	}

	class SupportingInformationControlForTest : SupportingInformationControl
	{
		public Type GetPreviousDocumentsUserControlType_Exposed() => base.GetPreviousDocumentsUserControlType();
		public Type GetSupportingDocumentsUserControlType_Exposed() => base.GetSupportingDocumentsUserControlType();
		public Type GetAdditionalInfosUserControlType_Exposed() => base.GetAdditionalInfosUserControlType();
	}
}
