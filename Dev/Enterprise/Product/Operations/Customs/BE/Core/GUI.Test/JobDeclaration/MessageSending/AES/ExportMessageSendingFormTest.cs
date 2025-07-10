using System.Windows.Forms;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.GUI.Testing;

[TestedType(typeof(ExportMessageSendingForm))]
class ExportMessageSendingFormTest : ZFormBasherTest
{
	protected override Form GetFormToBashCore()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.CustomsEntryHeaders.AddNew();
		dec.CustomsEntryHeaders.AddNew();

		var parent = new ExportDeclarationMessageSendingActionParent(dec);
		return new ExportMessageSendingForm(parent);
	}
}
