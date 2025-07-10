using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.JP.Business;
using Enterprise.Customs.JP.Common;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public partial class ExportControlNumberUserControl : ZUserControl, IExtendedControl, IResourceStringBindingMember
	{
		public ExportControlNumberUserControl()
		{
			InitializeComponent();
			InitializeControls();
			Extensions = new DefaultControlExtensionCollection(this);
		}

		Control IExtendedControl.Host => this;

		public IControlExtensionCollection Extensions { get; }

		string IResourceStringBindingMember.ResourceStringBindingMember => nameof(CusEntryInstruction.ExportControlNumber);

		void InitializeControls()
		{
			DeleteButton.Click -= DeleteButton_Click;
			DeleteButton.Click += DeleteButton_Click;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			DeleteButton.DataBindings.RemoveBinding(nameof(ZButton.IsVisibleForBinding));
			base.SetDataBinding(dataSource, dataMember);

			if (dataSource is JobDeclaration declaration)
			{
				DeleteButton.DataBindings.Add(new KBinding(nameof(ZButton.IsVisibleForBinding), declaration, DataMember + "." + nameof(CusEntryInstruction.DeleteExportControlEntryNumEnabled)));
			}
		}

		void DeleteButton_Click(object sender, System.EventArgs e)
		{
			using var menu = new EDIMenu();
			if (CurrentDataItem is CusEntryInstruction cusEntryInstruction
				&& cusEntryInstruction.EntryHeader is CusEntryHeader header
				&& cusEntryInstruction.JobDeclaration is JobDeclaration declaration)
			{
				var messageSendingContext = new MessageSendingContext()
				{
					EnableMessageVisual = true,
					ProcedureCode = JPProcedureCodeList.Codes.ECR,
					Action = ActionList.Codes.One,
					EntryHeadersToBeSent = [header]
				};

				using (declaration.SetCurrentMessageSendingContext(messageSendingContext))
				{
					menu.Declaration = declaration;
					menu.SendOrExportNACCSMessage(ParentForm as ZForm, JPProcedureCodeList.Codes.ECR);
				}
			}
		}
	}
}
