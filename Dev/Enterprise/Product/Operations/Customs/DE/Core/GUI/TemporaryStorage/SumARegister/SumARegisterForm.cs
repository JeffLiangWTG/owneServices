using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public partial class SumARegisterForm : ZTemplateForm
	{
		public SumARegisterForm()
		{
			InitializeComponent();
		}

		public SumARegisterForm(CusTempStorageRegHeader header)
			: base(header)
		{
		}

		CusTempStorageRegHeader RegHeader => (CusTempStorageRegHeader)BusinessEntity;

		public override string FormCaption
		{
			get
			{
				var caption = Res.GetString("8514272b-7708-4037-aa2c-cb83848b058d", "SumA Register Form");
				var reference = RegHeader?.SRH_Reference;
				if (!string.IsNullOrEmpty(reference))
				{
					caption += Res.GetString("FBCDC95E-9525-4723-9470-D4748BF6CCE6", " - {0}", reference);
				}
				return caption;
			}
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var result = base.ShowPreSaveDialogs();

			if (!RegHeader.CusTempStorageRegLines.Any())
			{
				Globals.Message.ShowError(Res.GetString("4997c511-10db-4b2b-99c3-a60807e57624", "At least one line should be entered."));
				result = ContinueWithSave.No;
			}
			else if (result == ContinueWithSave.Yes && RegHeader.CusTempStorageRegLines.SelectMany(x => x.CusTempStorageRegLineTransactions).Any(t => !t.IsInDatabase))
			{
				var message = Res.GetString("84a3c249-0bcd-4146-8d51-6458e24e6ccc", "Transactions cannot be amended once saved. Do you want to continue saving the transactions?");
				var caption = Res.GetString("5113c04a-ff89-4b5a-ae8c-72aa9461c69d", "Warning: Transactions cannot be amended");
				var dialogResult = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

				result = dialogResult == DialogResult.Yes ? ContinueWithSave.Yes : ContinueWithSave.No;
			}

			return result;
		}
	}
}
