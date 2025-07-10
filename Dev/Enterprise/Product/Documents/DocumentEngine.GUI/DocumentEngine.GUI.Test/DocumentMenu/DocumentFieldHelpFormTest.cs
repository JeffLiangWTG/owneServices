using System;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.DocumentEngineCore.DocumentParsing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu.Testing
{
	[TestedType(typeof(DocumentFieldHelpForm))]
	sealed class DocumentFieldHelpForm_Test : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		public void TestDocumentFieldHelpForm()
		{
			DocumentFieldDefinitionCollection definitions = new DocumentFieldDefinitionCollection();
			definitions.Add(new DocumentFieldDefinition("Blob", "Blobs", DocumentFieldDefinition.FieldTypes.Property));

			DocumentFieldDefinitionFormBizo bizO = new DocumentFieldDefinitionFormBizo(definitions);

			using (DocumentFieldHelpFormForTest form = new DocumentFieldHelpFormForTest(bizO))
			{
				form.Show();

				AssertEquals("BizO should be the form's business entity", bizO, form.BusinessEntity);
				Assert("On Closing has NOT been called", !form.IsOnClosingCalled);
				form.Close();
				Assert("On Closing has been called", form.IsOnClosingCalled);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			DocumentFieldDefinitionCollection list = new DocumentFieldDefinitionCollection();
			DocumentFieldDefinitionFormBizo bizo = new DocumentFieldDefinitionFormBizo(list);
			return new DocumentFieldHelpForm(bizo);
		}

		public override Type FormToBashType
		{
			get { return typeof(DocumentFieldHelpForm); }
		}

		#endregion

		#region Test Objects

		class DocumentFieldHelpFormForTest : DocumentFieldHelpForm
		{
			public DocumentFieldHelpFormForTest(DocumentFieldDefinitionFormBizo fields)
				: base(fields)
			{
			}

			protected override void OnClosing(CancelEventArgs e)
			{
				IsOnClosingCalled = true;
				base.OnClosing(e);
			}

			public bool IsOnClosingCalled;
		}

		#endregion
	}
}
