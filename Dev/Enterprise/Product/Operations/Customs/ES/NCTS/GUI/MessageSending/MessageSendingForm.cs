using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public partial class MessageSendingForm : MessageSendingFormWithValidationDetails
	{
		public MessageSendingForm(NctsHeaderMessageSendingObjectParent sendingObjectParent)
			: base(sendingObjectParent)
		{
			InitializeComponent();
			LoadBottomFields();
		}

		public new NctsHeaderMessageSendingObjectParent BusinessEntity => (NctsHeaderMessageSendingObjectParent)base.BusinessEntity;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();

			if (BusinessEntity.SendingObjectsCollection is IBusinessObjectCollection sendingObjectCollection
				&& sendingObjectCollection.Count > 0
				&& sendingObjectCollection[0] is BusinessObject sendingObject)
			{
				MessageSendingObjectsGrid.TabStop = BusinessEntity
					.MessageSendingObjectProperties
					.Select(x => x.PropertyName)
					.Any(x => !sendingObject.FindPropertyInfo(x).ReadOnly);
			}
		}

		void LoadBottomFields()
		{
			if (ShowBottomSection)
			{
				MessageSendingObjectsGrid.AfterBind += MessageSendingObjectsGrid_AfterBind;
				InitializeExportBottomSectionFieldsVisibility();
			}
		}

		void MessageSendingObjectsGrid_AfterBind(object sender, EventArgs e)
		{
			if (ncts5BottomSectionUserControl != null)
			{
				if (MessageSendingObjectsGrid.ListManager != null)
				{
					MessageSendingObjectsGrid.ListManager.CurrentChanged += ListManager_CurrentChanged;
				}
			}
		}

		void ListManager_CurrentChanged(object sender, EventArgs e)
		{
			ncts5BottomSectionUserControl.MessageTypeChanged((NctsHeaderMessageSendingObject)MessageSendingObjectsGrid.ListManager.GetCurrent());
		}

		void InitializeExportBottomSectionFieldsVisibility()
		{
			if (ncts5BottomSectionUserControl != null)
			{
				var firstJobDeclarationMessageSendingObject = BusinessEntity.SendingObjectsCollection.Cast<NctsHeaderMessageSendingObject>().FirstOrDefault();
				if (firstJobDeclarationMessageSendingObject != null)
				{
					ncts5BottomSectionUserControl.MessageTypeChanged(firstJobDeclarationMessageSendingObject);
				}
			}
		}

		NctsHeader NctsHeader => (NctsHeader)BusinessEntity?.TopLevelBusinessObject;
		bool ShowBottomSection => NctsHeader.IsPhase5;

		protected override ZUserControl GetBottomSectionUserControl()
		{
			if (ShowBottomSection)
			{
				ncts5BottomSectionUserControl = new Ncts5BottomSectionUserControl();
				return ncts5BottomSectionUserControl;
			}
			else
			{
				return base.GetBottomSectionUserControl();
			}
		}
		Ncts5BottomSectionUserControl ncts5BottomSectionUserControl;

		protected override void Dispose(bool disposing)
		{
			if (ncts5BottomSectionUserControl != null)
			{
				if (MessageSendingObjectsGrid.ListManager != null)
				{
					MessageSendingObjectsGrid.ListManager.CurrentChanged -= ListManager_CurrentChanged;
				}
			}

			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
