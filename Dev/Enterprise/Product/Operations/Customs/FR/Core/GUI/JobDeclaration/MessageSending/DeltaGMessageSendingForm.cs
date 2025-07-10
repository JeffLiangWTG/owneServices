using System.Windows.Forms;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI
{
	public partial class DeltaGMessageSendingForm : MessageSendingFormWithValidationDetails
	{
		ZMultiLineTextBoxColumnInfo vocReasonzTextBoxColumnStyleInfo;

		public DeltaGMessageSendingForm(DeltaGJobDeclarationMessageSendingObjectParent declarationWrapper)
			: base(declarationWrapper)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			InitializeNewColumns();
		}

		void InitializeNewColumns()
		{
			var messageTypeDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			messageTypeDropEditColumnStyleInfo.ColumnName = DeltaGJobDeclarationMessageSendingObject.Schema.MessageType;
			messageTypeDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			messageTypeDropEditColumnStyleInfo.CharacterCasing = CharacterCasing.Upper;
			messageTypeDropEditColumnStyleInfo.IsMandatory = true;
			MessageSendingObjectsGrid.ColumnStyles.Add(messageTypeDropEditColumnStyleInfo);

			var entryInstructionDescriptionTextBoxStyleInfo = new ZTextBoxColumnStyleInfo();
			entryInstructionDescriptionTextBoxStyleInfo.ColumnName = DeltaGJobDeclarationMessageSendingObject.Schema.EntryInstructionDescription;
			entryInstructionDescriptionTextBoxStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			MessageSendingObjectsGrid.ColumnStyles.Add(entryInstructionDescriptionTextBoxStyleInfo);

			var sequenceNumberTextBoxStyleInfo = new ZTextBoxColumnStyleInfo();
			sequenceNumberTextBoxStyleInfo.ColumnName = DeltaGJobDeclarationMessageSendingObject.Schema.SequenceNumber;
			sequenceNumberTextBoxStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			sequenceNumberTextBoxStyleInfo.CharacterCasing = CharacterCasing.Upper;
			sequenceNumberTextBoxStyleInfo.IsMandatory = true;
			MessageSendingObjectsGrid.ColumnStyles.Add(sequenceNumberTextBoxStyleInfo);

			var dateMessageTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			dateMessageTextBoxColumnStyleInfo.ColumnName = DeltaGJobDeclarationMessageSendingObject.Schema.DateMessage;
			dateMessageTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			MessageSendingObjectsGrid.ColumnStyles.Add(dateMessageTextBoxColumnStyleInfo);

			var amendmentReasonDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			amendmentReasonDropEditColumnStyleInfo.ColumnName = DeltaGJobDeclarationMessageSendingObject.Schema.ChangeAcknowledgementIndicator;
			amendmentReasonDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			amendmentReasonDropEditColumnStyleInfo.CharacterCasing = CharacterCasing.Upper;
			amendmentReasonDropEditColumnStyleInfo.IsMandatory = true;
			amendmentReasonDropEditColumnStyleInfo.IsVisible = false;
			MessageSendingObjectsGrid.ColumnStyles.Add(amendmentReasonDropEditColumnStyleInfo);

			vocReasonzTextBoxColumnStyleInfo = new ZMultiLineTextBoxColumnInfo();
			vocReasonzTextBoxColumnStyleInfo.ColumnName = DeltaGJobDeclarationMessageSendingObject.Schema.VOCReason;
			vocReasonzTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			MessageSendingObjectsGrid.ColumnStyles.Add(vocReasonzTextBoxColumnStyleInfo);

			var declarationTypeDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			declarationTypeDropEditColumnStyleInfo.ColumnName = DeltaGJobDeclarationMessageSendingObject.Schema.DeclarationType;
			declarationTypeDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			declarationTypeDropEditColumnStyleInfo.CharacterCasing = CharacterCasing.Upper;
			declarationTypeDropEditColumnStyleInfo.IsMandatory = true;
			MessageSendingObjectsGrid.ColumnStyles.Add(declarationTypeDropEditColumnStyleInfo);

			var replacementDeclarationTypeDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			replacementDeclarationTypeDropEditColumnStyleInfo.ColumnName = Business.MessageSending.DeltaGJobDeclarationMessageSendingObject.Schema.ReplacementDeclarationType;
			replacementDeclarationTypeDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			replacementDeclarationTypeDropEditColumnStyleInfo.CharacterCasing = CharacterCasing.Upper;
			MessageSendingObjectsGrid.ColumnStyles.Add(replacementDeclarationTypeDropEditColumnStyleInfo);

			var doNotRecalculateEntrySubstyleTextBoxStyleInfo = new ZCheckBoxColumnStyleInfo();
			doNotRecalculateEntrySubstyleTextBoxStyleInfo.ColumnName = DeltaGJobDeclarationMessageSendingObject.Schema.DoNotRecalculateEntrySubstyle;
			doNotRecalculateEntrySubstyleTextBoxStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			doNotRecalculateEntrySubstyleTextBoxStyleInfo.CharacterCasing = CharacterCasing.Upper;
			MessageSendingObjectsGrid.ColumnStyles.Add(doNotRecalculateEntrySubstyleTextBoxStyleInfo);

			var triggeringPointofvalidationTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			triggeringPointofvalidationTextBoxColumnStyleInfo.ColumnName = DeltaGJobDeclarationMessageSendingObject.Schema.TriggeringPointForValidation;
			triggeringPointofvalidationTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			triggeringPointofvalidationTextBoxColumnStyleInfo.CharacterCasing = CharacterCasing.Upper;
			triggeringPointofvalidationTextBoxColumnStyleInfo.IsMandatory = true;
			MessageSendingObjectsGrid.ColumnStyles.Add(triggeringPointofvalidationTextBoxColumnStyleInfo);
		}
		public new DeltaGJobDeclarationMessageSendingObjectParent BusinessEntity => (DeltaGJobDeclarationMessageSendingObjectParent)base.BusinessEntity;
	}
}
