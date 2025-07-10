using System.ComponentModel;
using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.TaxFramework.Business
{
	public class AccTaxGLMovement : AutoAccTaxGLMovement
	{
		public AccTaxGLMovement(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ZDate ATM_Date
		{
			get => base.ATM_Date;
			set
			{
				base.ATM_Date = value;

				ATM_Period = new AccountingPeriodCalculator(Factory).GetPeriodFromDate(ATM_Date, TaxTransaction.Company.PK);
			}
		}

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public override ZDecimal ATM_Amount { get => base.ATM_Amount; set => base.ATM_Amount = Utilities.Round(value, LocalCurrencyDecimals); }

		[RelatedBusinessObject(nameof(TaxTransaction))]
		public override ZGuid ATM_ATT_TaxTransaction { get => base.ATM_ATT_TaxTransaction; set => base.ATM_ATT_TaxTransaction = value; }

		public AccTaxTransaction TaxTransaction => Factory.Load<AccTaxTransaction>(ATM_ATT_TaxTransaction);

		int LocalCurrencyDecimals => TaxTransaction.Company.GetLocalDecimals();

		public override void Delete()
		{
			if (!IsInDatabase)
			{
				base.Delete();
			}
			else
			{
				var message = new ZStringBuilder();
				message.Append((NoResString)"GL Movement in database cannot be deleted.");
				message.Append(this.GetAllPropertyValues());
				ErrorReporter.ReportOnce("AccTaxGLMovementInDBCannotBeDeleted", message.ToStringWithNewLineBetweenAppends());
			}
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			if (ATM_ATT_TaxTransaction.IsEmpty)
			{
				var taxRecord = Factory.NewWithValidTestData<AccTaxTransaction>();
				taxRecord.ATT_Basis = TaxBasisList.Posting.Code;
				ATM_ATT_TaxTransaction = taxRecord.PK;
			}

			base.FillWithValidTestDataCore(kind, propertyPath);

			ATM_Amount = 1M;
			ATM_Period = 202001;
			ATM_Type = TaxGLMovementTypeList.Normal.Code;
		}
#endif
	}
}
