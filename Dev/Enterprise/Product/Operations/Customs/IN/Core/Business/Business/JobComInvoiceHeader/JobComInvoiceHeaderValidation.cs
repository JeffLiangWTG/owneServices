using CargoWise.EntityFramework;

namespace Enterprise.Customs.IN.Business;

public class JobComInvoiceHeaderValidation : AutoINJobComInvoiceHeaderValidation
{
	public JobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader)
		: base(invoiceHeader)
	{
	}

	protected override string GetIncoTermIsRequiredMessage(ZPropertyInfo info)
	{
		return MandatoryValidation.YouHaveNotEnteredMessage(info.HumanReadableName);
	}

	new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;

	public override void ValidateAll()
	{
		Parent.ClearRowNotifications();
		base.ValidateAll();
		CheckMaxInvoiceLines();
	}

	void CheckMaxInvoiceLines()
	{
		var parent = Parent;
		const int maxInvoiceLines = 9999;
		if (parent.JobComInvoiceLines.Count > maxInvoiceLines)
		{
			parent.AddRowMessageError(Res.GetString("8F55121B-0583-47BD-AE89-C993B7471445", "Maximum 9999 Invoice Lines are allowed under each invoice."));
		}
	}

	protected override void CheckJZ_PaymentDays()
	{
		var parent = Parent;
		const int maxPaymentDays = 180;
		if (parent.IsExport)
		{
			MandatoryValidation.CheckNotNegative(parent.JZ_PaymentDaysInfo);
			if (parent.JZ_PaymentDays >= maxPaymentDays)
			{
				parent.JZ_PaymentDaysInfo.AddMessageError(Res.GetString("6BD1671E-7700-490F-80CB-602E5361AF7D", "Payment days should be less than {0} days.", maxPaymentDays));
			}
		}
	}

	protected override void CheckJZ_InvoiceNumber()
	{
		base.CheckJZ_InvoiceNumber();
		var parent = Parent;
		if (parent.IsExport)
		{
			MandatoryValidation.MessageErrorIfNotEntered(parent.JZ_InvoiceNumberInfo);
		}
	}

	protected override void CheckJZ_InvoiceDate()
	{
		base.CheckJZ_InvoiceDate();
		var parent = Parent;
		if (parent.IsExport && !parent.JZ_InvoiceNumber.IsEmpty)
		{
			MandatoryValidation.MessageErrorIfNotEntered(parent.JZ_InvoiceDateInfo);
		}
	}

	protected override void CheckJZ_AuthorizedEconomicOperatorRole()
	{
		base.CheckJZ_AuthorizedEconomicOperatorRole();
		var parent = Parent;
		if (parent.IsExport && parent.JZ_AuthorizedEconomicOperatorRole.IsEmpty && !parent.AuthorizedEconomicOperatorOrgPK.IsEmpty)
		{
			parent.JZ_AuthorizedEconomicOperatorRoleInfo.AddMessageError(Res.GetString("11D47D0F-AE0F-4F93-B19B-2FCD6CAB95B7", "You have not entered an AEO Role"));
		}
	}

	protected override void CheckJZ_RX_NKInvoice_Currency()
	{
		base.CheckJZ_RX_NKInvoice_Currency();
		var parent = Parent;
		var info = parent.JZ_RX_NKInvoice_CurrencyInfo;
		if (parent.IsNonStandardCurrency)
		{
			info.AddWarning(Res.GetString("34E6D0A6-83F0-4D28-8A45-E8E90F4180D5", "You have selected a Non-Standard Currency."));
			if (parent.JZ_InvoiceCurrExRate.IsEmpty)
			{
				info.AddMessageError(Res.GetString("E31FEC18-15BC-4D74-AA7C-69BF9E6FCB45", "You have not entered a valid exchange rate for selected Non-Standard currency. Please update the exchange rate under Misc. tab."));
			}
		}
	}

	protected override void CheckJZ_InvoiceCurrExRate()
	{
		base.CheckJZ_InvoiceCurrExRate();
		ValidateJZ_RX_NKInvoice_Currency();
	}

	protected override bool ShouldCheckExRates => !Parent.IsNonStandardCurrency;

	protected override void CheckJZ_GSTPaymentStatus()
	{
		base.CheckJZ_GSTPaymentStatus();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_GSTPaymentStatusInfo);
	}
}
