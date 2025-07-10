using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.WCB.DaimlerChrysler.GUI.Testing
{
	[TestedType(typeof(DCImportDataEditorForm))]
	class DCImportDataEditorFormTest : ZFormBasherTest
	{
		public void TestFormVerbAndCaption()
		{
			using (DCImportDataEditorForm form = new DCImportDataEditorForm(JobDecs))
			{
				AssertEquals("Form Verb", "", form.FormVerb);
				AssertEquals("Form Caption", "Imported declarations", form.FormCaption);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new DCImportDataEditorForm(JobDecs);
		}

		protected override void SetUp()
		{
			base.SetUp();
			JobDecs = new JobDeclarationCollection(Factory);
			var jobDec = JobDecs.AddNew();
			jobDec.FillWithValidTestData();
			jobDec.Invoices.AddNew();
			Factory.Save();
		}

		JobDeclarationCollection JobDecs;
	}
}
