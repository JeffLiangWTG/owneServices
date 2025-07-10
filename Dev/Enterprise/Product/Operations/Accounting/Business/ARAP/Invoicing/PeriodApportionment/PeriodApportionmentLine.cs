using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class PeriodApportionmentLine : NonPersistentBusinessObject
	{
		public PeriodApportionmentLine(InvoicingLineBase invoicingLine, int period)
			: this(invoicingLine, period, 0)
		{
		}

		public PeriodApportionmentLine(InvoicingLineBase invoicingLine, int period, decimal osAmount, decimal osTaxNotRecoverable = decimal.Zero)
			: base(invoicingLine.Factory)
		{
			this.period = period;
			this.manager = invoicingLine.PeriodApportionment;
			companyExchangeRate = manager.CompanyExchangeRate;
			InvoicingLine = invoicingLine;
			periodCalculator = InvoicingLine.InvoiceBase.PeriodCalculator;
			this.osAmount = osAmount;
			this.osTaxNotRecoverable = osTaxNotRecoverable;

			Decimals = invoicingLine.Decimals;

			invoicingLine.RegisterEditableChildObject(this);
		}

		public int Decimals { get; set; }

		public InvoicingLineBase InvoicingLine { get; }

		public bool IsFirstLine => Period == periodCalculator.GetPeriodFromDate(InvoicingLine.PeriodStartDate);
		public bool IsLastLine => Period == periodCalculator.GetPeriodFromDate(InvoicingLine.PeriodEndDate);

		readonly PeriodApportionmentManager manager;
		readonly int period;
		readonly AccountingPeriodCalculator periodCalculator;
		readonly ZArchitecture.Environment.ExchangeRate companyExchangeRate;

		public RefCurrency Currency => InvoicingLine.TransactionCurrency;
		public AccGLHeader PeriodClearingGLAccount => InvoicingLine.PeriodClearingGLAccount;
		public AccGLHeader ControlAccount => InvoicingLine.GLHeader;

		#region Properties

		[ResourceStringData("D1EC60FE-2FE3-4A69-ADA3-BE0A81C3AA7B", ShortCaption = "Per.", Caption = "Period")]
		public ZInt Period => period;
		public ZPropertyInfo PeriodInfo => GetZPropertyInfo(nameof(Period));

		[ResourceStringData("475756AD-D0F1-4194-B081-4A16657DBFC7", ShortCaption = "Start", Caption = "Period Start")]
		public ZDateTime PeriodStart => periodCalculator.GetFirstDayForPeriod(Period);
		public ZPropertyInfo PeriodStartInfo => GetZPropertyInfo(nameof(PeriodStart));

		[ResourceStringData("C3A9EB13-CA70-480F-B7A6-E56363CD71C8", ShortCaption = "End", Caption = "Period End")]
		public ZDateTime PeriodEnd => periodCalculator.GetLastDayForPeriod(Period);
		public ZPropertyInfo PeriodEndInfo => GetZPropertyInfo(nameof(PeriodEnd));

		[ResourceStringData("54F58FAF-D7D1-42E0-9C9C-3AF5E57A612F", ShortCaption = "# Days", Caption = "Days in Period", MediumCaption = "Number of Days in Period")]
		public ZInt NumberOfDaysInPeriod => IsFirstLine
			? (PeriodEnd.Date - InvoicingLine.PeriodStartDate).Days + 1
			: IsLastLine
				? (InvoicingLine.PeriodEndDate - PeriodStart.Date).Days + 1
				: (PeriodEnd.Date - PeriodStart.Date).Days + 1;

		public ZPropertyInfo NumberOfDaysInPeriodInfo => GetZPropertyInfo(nameof(NumberOfDaysInPeriod));

		[ResourceStringData("DF28C66E-BD18-48EF-A51C-FCA5CE146BA9", ShortCaption = "Cur.", Caption = "Currency")]
		public ZString CurrencyCode => InvoicingLine.TransactionCurrency?.RX_Code ?? ZString.Empty;
		public ZPropertyInfo CurrencyCodeInfo => GetZPropertyInfo(nameof(CurrencyCode));

		[ReadOnlyMember(nameof(OSAmount_ReadOnly))]
		[ResourceStringData("F113C3D4-496D-410C-8936-9CAD1A1873F9", Caption = "OS Amount")]
		public ZDecimal OSAmount
		{
			get => osAmount;
			set
			{
				if (!IsLastLine && osAmount != value)
				{
					if (InvoicingLine.AL_OSExTaxAmount != 0)
					{
						var taxValue = value * (InvoicingLine.AL_OSTaxAmount_NotRecoverable / InvoicingLine.AL_OSExTaxAmount);
						osTaxNotRecoverable = decimal.Round(taxValue, Decimals);
					}
					else
					{
						osTaxNotRecoverable = 0;
					}
				}

				SetNonPersistentPropertyValue(OSAmountInfo, ref osAmount, value);
				if (!IsLastLine)
				{
					manager.UpdateLastLine();
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateOSAmount();
				}
			}
		}
		ZDecimal osAmount;

		public ZPropertyInfo OSAmountInfo => GetZPropertyInfo(nameof(OSAmount));
		public ZBool OSAmount_ReadOnly => IsLastLine || InvoicingLine.PeriodApportionmentMethod != PeriodApportionmentMethods.Codes.Manual;

		[ResourceStringData("3D79ECAF-D02A-4C94-A1E8-58058564984F", ShortCaption = "Ex. Rate", Caption = "Exchange Rate")]
		public ZDecimal ExchangeRate => IsLastLine ? (ZDecimal)companyExchangeRate.GetRate(LocalAmount, OSAmount) : InvoicingLine.AL_ExchangeRate;

		public ZPropertyInfo ExchangeRateInfo => GetZPropertyInfo(nameof(ExchangeRate));

		[ReadOnly(true)]
		[ResourceStringData("6B8DCE4E-9F5D-4794-8C28-0633F6ED67BA", Caption = "Local Amount")]
		public ZDecimal LocalAmount
		{
			get => IsLastLine ? localAmount : (ZDecimal)companyExchangeRate.ForeignToLocal(OSAmount, ExchangeRate);
			set => SetNonPersistentPropertyValue(LocalAmountInfo, ref localAmount, value);
		}
		ZDecimal localAmount;

		public ZPropertyInfo LocalAmountInfo => GetZPropertyInfo(nameof(LocalAmount));

		[DecimalPlaces(nameof(Decimals))]
		[ReadOnly(true)]
		[ResourceStringData("92D8086E-36F5-4FE3-84D3-D5110B351486", ShortCaption = "OS Tax NR", Caption = "OS Tax Not Rec.", MediumCaption = "OS Tax Not Recoverable")]
		public ZDecimal OSTaxNotRecoverable
		{
			get => osTaxNotRecoverable;
			set
			{
				SetNonPersistentPropertyValue(OSTaxNotRecoverableInfo, ref osTaxNotRecoverable, value);
				if (!IsLastLine)
				{
					manager.UpdateLastLine();
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateOSTaxNotRecoverable();
				}
			}
		}
		ZDecimal osTaxNotRecoverable;

		public ZPropertyInfo OSTaxNotRecoverableInfo => GetZPropertyInfo(nameof(OSTaxNotRecoverable));

		[DecimalPlaces(nameof(Decimals))]
		[ReadOnly(true)]
		[ResourceStringData("23B8765E-4532-4FB7-95E5-BFD1A680EE25", ShortCaption = "Local Tax NR", Caption = "Local Tax Not R.", MediumCaption = "Local Tax Not Recoverable")]
		public ZDecimal LocalTaxNotRecoverable
		{
			get => IsLastLine ? localTaxNotRecoverable : (ZDecimal)companyExchangeRate.ForeignToLocal(OSTaxNotRecoverable, ExchangeRate);
			set => SetNonPersistentPropertyValue(LocalTaxNotRecoverableInfo, ref localTaxNotRecoverable, value);
		}
		ZDecimal localTaxNotRecoverable;

		public ZPropertyInfo LocalTaxNotRecoverableInfo => GetZPropertyInfo(nameof(LocalTaxNotRecoverable));

		#endregion

		#region Validation

		public PeriodApportionmentLineValidation Validation => new PeriodApportionmentLineValidation(this);

		#endregion
	}
}
