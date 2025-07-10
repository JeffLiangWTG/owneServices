using System.ComponentModel;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI
{
	public partial class DocumentFieldHelpForm : ZChildForm
	{
		public DocumentFieldHelpForm(DocumentFieldDefinitionFormBizo fields)
			: base(fields)
		{
			InitializeComponent();
			SetupInformationTexts();

#if DEBUG
			TypeDescriptor.AddAttributes(ExplanationLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(TagLabel, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		#region GUI Setup

		void SetupInformationTexts()
		{
			ExplanationLabel.Text = Res.GetString("c0da8609-1c3e-481e-b1ff-9f33f0f1f4be", "You can place special fields in your document that will be merged with {0} information before sending or printing the document. For example, a field called {1} would be replaced by the contact's full name when the document is sent.", Core.Constants.ProductName, "ContactName");
			TagLabel.Text = Res.GetString("6298b314-421b-4bed-b7ba-b54c5eee9bab", "IMPORTANT: \r\n\r\nWhen customizing Excel Templates, all fields should start with < and end with > e.g. {0}\r\n\r\nWhen customizing HTML documents, all fields should start with {1} and end with {2} e.g. {3}", "<Email>", StartTag, EndTag, StartTag + "Email" + EndTag);
		}

		readonly string StartTag = Core.Constants.DocumentEngine.EmailParsing.StartTag;
		readonly string EndTag = Core.Constants.DocumentEngine.EmailParsing.EndTag;

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		#endregion

		#region Close

		void CloseButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		#endregion
	}
}
