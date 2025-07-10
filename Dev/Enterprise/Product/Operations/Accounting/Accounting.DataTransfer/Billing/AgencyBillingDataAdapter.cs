using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer
{
	public class AgencyBillingDataAdapter : BillingDataAdapter, Accounting.Integration.IAgencyBillingDataAdapter
	{
		enum PrePaidCollect
		{
			Unknown,
			Hidden,
			Prepaid,
			Collect,
		}

		#region Export

		protected override bool ShouldExportCharge(Charge charge, INotifications notifications)
		{
			bool chargesALL = false;
			bool chargesPPO = false;

			switch (SystemDataRegistry.Instance.IncludeBillingInfoInAgencyXMLFile.Value)
			{
				case Constants.IncludeBillingInfoInXMLMethod.Codes.All:
					chargesALL = true;
					break;

				case Constants.IncludeBillingInfoInXMLMethod.Codes.PrincipalOnly:
					chargesPPO = true;
					break;
			}

			bool result = (chargesALL || chargesPPO && charge.ChargeCode.AC_ChargeOtherGroups == ChargeOtherGroupsList.Codes.Principal);

			if (result)
			{
				PrePaidCollect prepaidCollect = PrepaidCollectFromInvoiceType(charge.JR_InvoiceType);

				if (prepaidCollect == PrePaidCollect.Unknown)
				{
					notifications.Notify(new WarningNotification(WarningType.Warning,
						Res.GetString("5335e465-4f82-4f5f-b528-e3735eef1e54", "unable to determine the prepaid/collect status of a charge from the invoice type '{0}'. Ignoring.",
						charge.JR_InvoiceType.IsEmpty)));

					result = false;
				}

				if (prepaidCollect == PrePaidCollect.Hidden)
				{
					result = false;
				}
			}

			return result;
		}

		protected override void ExportCollect(Charge charge, Xsd.ChargeLine line)
		{
			PrePaidCollect prepaidCollect = PrepaidCollectFromInvoiceType(charge.JR_InvoiceType);

			line.Collect = (prepaidCollect == PrePaidCollect.Collect);
			line.CollectSpecified = true;
		}

		protected override void CollectExchangeRates(IDictionary<string, Xsd.ExchangeRate> list, Job job)
		{
			AgencyShipment shipment;
			JobSailing sailing;
			JobVoyage voyage;

			if ((shipment = job.Parent as AgencyShipment) != null && (sailing = shipment.Sailing) != null && (voyage = sailing.Voyage) != null)
			{
				foreach (VoyageExRate rate in voyage.ExRates)
				{
					if (rate.E8_VoyageExchangeRate > 0)
					{
						list[rate.E8_RX_NKExCurrency] = new Xsd.ExchangeRate()
						{
							CurrencyCode = rate.E8_RX_NKExCurrency,
							Value = rate.E8_VoyageExchangeRate
						};
					}
				}
			}

			base.CollectExchangeRates(list, job);
		}

		protected override Xsd.Billing NewBilling()
		{
			return new Xsd.BillingWithExchangeRates();
		}

		PrePaidCollect PrepaidCollectFromInvoiceType(string invoiceType)
		{
			switch (invoiceType)
			{
				case AgencyInvoiceTypesList.Codes.LocalPrePaid:
				case AgencyInvoiceTypesList.Codes.ForeignPrePaid:
					return PrePaidCollect.Prepaid;

				case AgencyInvoiceTypesList.Codes.LocalCollect:
				case AgencyInvoiceTypesList.Codes.ForeignCollect:
					return PrePaidCollect.Collect;

				case AgencyInvoiceTypesList.Codes.Misc:
				case AgencyInvoiceTypesList.Codes.DoNotPost:
					return PrePaidCollect.Hidden;

				default:
					return PrePaidCollect.Unknown;
			}
		}

		#endregion

		#region Import

		protected override void ImportCollect(Job job, Charge charge, Xsd.ChargeLine line)
		{
			charge.JR_InvoiceType = InvoiceTypeFromPrePaidCollect(job, charge, line.Collect);

			if (!line.Creditor.IsSpecified)
			{
				var creditor = job.PlugInData.InvoicingSupporter.GetDefaultCreditor(new DefaultCreditorSetting(charge.ChargeCode, charge.JR_InvoiceType));
				charge.JR_OH_CostAccount = creditor != null ? creditor.PK : ZGuid.Empty;
			}
		}

		string InvoiceTypeFromPrePaidCollect(Job job, Charge charge, bool isCollect)
		{
			return isCollect
				? LocalOrForeignCode(charge, job.Company, job.Destination, AgencyInvoiceTypesList.Codes.LocalCollect, AgencyInvoiceTypesList.Codes.ForeignCollect)
				: LocalOrForeignCode(charge, job.Company, job.Origin, AgencyInvoiceTypesList.Codes.LocalPrePaid, AgencyInvoiceTypesList.Codes.ForeignPrePaid);
		}

		string LocalOrForeignCode(Charge charge, GlbCompany company, RefCountry country, string localCode, string foreignCode)
		{
			if (country == null)
			{
				return "";
			}
			else
			{
				var postingStyle = new InvoiceTypeCalculator(charge).GetInvoicePostingStyle();
				if (postingStyle != null && AgencyConsumerType.IsUsingForeignPostingStyle(postingStyle))
				{
					var currency = charge.SellCurrency;
					if (currency != null && currency.RX_Code != company.GC_RX_NKLocalCurrency)
					{
						return foreignCode;
					}
				}

				return localCode;
			}
		}

		#endregion
	}
}
