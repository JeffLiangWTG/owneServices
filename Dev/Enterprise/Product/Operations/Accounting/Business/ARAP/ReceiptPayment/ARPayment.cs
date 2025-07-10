using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	public class ARPayment : Payment, IEDocsParsingSupport
	{
		public ARPayment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Implementation

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("dcbb36f0-2a48-4f87-803a-73fc8a2a89e5", "Accounts Receivable Payment"); }
		}

		protected override ZString Ledger
		{
			get { return ZArchitecture.Core.LedgerTypes.AccountsReceivable; }
		}

		protected override void SetDefaultBankAccount()
		{
			SetDefaultBankAccountAR();
		}

		public override ZString ExchangeRateType
		{
			get { return Core.Constants.ExchangeRateTypes.Code.SellRate; }
		}

		#endregion

		#region IDocManagerSupport Members

		public override DocManagerInfo DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new PaymentDocManagerInfo(this, Core.Constants.DocManagerCodes.ReceivablePayment)); }
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
