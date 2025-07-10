using Enterprise.Customs.ES.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI
{
	public partial class UpdateCSVClearanceForm : ZChildForm
	{
		public UpdateCSVClearanceForm(CsvCodeInfo updateCSVClearance)
			: base(updateCSVClearance)
		{
		}

		CsvCodeInfo CsvCodeInfo => DataSource as CsvCodeInfo;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			InitializeUpdateCSVClearanceLayout();
		}

		void InitializeUpdateCSVClearanceLayout()
		{
			SecondaryCSVNumber.Visible = !CsvCodeInfo.entryHeader.IsT2L && !CsvCodeInfo.entryHeader.IsExsSubStyle && !CsvCodeInfo.entryHeader.IsT2C;
			ThirdCSVNumber.Visible = CsvCodeInfo.entryHeader.IsExportUCC6;
			MessageLabel.Text = Res.GetString("7F45BFAF-6C1F-407E-976F-292A96C71C51", @"You are about to change the clearance number of entry {0}.
Please make sure that the number that you are entering is the right clearance number.", CsvCodeInfo.entryHeader.CH_BGMReference);
			SecondaryCSVNumber.CaptionResourceString = CsvCodeInfo.entryHeader.IsImport ? Res.GetData("A4FF8130-4458-43C9-9185-E056FA39D45E", "CSV Import Certificate") : Res.GetData("EE1472F3-CE3C-4ECE-AEF0-085FA58FFAB0", "CSV T2L");
			CSVClearance.MaxLength = 16;
			SecondaryCSVNumber.MaxLength = 16;
			ThirdCSVNumber.MaxLength = 16;
		}

		public override string FormVerb => string.Empty;
		public override string FormCaption => Res.GetString("7688F20B-24CD-4178-90C9-32D2F6A00CC4", "Update CSV Clearance");
	}
}
