using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	public partial class AssignLocationAndReferenceForm : ZChildForm
	{
		public AssignLocationAndReferenceForm() : base()
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			EmptyLocationCheckBox.CheckedChanged += new EventHandler(EmptyLocationCheckBoxChanged);
			EmptyReferenceCheckBox.CheckedChanged += new EventHandler(EmptyReferenceCheckBoxChanged);
		}

		void EmptyLocationCheckBoxChanged(object sender, EventArgs e)
		{
			LocationTextBox.ReadOnly = EmptyLocationCheckBox.Checked;
		}

		void EmptyReferenceCheckBoxChanged(object sender, EventArgs e)
		{
			ReferenceTextBox.ReadOnly = EmptyReferenceCheckBox.Checked;
		}

		public override string FormVerb => string.Empty;
		public override string FormCaption => Res.GetString("1449B8DC-40A5-4E86-AE61-F6C599521D08", "Assign Location + Reference");
	}
}
