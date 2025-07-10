//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientInvoiceDeliveryValidation
//
//    This class should be used for overriding validation in AutoClientInvoiceDeliveryValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.Billing.Business
{
	using System;
	using System.Linq;
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Client.EDI.LicenceKeyBuilder.Business;

	public class ClientInvoiceDeliveryValidation : AutoClientInvoiceDeliveryValidation
	{
		public ClientInvoiceDeliveryValidation(AutoClientInvoiceDelivery parent) : base(parent)
		{
		}

		public new ClientInvoiceDelivery Parent
		{
			get { return (ClientInvoiceDelivery)base.Parent; }
		}

		protected override void CheckL9_UseParentPrices()
		{
			base.CheckL9_UseParentPrices();
			if (Parent.L9_UseParentPrices
				&& (Parent.L9_OH_InvoiceTo.IsEmpty || Parent.Company.LC_OH == Parent.L9_OH_InvoiceTo))
			{
				Parent.L9_UseParentPricesInfo.AddError("Other, different organization not entered.");
			}
		}

		protected override void CheckL9_SystemCode()
		{
			base.CheckL9_SystemCode();
			MandatoryValidation.CheckEntered(Parent.L9_SystemCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.L9_SystemCodeInfo);

			if (!Parent.L9_SystemCodeInfo.HasErrors() && Parent.Company != null)
			{
				foreach (ClientInvoiceDelivery invoiceDelivery in Parent.Company.InvoiceDeliveries)
				{
					if (invoiceDelivery.PK != Parent.PK && invoiceDelivery.L9_SystemCode == Parent.L9_SystemCode && invoiceDelivery.L9_ServerCode == Parent.L9_ServerCode)
					{
						Parent.L9_SystemCodeInfo.AddError("You can't have duplicate server/system.");
						break;
					}
				}
			}
		}

		protected override void CheckL9_ServerCode()
		{
			base.CheckL9_ServerCode();
			if (!Parent.L9_ServerCode.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.L9_ServerCodeInfo, Parent.GetServerCodes());
			}
		}

		protected override void CheckL9_GB_InvoicingBranch()
		{
			base.CheckL9_GB_InvoicingBranch();
			var delivery = Parent;
			if (delivery.L9_IsBilled)
			{
				var info = delivery.L9_GB_InvoicingBranchInfo;
				MandatoryValidation.CheckEntered(info);

				if (!delivery.L9_GB_InvoicingBranch.IsEmpty)
				{
					var branch = delivery.InvoicingBranch;
					if (branch != null)
					{
						bool hasChanges = info.HasChanges || !delivery.IsInDatabase;
						var regItem = EDIDataRegistry.Instance.AllowedInvoicingBranches;
						if (!regItem.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), Guid.Empty))
						{
							var msg = "Branch has not been configured for invoicing in Registry: " + regItem.Inner.Location;
							if (hasChanges)
							{
								info.AddError(msg);
							}
							else
							{
								info.AddWarning(msg);
							}
						}

						if (!delivery.L9_ServerCode.IsEmpty)
						{
							var productCode = delivery.Company?.LicDatabases.OfType<LicenceDatabase>()
								.FirstOrDefault(x => x.LD_ServerCode.EqualsIgnoringCase(delivery.L9_ServerCode))
								?.LD_Product ?? ZString.Empty;

							if (!productCode.IsEmpty)
							{
								var usageBillingSettings = EDIDataRegistry.Instance.UsageBillingSettings;
								if (!usageBillingSettings.Value.BranchRestrictions.IsInvoicingBranchAllowed(productCode, delivery.L9_GB_InvoicingBranch))
								{
									var msg = "Branch has not been configured for invoicing in Registry: " + usageBillingSettings.Inner.Location;
									if (hasChanges)
									{
										info.AddError(msg);
									}
									else
									{
										info.AddWarning(msg);
									}
								}
							}
						}

						if (!CheckInvoicingBranchWithInvoiceCurrency())
						{
							info.AddError("Cannot use the same branch if currency is not the same.");
						}
					}
				}
			}
		}

		protected override void CheckL9_RX_NKInvoiceCurrency()
		{
			base.CheckL9_RX_NKInvoiceCurrency();
			if (Parent.L9_IsBilled)
			{
				MandatoryValidation.CheckEntered(Parent.L9_RX_NKInvoiceCurrencyInfo);
				if (!Parent.L9_RX_NKInvoiceCurrency.IsEmpty)
				{
					ListValidation.ErrorIfInvalidCode(Parent.L9_RX_NKInvoiceCurrencyInfo);

					if (!CheckInvoicingBranchWithInvoiceCurrency())
					{
						Parent.L9_RX_NKInvoiceCurrencyInfo.AddError("Cannot use a different currency if the issuing branch is the same.");
					}
				}
			}
		}

		bool CheckInvoicingBranchWithInvoiceCurrency()
		{
			if (!Parent.L9_RX_NKInvoiceCurrency.IsEmpty && !Parent.L9_GB_InvoicingBranch.IsEmpty && !Parent.L9_ServerCode.IsEmpty
				&& !Parent.L9_RX_NKInvoiceCurrencyInfo.HasErrors() && !Parent.L9_GB_InvoicingBranchInfo.HasErrors() && Parent.Company != null)
			{
				foreach (var invoiceDelivery in Parent.Company.InvoiceDeliveries)
				{
					if (invoiceDelivery.PK != Parent.PK
						&& !invoiceDelivery.L9_RX_NKInvoiceCurrency.IsEmpty && !invoiceDelivery.L9_GB_InvoicingBranch.IsEmpty && !invoiceDelivery.L9_ServerCode.IsEmpty
						&& invoiceDelivery.L9_GB_InvoicingBranch == Parent.L9_GB_InvoicingBranch
						&& invoiceDelivery.L9_RX_NKInvoiceCurrency != Parent.L9_RX_NKInvoiceCurrency)
					{
						return false;
					}
				}
			}

			return true;
		}

		protected override void CheckL9_AT_TaxId()
		{
			base.CheckL9_AT_TaxId();
			if (!Parent.L9_AT_TaxId.IsEmpty)
			{
				ListValidation.ErrorIfInvalidPK(Parent.L9_AT_TaxIdInfo, Parent.Lookups.TaxIds);
			}
		}

		protected override void CheckL9_AC_SalesTaxChargeCode()
		{
			base.CheckL9_AC_SalesTaxChargeCode();

			if (!Parent.L9_AC_SalesTaxChargeCode.IsEmpty)
			{
				ListValidation.ErrorIfInvalidPK(Parent.L9_AC_SalesTaxChargeCodeInfo, Parent.Lookups.SalesTaxChargeCodes);
			}
		}
	}
}

