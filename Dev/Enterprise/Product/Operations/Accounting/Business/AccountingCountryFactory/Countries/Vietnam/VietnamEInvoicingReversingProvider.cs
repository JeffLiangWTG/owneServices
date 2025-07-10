using System;
using System.Globalization;
using CargoWise.Application;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing.FeatureConfiguration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Vietnam
{
	class VietnamEInvoicingReversingProvider : IEInvoicingReversingProvider
	{
		bool IEInvoicingReversingProvider.CanReverseInvoice(InvoicingBase transaction)
		{
			var statusCanBeReversed =
				(transaction.EInvoicingStatus == Constants.EInvoicingPivotState.Succeed && IsBeforeDisableCancellationDate)
				|| transaction.EInvoicingStatus == Constants.EInvoicingPivotState.Discarded;
			return transaction.EInvoicingStatus.IsEmpty || (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.GetValueWithoutFallback(transaction.Company.PK.ToGuid(), Guid.Empty, Guid.Empty) && statusCanBeReversed);
		}

		string IEInvoicingReversingProvider.GetPreventInvoiceReversingPrompt()
		{
			if (IsBeforeDisableCancellationDate)
			{
				return Res.GetString("5330E381-9BCB-418F-8F99-2E09706E1FBF", "Invoices can only be reversed when e-Reporting status is 'SUC' or 'DCD'");
			}
			else
			{
				return Res.GetString("448F4880-46C9-4B95-8A3F-4FDD5A7AD2D5", "Invoices can only be reversed when e-Reporting status is either 'DCD' or blank. \r\nIn compliance with Vietnam tax regulations, reversal is not allowed. If needed, please issue a Credit Note to amend the original transaction through the Job Billing module.");
			}
		}

		bool IsBeforeDisableCancellationDate
		{
			get
			{
				ZDateTime disableCancellationDate = DisableCancellationDateTimeFromFeatureControl ?? AccountingConfigurationRegistry.Instance.PreventTheVietnamElectronicInvoiceFromBeingReversed.Value;
				return ZDateTime.UtcNow < disableCancellationDate;
			}
		}

		public DateTime? DisableCancellationDateTimeFromFeatureControl
		{
			get
			{
				var featureControlManager = ObjectFactory.Get<IFeatureControlManager>();
				var settingsReader = new EInvoicingFeatureSettingsReader(Constants.CountryCodes.VietNam, featureControlManager);

				if (settingsReader.TryGetCustomProperty(DisableCancellationAttribute, out string featureControlData))
				{
					return DateTime.TryParseExact(featureControlData, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result) ? result : null;
				}

				return null;
			}
		}

		const string DisableCancellationAttribute = "DisableCancellationDateTime";
	}
}
