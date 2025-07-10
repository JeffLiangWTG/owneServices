using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.Accounting.GUI.CashBook.Transfer;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public partial class BankTransferController : AccountingTransactionController
	{
		public BankTransferController()
		{
		}

		protected override IZForm GetFormCore(IBusiness businessEntity)
		{
			return new BankTransferForm(businessEntity as BankTransfer);
		}

		protected override ZString CantReverseMessageBoxCaption
		{
			get { return Res.GetString("002da428-2c0e-43a3-8c18-404f5e9e5c5d", "Bank Transfer"); }
		}

		protected override ControllerID IDCore
		{
			get { return ControllerIDs.BankTransfer; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(BankTransfer); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new BankTransfer(Factory, null);
		}

		protected override IBusiness GetTopLevelBusinessObject(IBusiness sourceEntity)
		{
			IBusiness result = null;

			if (sourceEntity is BankTransfer)
			{
				result = sourceEntity;
			}
			else
			{
				result = new BankTransfer(Factory, BankTransferFromRow.LoadBankTransferFromRow((BankTransferRow)sourceEntity));
			}

			return result;
		}

		public override IZForm ShowTemplateCopyForm(BusinessObject inMemorySourceEntity)
		{
			return base.ShowTemplateCopyFormFromBase(inMemorySourceEntity);
		}

		#region SecurityCheckpoints

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ReverseCashBookBankTransfer; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ViewCashBookBankTransfer; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.NewCashBookBankTransfer; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ViewCashBookBankTransfer; }
		}

		#endregion
	}
}
