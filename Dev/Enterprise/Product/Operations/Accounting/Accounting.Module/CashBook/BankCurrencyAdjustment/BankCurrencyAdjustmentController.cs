using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.CashBook.ExchangeDifference;
using Enterprise.Accounting.GUI.CashBook;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class BankCurrencyAdjustmentController : AccountingTransactionController
	{
		public BankCurrencyAdjustmentController()
		{
		}
		protected override IZForm GetFormCore(IBusiness businessEntity)
		{
			var business = businessEntity as NewCashbookExchangeDiffHeader;
			if (business != null)
			{
				return new NewCashBookExchangeDiffForm(business);
			}
			else
			{
				return new CashBookExchangeDiffForm(businessEntity as CashbookExchangeDiff);
			}
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ReverseCashBookBankCurrencyAdjustment; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ViewCashBookBankCurrencyAdjustment; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.NewCashBookBankCurrencyAdjustment; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ViewCashBookBankCurrencyAdjustment; }
		}

		protected override ControllerID IDCore
		{
			get { return ControllerIDs.BankCurrencyAdjustment; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.CashbookTransaction; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(NewCashbookExchangeDiffHeader); }
		}
	}
}
