using Enterprise.Customs.ES.TemporaryStorage.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI;
public partial class UpdateCSVClearanceForm : ZChildForm
{
	public UpdateCSVClearanceForm(CsvCodeInfo updateCSVClearance)
		: base(updateCSVClearance)
	{
	}

	protected override void InitialiseForm()
	{
		base.InitialiseForm();
		InitializeComponent();
		InitializeUpdateCSVClearanceLayout();
	}

	void InitializeUpdateCSVClearanceLayout()
	{
		MessageLabel.Text = Res.GetString("249b7d36-378e-4e46-8c0e-655e7e1d8a10", @"You are about to change the clearance number.
Please make sure that the number that you are entering is the right clearance number.");
		CSVClearance.MaxLength = 16;
	}

	public override string FormVerb => string.Empty;
	public override string FormCaption => Res.GetString("08492c72-b824-4b66-8449-4193289a8c0c", "Update CSV Clearance");
}
