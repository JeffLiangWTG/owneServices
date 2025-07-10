using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class SupportingInformationControlTest : TestCaseWithFactory
{
	public void TestGetPreviousDocumentsUserControlType()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		using (var control = new SupportingInformationControlForTest())
		{
			control.SetDataBinding(declaration, string.Empty);
			AssertEquals("Export type", typeof(PreviousDocumentsUserControl), control.GetPreviousDocumentsUserControlType_Exposed());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			control.SetDataBinding(declaration, string.Empty);
			AssertEquals("Import type", typeof(LayoutPreviousDocumentsUserControl), control.GetPreviousDocumentsUserControlType_Exposed());
		}
	}

	public void TestGetSupportingDocumentsUserControlType()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		using (var control = new SupportingInformationControlForTest())
		{
			control.SetDataBinding(declaration, string.Empty);
			AssertEquals("Export type", typeof(DeclarationLayoutSupportingDocumentsUserControl), control.GetSupportingDocumentsUserControlType_Exposed());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			control.SetDataBinding(declaration, string.Empty);
			AssertEquals("Import type", typeof(DeclarationLayoutSupportingDocumentsUserControl), control.GetSupportingDocumentsUserControlType_Exposed());
		}
	}

	class SupportingInformationControlForTest : SupportingInformationControl
	{
		public Type GetPreviousDocumentsUserControlType_Exposed() => base.GetPreviousDocumentsUserControlType();
		public Type GetSupportingDocumentsUserControlType_Exposed() => base.GetSupportingDocumentsUserControlType();
	}
}
