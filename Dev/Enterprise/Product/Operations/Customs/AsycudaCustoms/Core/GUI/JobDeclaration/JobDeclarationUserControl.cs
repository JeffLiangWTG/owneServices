using CargoWise.Windows.UI;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	public partial class JobDeclarationUserControl : BaseCustomsDeclarationUserControl
	{
		public JobDeclarationUserControl()
		{
			InitializeComponent();
			JE_MessageSubTypeBoundDropDownEdit.SetBindingMember("JE_DeclarationType");
			JE_MessageSubTypeBoundDropDownEdit.BindToList = "CusEntryInstruction.Lookups.StyleList";
		}

		protected override void HandleDeclarationControlVisibilityChangedCore()
		{
			base.HandleDeclarationControlVisibilityChangedCore();
			var declaration = (JobDeclaration)JobDeclaration;
			JE_MessageSubTypeBoundDropDownEdit.Visible = !declaration.AreMultipleEntryInstructionsAllowed;
		}

		protected override Customs.GUI.NumbersUserControl GetNumbersUserControl() => new NumbersUserControl();
	}
}
