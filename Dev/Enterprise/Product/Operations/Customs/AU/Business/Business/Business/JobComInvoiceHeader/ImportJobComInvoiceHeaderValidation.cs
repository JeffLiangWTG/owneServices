using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ImportJobComInvoiceHeaderValidation : JobComInvoiceHeaderValidation
	{
		public ImportJobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		protected override void CheckJZ_ValuationDateOverride()
		{
			base.CheckJZ_ValuationDateOverride();
			if (Parent.JZ_ValuationDateOverride.IsValid && Parent.JZ_ValuationDateOverride.Date > ZDateTime.Today)
			{
				Parent.JZ_ValuationDateOverrideInfo.AddMessageError("The valuation date override cannot be in the future.");
			}
			if (Parent.JobDeclaration.IsExWarehouse)
			{
				ExWarehouseValuationDateChecks();
			}
		}

		internal const string ValuationDateIsRequired = "A valuation date override is required on ExWarehouse entries when foreign currency amounts are used on lines.";
		internal const string AUDIsRequired = "This ExWarehouse Entry contains lines with different WRNs and foreign currency amounts have been used. These amounts will all be converted to AUD using the rate for the date specified in the valuation date override. This is probably wrong since different dates probably apply to different lines. It is recommended that you only use AUD amounts on lines under these circumstances.";
		void ExWarehouseValuationDateChecks()
		{
			bool foreignCurrencyUsed = false;
			bool multipleWRNs = false;
			ZString wRN = string.Empty;
			foreach (JobComInvoiceLine invoiceLine in Parent.InvoiceLines)
			{
				if (!foreignCurrencyUsed && ((!invoiceLine.AddInfo.TILVMoney.IsEmpty &&
							 invoiceLine.AddInfo.TILVMoney.Currency.Code != Core.Constants.CurrencyCodes.Australia) ||
							(!invoiceLine.JI_OverseasFreight.IsEmpty &&
							 invoiceLine.JI_OverseasFreight.Currency.Code != Core.Constants.CurrencyCodes.Australia) ||
							(!invoiceLine.JI_OverseasInsurance.IsEmpty &&
							 invoiceLine.JI_OverseasInsurance.Currency.Code != Core.Constants.CurrencyCodes.Australia)))
				{
					foreignCurrencyUsed = true;
				}
				if (!multipleWRNs && !invoiceLine.AddInfo.ZA_WRN.IsEmpty)
				{
					if (wRN.IsEmpty)
					{
						wRN = invoiceLine.AddInfo.ZA_WRN;
					}
					else if (wRN != invoiceLine.AddInfo.ZA_WRN)
					{
						multipleWRNs = true;
					}
				}
				if (foreignCurrencyUsed && multipleWRNs)
				{
					break;
				}
			}
			if (foreignCurrencyUsed)
			{
				if (multipleWRNs)
				{
					Parent.JZ_ValuationDateOverrideInfo.AddMessageError(AUDIsRequired);
				}
				if (Parent.JZ_ValuationDateOverride.IsEmpty)
				{
					Parent.JZ_ValuationDateOverrideInfo.AddMessageError(ValuationDateIsRequired);
				}
			}
		}

		protected override void CheckJZ_Weight()
		{
			base.CheckJZ_Weight();
			if (!Parent.JobDeclaration.IsExWarehouse)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_WeightInfo);
			}
		}

		protected override void CheckJZ_WeightUQ()
		{
			base.CheckJZ_WeightUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.JZ_WeightUQInfo, Parent.Lookups.JZ_WeightUQ_List);
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_WeightUQInfo);
		}

		protected override void CheckJZ_OH_Buyer()
		{
			base.CheckJZ_OH_Buyer();

			if (!Parent.JZ_OH_Buyer.IsEmpty)
			{
				Parent.JZ_OH_BuyerInfo.AddWarning("The Importer on the declaration tab will be used in the message NOT the Importer entered on the Invoice Header tab.");
			}
		}

		protected override void CheckJZ_OH_Supplier()
		{
			base.CheckJZ_OH_Supplier();

			if (Parent.JobDeclaration == null || !Parent.JobDeclaration.IsExWarehouse)
			{
				if (Parent.Supplier != null)
				{
					var customsClientID = Parent.Supplier.GetCustomsClientID(Parent.SupplierAddress);
					if (customsClientID.IsEmpty)
					{
						Parent.JZ_OH_SupplierInfo.AddMessageError("The INVOICE HEADER Supplier must have a Customs Client ID configured. Press F3 to edit masterfiles and modify the CID on the config tab.");
					}
					else if (customsClientID.Length != 11)
					{
						Parent.JZ_OH_SupplierInfo.AddMessageError("The Customs Client ID must be 11 characters long. Press F3 to edit masterfiles and modify the CID on the config tab.");
					}
				}
				else
				{
					Parent.JZ_OH_SupplierInfo.AddMessageError("Without a supplier, Customs will likely reject your declaration with error 'ID0417 - SUPPLIER ID IS MANDATORY'");
				}
			}
		}

		protected override void CheckJZ_Calc_TNI()
		{
			base.CheckJZ_Calc_TNI();

			JobDeclaration declaration = Parent.JobDeclaration;

			if (Parent.JZ_Calc_TNI <= 0m && declaration != null && !declaration.IsExWarehouse)
			{
				Parent.JZ_Calc_TNIInfo.AddMessageError(ZeroTNIWarning);
			}

			if (!Parent.AddInfo.ZA_TILV.IsEmpty)
			{
				foreach (JobComInvoiceHeader invoice in Parent.JobDeclaration.Invoices)
				{
					if (invoice.AddInfo.ZA_TILV.IsEmpty && invoice.JZ_Calc_TNI > 0m)
					{
						Parent.JZ_Calc_TNIInfo.AddMessageError(AllInvoicesShouldHaveTILV);
						break;
					}
				}

				if (declaration != null && !declaration.ApportionmentDirty && Parent.AddInfo.TILVMoney.Currency != null)
				{
					ZDecimal aggregatedTAndIInTILVCurrencyRounded = Parent.CurrencyConverter.ConvertExact(Parent.AggregatedTransportAndInsurance, Parent.AddInfo.TILVMoney.Currency).Amount.Round(2);

					if (Parent.AddInfo.TILVMoney.Amount.Round(2) != aggregatedTAndIInTILVCurrencyRounded)
					{
						Parent.JZ_Calc_TNIInfo.AddMessageError(string.Format(LineTILVsDoNotAddUpToHeaderTILV, aggregatedTAndIInTILVCurrencyRounded + " " + Parent.AddInfo.TILVMoney.Currency.Code));
					}
				}
			}
		}

		protected override void CheckJZ_InvoiceNumber()
		{
			base.CheckJZ_InvoiceNumber();

			JobDeclaration declaration = Parent.JobDeclaration;

			if (declaration.AddInfo.ZA_UPEIndicator_Hidden)
			{
				bool upeTariffFound = Parent.InvoiceLines.Cast<JobComInvoiceLine>().
					FirstOrDefault(x => x.JI_Tariff == "9999.40.15 41") != null;

				if (!upeTariffFound)
				{
					Parent.JZ_InvoiceNumberInfo.AddMessageError("There must be at least ONE tariff line entered that has a Tariff Classification of 9999 4015 and the statistical code of 41.");
				}
			}
		}

		protected override INotificationType NotificationTypeForZeroFreightAndInsuranceToCIF
		{
			get
			{
				INotificationType result = base.NotificationTypeForZeroFreightAndInsuranceToCIF;

				if (Parent.JobDeclaration != null && Parent.JobDeclaration.JE_MessageType == JobMessageTypeList.Codes.Import)
				{
					result = CargoWise.EntityFramework.NotificationType.MessageError;
				}
				return result;
			}
		}

		public const string AllInvoicesShouldHaveTILV = "You have overriden Transport & Insurance for this invoice. Please override for the other invoices, too.";
		public const string ZeroTNIWarning = "T & I cannot be calculated, this may lead to the declaration going red-line. Please enter Overseas Freight and Insurance amounts.";
		public const string LineTILVsDoNotAddUpToHeaderTILV = @"The total T&I of lines, {0} does not add up this amount.";
		#region Implementation

		protected override bool IncoTermRequired
		{
			get { return Parent.JobDeclaration == null || !Parent.JobDeclaration.IsExWarehouse; }
		}

		protected override TypeOfValidationForMissingMandatoryChargesForIncoterm ValidationForMissingMandatoryCharges
		{
			get
			{
				TypeOfValidationForMissingMandatoryChargesForIncoterm result = base.ValidationForMissingMandatoryCharges;
				if (IncoTermRequired)
				{
					result = TypeOfValidationForMissingMandatoryChargesForIncoterm.MessageError;
				}
				return result;
			}
		}

		#endregion
	}
}
