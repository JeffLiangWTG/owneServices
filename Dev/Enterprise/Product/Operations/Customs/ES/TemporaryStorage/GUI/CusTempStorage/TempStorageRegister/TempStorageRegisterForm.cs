using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI;

public partial class TempStorageRegisterForm : ZTemplateForm
{
	public TempStorageRegisterForm()
	{
		InitializeComponent();
	}

	public TempStorageRegisterForm(CusTempStorageRegHeader header)
		: base(header)
	{
	}

	CusTempStorageRegHeader RegHeader => (CusTempStorageRegHeader)BusinessEntity;

	public override string FormCaption
	{
		get
		{
			var caption = Res.GetString("D33F01B1-EC89-47A2-82ED-0CED84AA1DF1", "Temporary Storage Register");
			var reference = RegHeader?.SRH_Reference;
			if (!string.IsNullOrEmpty(reference))
			{
				caption += string.Format(" - {0}", reference);
			}
			return caption;
		}
	}

	protected override ContinueWithSave ShowPreSaveDialogs()
	{
		var result = base.ShowPreSaveDialogs();

		if (result == ContinueWithSave.Yes && RegHeader.CusTempStorageRegLines.SelectMany(x => x.CusTempStorageRegLineTransactions).Any(t => !t.IsInDatabase))
		{
			if (result == ContinueWithSave.Yes)
			{
				var message = Res.GetString("837C464C-F890-46C8-A27C-21D5A0BDAF07", "Transactions cannot be amended once saved. Do you want to continue saving the transactions?");
				var caption = Res.GetString("C3D46578-A61C-4DAD-8B45-43676DF757FC", "Warning: Transactions cannot be amended");
				var dialogResult = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

				var transaction = RegHeader.CusTempStorageRegLines.SelectMany(x => x.CusTempStorageRegLineTransactions).SingleOrDefault(t => !t.IsInDatabase);
				result = dialogResult == DialogResult.Yes ? ContinueWithSave.Yes : ContinueWithSave.No;
				if (result == ContinueWithSave.Yes && transaction != null)
				{
					var writeOffResult = ZString.Empty;
					writeOffResult += EU.Business.TemporaryStorageHelper.ConfirmNewADJTransactionBeforeSaving(transaction);
					if (!writeOffResult.IsEmpty)
					{
						Globals.Message.ShowError(writeOffResult);
					}
				}
			}
		}

		return result;
	}

	protected override void Save(ITransactionParticipant[] factories)
	{
		try
		{
			base.Save(factories);
		}
		catch (Exception ex) when (!ex.IsCriticalException())
		{
			var transactions = RegHeader.CusTempStorageRegLines.SelectMany(x => x.CusTempStorageRegLineTransactions).Where(t => !t.IsInDatabase);
			foreach (var tran in transactions)
			{
				EU.Business.TemporaryStorageHelper.ResetNewADJTransactionWhenSavingError(tran);
			}

			throw;
		}
	}

	protected override bool AllowNew => false;
}
