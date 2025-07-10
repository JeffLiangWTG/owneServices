using Enterprise.Customs.BE.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.BE.GUI;

public partial class MessageSendingForm<ActionParentType> : MessageSendingFormWithValidationDetails
where ActionParentType : BaseMessageSendingObjectParent
{
	/// Required method for Designer support - do not modify or use
	public MessageSendingForm(ActionParentType declarationWrapper) : base(declarationWrapper)
	{
	}

	public MessageSendingForm(ActionParentType parent, string messageType) : base(parent)
	{
		this.messageType = messageType;
	}

	public MessageSendingForm(ActionParentType parent, string messageType, string groupBoxText) : base(parent)
	{
		this.messageType = messageType;
		messageSendingObjectsGroupBox.Text = Res.GetString("7C46E78D-298A-4D19-BAE0-1AF6E1A22CEC", "{0}", groupBoxText);
	}

	readonly string messageType;

	public override string FormHeading => Res.GetString("EFA255A2-50A5-4D78-BB72-2533DC0E9C4D", "Send {0}", messageType);

	protected override void InitialiseForm()
	{
		base.InitialiseForm();
		InitializeComponent();
		InitializeNewColumns();
	}

	void InitializeNewColumns()
	{
		var bgmRefTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
		bgmRefTextBoxColumnStyleInfo.ColumnName = BEJobDeclarationMessageSendingObject.Schema.BGMReference;
		bgmRefTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
		bgmRefTextBoxColumnStyleInfo.IsMandatory = true;
		MessageSendingObjectsGrid.ColumnStyles.Add(bgmRefTextBoxColumnStyleInfo);

		var ceiDescTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
		ceiDescTextBoxColumnStyleInfo.ColumnName = BEJobDeclarationMessageSendingObject.BESchema.Description;
		ceiDescTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
		ceiDescTextBoxColumnStyleInfo.IsMandatory = true;
		MessageSendingObjectsGrid.ColumnStyles.Add(ceiDescTextBoxColumnStyleInfo);

		var isTestDeclarationCheckBoxColumnStyleInfo = new ZCheckBoxColumnStyleInfo();
		isTestDeclarationCheckBoxColumnStyleInfo.ColumnName = "IsTestDeclaration";
		isTestDeclarationCheckBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
		isTestDeclarationCheckBoxColumnStyleInfo.IsMandatory = false;
		MessageSendingObjectsGrid.ColumnStyles.Insert(1, isTestDeclarationCheckBoxColumnStyleInfo);
	}
}
