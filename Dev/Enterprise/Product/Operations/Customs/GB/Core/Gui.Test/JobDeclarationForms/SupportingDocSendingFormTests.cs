using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.JobDeclarationForms.Testing
{
	[TestedType(typeof(SupportingDocSendingForm))]
	class SupportingDocSendingFormTests : Customs.GUI.Testing.MessageSendingObjectFormTest
	{
		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			return new SupportingDocSendingForm(new JobDeclarationSupportingDocSendingObjectParent(declaration));
		}
	}
}
