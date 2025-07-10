using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CSARSFPaymentCollection : NonPersistentBusinessObjectCollection<CSARSFPayment>
	{
		public CSARSFPaymentCollection(CusStatementHeader cusStatementHeader, ZString paymentType)
			: base(cusStatementHeader.Factory)
		{
			this.cusStatementHeader = cusStatementHeader;
			this.paymentType = paymentType;
			Load();
		}

		public override void Load()
		{
			using (SuspendSettingHasChanges())
			{
				RemoveAll();
				var codes = CSARSFExtension.GetPaymentCodeList(Factory, paymentType);
				var line = cusStatementHeader.GetStatementLineDependsOnPaymentType(paymentType);
				foreach (CodeDescriptionPair code in codes)
				{
					Add(new CSARSFPayment(line, paymentType, code.Code, code.Description));
				}
			}
		}

		readonly ZString paymentType;

		readonly CusStatementHeader cusStatementHeader;

		protected override bool AllowNewCore => false;

		public void CalculatePayments()
		{
			var transactionLines = cusStatementHeader.StatementLines.Cast<CusStatementLine>().Where(x => x.B3_EntryType == JobMessageTypeList.Codes.Import || x.B3_EntryType == JobMessageTypeList.Codes.XTypeEntry);

			foreach (CSARSFPayment payment in this)
			{
				if (payment.IsCalculatedAutomatically())
				{
					payment.Amount = 0m;
					foreach (var line in transactionLines)
					{
						payment.Amount += line.Charges.Cast<CusStatementLineCharge>().Where(x => x.B4_ChargeType == payment.CodeID).Sum(y => y.B4_ChargeAmount);
					}
				}
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return null;
		}

		public ZDecimal TotalAmount
		{
			get
			{
				return this.Cast<CSARSFPayment>().Sum(x => x.Amount);
			}
		}
	}
}
