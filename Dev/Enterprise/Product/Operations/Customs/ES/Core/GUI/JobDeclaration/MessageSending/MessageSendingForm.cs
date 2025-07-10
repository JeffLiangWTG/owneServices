using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageSending;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI
{
	public partial class MessageSendingForm : MessageSendingFormWithValidationDetails
	{
		public MessageSendingForm(JobDeclarationMessageSendingObjectParent declarationWrapper)
			: base(declarationWrapper)
		{
			InitializeComponent();
			InitializeNewColumns();
			LoadBottomFieldsWhenUcc6OrPOUSDeclaration();
		}

		public new JobDeclarationMessageSendingObjectParent BusinessEntity => (JobDeclarationMessageSendingObjectParent)base.BusinessEntity;

		void InitializeNewColumns()
		{
			var refNoTextBoxStyleInfo = new ZTextBoxColumnStyleInfo();
			refNoTextBoxStyleInfo.ColumnName = JobDeclarationMessageSendingObject.Schema.CH_BGMReference;
			refNoTextBoxStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			refNoTextBoxStyleInfo.CharacterCasing = CharacterCasing.Upper;
			MessageSendingObjectsGrid.ColumnStyles.Add(refNoTextBoxStyleInfo);

			MessageSendingObjectsGrid.ColumnStyles.Remove(MessageSendingObjectsGrid.GetColumnStyle(JobDeclarationMessageSendingObject.Schema.MessageType));
			var messageTypeDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			messageTypeDropEditColumnStyleInfo.ColumnName = JobDeclarationMessageSendingObject.Schema.MessageType;
			messageTypeDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			messageTypeDropEditColumnStyleInfo.CharacterCasing = CharacterCasing.Upper;
			messageTypeDropEditColumnStyleInfo.IsMandatory = true;
			MessageSendingObjectsGrid.ColumnStyles.Add(messageTypeDropEditColumnStyleInfo);

			var mrnTextBoxStyleInfo = new ZTextBoxColumnStyleInfo();
			mrnTextBoxStyleInfo.ColumnName = JobDeclarationMessageSendingObject.Schema.MRN;
			mrnTextBoxStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			mrnTextBoxStyleInfo.CharacterCasing = CharacterCasing.Upper;
			MessageSendingObjectsGrid.ColumnStyles.Add(mrnTextBoxStyleInfo);

			var subStyleDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			subStyleDropEditColumnStyleInfo.ColumnName = JobDeclarationMessageSendingObject.Schema.MessageSubType;
			subStyleDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			subStyleDropEditColumnStyleInfo.CharacterCasing = CharacterCasing.Upper;
			MessageSendingObjectsGrid.ColumnStyles.Add(subStyleDropEditColumnStyleInfo);

			var messageStatusTextBoxStyleInfo = new ZTextBoxColumnStyleInfo();
			messageStatusTextBoxStyleInfo.ColumnName = JobDeclarationMessageSendingObject.Schema.MessageStatus;
			messageStatusTextBoxStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			messageStatusTextBoxStyleInfo.CharacterCasing = CharacterCasing.Upper;
			MessageSendingObjectsGrid.ColumnStyles.Add(messageStatusTextBoxStyleInfo);
		}

		void LoadBottomFieldsWhenUcc6OrPOUSDeclaration()
		{
			if (ShowUcc6OrPOUSBottomSection)
			{
				MessageSendingObjectsGrid.AfterBind += MessageSendingObjectsGrid_AfterBind;
				InitializeBottomSectionFieldsVisibility();
			}
		}

		void MessageSendingObjectsGrid_AfterBind(object sender, EventArgs e)
		{
			if (ucc6OrPOUSBottomSectionUserControl != null)
			{
				if (MessageSendingObjectsGrid.ListManager != null)
				{
					MessageSendingObjectsGrid.ListManager.CurrentChanged += ListManager_CurrentChanged;
				}
			}
		}

		void ListManager_CurrentChanged(object sender, EventArgs e)
		{
			ucc6OrPOUSBottomSectionUserControl.MessageTypeChanged((JobDeclarationMessageSendingObject)MessageSendingObjectsGrid.ListManager.GetCurrent());
		}

		void InitializeBottomSectionFieldsVisibility()
		{
			if (ucc6OrPOUSBottomSectionUserControl != null)
			{
				var firstJobDeclarationMessageSendingObject = BusinessEntity.SendingObjectsCollection.Cast<JobDeclarationMessageSendingObject>().FirstOrDefault();
				if (firstJobDeclarationMessageSendingObject != null)
				{
					ucc6OrPOUSBottomSectionUserControl.MessageTypeChanged(firstJobDeclarationMessageSendingObject);
				}
			}
		}

		JobDeclaration Declaration => BusinessEntity.ParentDeclaration;
		bool ShowUcc6OrPOUSBottomSection => ((Declaration.IsExport || Declaration.IsImport) && BusinessEntity.SendingObjectsCollection.Cast<JobDeclarationMessageSendingObject>().Any(x => x.Header.IsUCC6))
													|| BusinessEntity.SendingObjectsCollection.Cast<JobDeclarationMessageSendingObject>().Any(x => x.Header.ZG_POUSVersion > 0);

		protected override ZUserControl GetBottomSectionUserControl()
		{
			if (ShowUcc6OrPOUSBottomSection)
			{
				ucc6OrPOUSBottomSectionUserControl = new Ucc6OrPOUSBottomSectionUserControl();
				return ucc6OrPOUSBottomSectionUserControl;
			}
			else
			{
				return base.GetBottomSectionUserControl();
			}
		}
		Ucc6OrPOUSBottomSectionUserControl ucc6OrPOUSBottomSectionUserControl;
	}
}
