using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	public class APReceipt : Receipt, IDocManagerSupport, IEDocsParsingSupport
	{
		public APReceipt(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Implementation

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("20154421-a388-4e2c-a6d0-b426ff6ef19a", "Accounts Payable Receipt"); }
		}

		protected override ZString Ledger
		{
			get { return ZArchitecture.Core.LedgerTypes.AccountsPayable; }
		}

		protected override void SetDefaultBankAccount()
		{
			SetDefaultBankAccountAP();
		}

		public override ZString ExchangeRateType
		{
			get { return Core.Constants.ExchangeRateTypes.Code.BuyRate; }
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new AccountingDocManagerInfo(this, Constants.DocManagerCodes.PayableReceipt)); }
		}
		AccountingDocManagerInfo docManagerInfo;

		#endregion

		#region IEDocsParsingSupport Members

		string IEDocsParsingSupport.UtilityData => throw new NotImplementedException();

		bool IEDocsParsingSupport.DenySendForParsing(Guid docPK, string docType, string fileName)
		{
			return true;
		}

		#endregion
	}
}
