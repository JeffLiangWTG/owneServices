#if NET
using System.Linq;
using System.Windows.Forms;
#endif
using CargoWise.EntityFramework;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI;

public partial class SumARegisterForm : ZTemplateForm, ICustomerServiceMenuSectionCodeOverridable
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
			var caption = Res.GetString("5ecdedee-cdcf-4c41-9145-b3fec27c5914", "SumA Register Form");
			var reference = RegHeader?.SRH_Reference;
			if (!string.IsNullOrEmpty(reference))
			{
				caption += " ";
				caption += Res.GetString("a1925786-d74d-48d3-a77d-fc0ef4ac6b9d", "- {0}", reference);
			}
			return caption;
		}
	}

	protected override ContinueWithSave ShowPreSaveDialogs()
	{
		var result = base.ShowPreSaveDialogs();

		if (RegHeader.CusTempStorageRegLines.Count == 0)
		{
			Globals.Message.ShowError(Res.GetString("24ed1354-d56c-4bfa-9253-4b9fa4e49c07", "At least one line should be entered."));
			return ContinueWithSave.No;
		}

		if (result == ContinueWithSave.Yes && RegHeader.CusTempStorageRegLines.SelectMany(x => x.CusTempStorageRegLineTransactions).Any(t => !t.IsInDatabase))
		{
			var message = Res.GetString("c22e0678-949b-43eb-b4a1-8bf8d760b512", "Transactions cannot be amended once saved. Do you want to continue saving the transactions?");
			var caption = Res.GetString("aed67986-0557-4844-8fa8-6d77d8239db7", "Warning: Transactions cannot be amended");
			var dialogResult = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
			return dialogResult == DialogResult.Yes ? ContinueWithSave.Yes : ContinueWithSave.No;
		}

		return result;
	}

	//This is specified purposefully in order to avoid Module Registry. Country requiring this module, can register this module under a specific section depending on their requirements.
	string ICustomerServiceMenuSectionCodeOverridable.SectionCode => (NoResString)"NONE";
}
