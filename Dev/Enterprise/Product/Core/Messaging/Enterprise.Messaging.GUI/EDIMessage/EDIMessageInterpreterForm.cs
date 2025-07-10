using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Messaging.GUI
{
	public partial class EDIMessageInterpreterForm : ZChildForm
	{
		public EDIMessageInterpreterForm()
		{
			InitializeComponent();
		}

		public EDIMessageInterpreterForm(EDIMessageInterpreter interpreter)
			: base(interpreter)
		{
			InitializeComponent();
		}

		public new EDIMessageInterpreter BusinessEntity => (EDIMessageInterpreter)base.BusinessEntity;

		public override string FormVerb => "";

		void InterpretButton_Click(object sender, System.EventArgs e)
		{
			BusinessEntity.GenerateMessageTextInterpretation();
		}
	}
}
