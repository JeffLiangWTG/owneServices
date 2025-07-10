using System.Windows.Forms;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.GUI.Testing;

[TestedType(typeof(ImportMessageSendingForm))]
sealed class ImportMessageSendingFormTest : ZFormBasherTest
{
	protected override Form GetFormToBashCore()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.CustomsEntryHeaders.AddNew();
		declaration.CustomsEntryHeaders.AddNew();

		var parent = new ImportDeclarationMessageSendingActionParent(declaration);
		return new ImportMessageSendingForm(parent);
	}
}
