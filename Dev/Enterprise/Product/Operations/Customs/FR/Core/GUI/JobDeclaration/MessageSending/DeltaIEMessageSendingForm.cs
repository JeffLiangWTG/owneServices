using Enterprise.Customs.FR.Business.MessageSending;

namespace Enterprise.Customs.FR.GUI
{
	public partial class DeltaIEMessageSendingForm : Customs.GUI.MessageSendingFormWithValidationDetails
	{
		public DeltaIEMessageSendingForm(DeltaIEJobDeclarationMessageSendingObjectParent declarationWrapper) : base(declarationWrapper)
		{
			SetLayout();
			var dataSource = (DeltaIEJobDeclarationMessageSendingObjectParent)DataSource;
			foreach (DeltaIEJobDeclarationMessageSendingObject sendingObject in dataSource.SendingObjectsCollection)
			{
				sendingObject.MessageTypeInfo.ValueChanged += MessageTypeInfo_ValueChanged;
			}
		}

		void MessageTypeInfo_ValueChanged(object sender, System.EventArgs e)
		{
			SetLayout();
		}

		void SetLayout()
		{
			if (((DeltaIEJobDeclarationMessageSendingObjectParent)MessageSendingObjectParent).ParentDeclaration.IsImport)
			{
				WarningSplitContainer.Panel2Collapsed = false;
			}
			else
			{
				WarningSplitContainer.Panel2Collapsed = true;
			}
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
	}
}
