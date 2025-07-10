using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	[DependentBusinessObject(typeof(ClientLicenceBilling), "BillingDiscounts")]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class ClientLicenceBillingDiscount : AutoClientLicenceBillingDiscount
	{
		public ClientLicenceBillingDiscount(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Parent

		public ClientLicenceBilling Parent
		{
			get { return Factory.Load<ClientLicenceBilling>(L5_L4); }
		}

		#endregion

		#region Properties

		[List("Lookups.SystemCodes")]
		public override ZString L5_SystemCode
		{
			get { return base.L5_SystemCode; }
			set
			{
				base.L5_SystemCode = value;
			}
		}

		[List("Lookups.SubCodes")]
		public override ZString L5_SubCode
		{
			get { return base.L5_SubCode; }
			set { base.L5_SubCode = value; }
		}

		public bool L5_SubCode_ReadOnly
		{
			get { return !ClientLicenceBillingDiscountLookups.GetSystemCodesThatHaveSubCodes().Contains(L5_SystemCode); }
		}

		[List("Lookups.DiscountTypes")]
		public override ZString L5_Type
		{
			get { return base.L5_Type; }
			set
			{
				if (base.L5_Type != value)
				{
					base.L5_Type = value;
					if (!IsModuleSpecific)
					{
						L5_ModuleCode = ZString.Empty;
					}
					if (L5_BreakAmount_ReadOnly && L5_BreakAmount != ZDecimal.Zero)
					{
						L5_BreakAmount = ZDecimal.Zero;
					}
				}
			}
		}

		[List("Lookups.ModuleCodeList")]
		public override ZString L5_ModuleCode
		{
			get { return base.L5_ModuleCode; }
			set
			{
				base.L5_ModuleCode = value;
			}
		}

		public bool L5_ModuleCode_ReadOnly
		{
			get { return !IsModuleSpecific; }
		}

		[DecimalPlaces(2)]
		public override ZDecimal L5_BreakAmount
		{
			get { return IsCapped ? CalculateCappedDiscountBreakAmount() : base.L5_BreakAmount; }
			set
			{
				if (!IsCapped)
				{
					base.L5_BreakAmount = value;
				}
				else
				{
					ZDecimal currentBreakAmount = CalculateCappedDiscountBreakAmount();
					ZDecimal adjustment = value - currentBreakAmount;
					base.L5_BreakAmount = base.L5_BreakAmount + adjustment;
				}
			}
		}

		public bool L5_BreakAmount_ReadOnly
		{
			get { return IsModuleSpecific || IsSpecial || IsSurcharge || IsIncrementalVolume || IsTransactionalCommitment || IsWiseCloud; }
		}

		public bool L5_Units_ReadOnly
		{
			get { return !IsMinimumFee && !IsIncrementalVolume && !IsTransactionalCommitment; }
		}

		public bool L5_Discount_ReadOnly
		{
			get { return IsMinimumFee; }
		}

		[List("Lookups.DiscountBreakUnits")]
		public override ZString L5_BreakUnits
		{
			get { return base.L5_BreakUnits; }
			set { base.L5_BreakUnits = value; }
		}

		public bool L5_DiscountCode_ReadOnly
		{
			get { return Parent != null && LicenceCompany.StandardPricesCompany != null && LicenceCompany.StandardPricesCompany.PK != Parent.L4_LC; }
		}

		#endregion

		#region Calculated Properties & Methods

		public bool IsVolume
		{
			get { return L5_Type == BillingConstants.DiscountType.Volume; }
		}

		public bool IsIncrementalVolume
		{
			get { return L5_Type == BillingConstants.DiscountType.IncrementalVolume; }
		}

		public bool IsPrepayment
		{
			get { return L5_Type == BillingConstants.DiscountType.Prepayment; }
		}

		public bool IsCommitment
		{
			get { return L5_Type == BillingConstants.DiscountType.Commitment; }
		}

		public bool IsTransactionalCommitment
		{
			get { return IsCommitment && BillingConstants.IsTransactional(L5_SystemCode); }
		}

		public bool IsSpecial
		{
			get { return L5_Type == BillingConstants.DiscountType.Special; }
		}

		public bool IsModuleSpecific
		{
			get { return L5_Type == BillingConstants.DiscountType.ModuleSpecific; }
		}

		public bool IsCapped
		{
			get { return L5_Type == BillingConstants.DiscountType.Capped; }
		}

		public bool IsMinimumFee
		{
			get { return L5_Type == BillingConstants.DiscountType.MinimumFee; }
		}

		public bool IsSurcharge
		{
			get { return L5_Type == BillingConstants.DiscountType.Surcharge; }
		}

		public bool IsWiseCloud
		{
			get { return L5_Type == BillingConstants.DiscountType.WiseCloud; }
		}

		public bool IsDateRangeMatched(ZDateTime startDate, ZDateTime endDate)
		{
			return (L5_StartDate.IsEmpty || startDate >= L5_StartDate) && (L5_EndDate.IsEmpty || endDate <= L5_EndDate);
		}

		#endregion

		#region Calculation

		public DiscountCalculation Calculate(ZDecimal amount, ZDecimal? licenceUnitRate = null, ZDecimal? licenceUnitAmount = null)
		{
			if (IsCommitment)
			{
				return CalculateCommitment(amount, licenceUnitRate, licenceUnitAmount);
			}
			else if (IsPrepayment)
			{
				return CalculatePrepayment(amount);
			}
			else if (IsCapped)
			{
				return CalculateCapped(amount);
			}

			DiscountCalculation result = new DiscountCalculation();

			result.Amount = amount;
			result.DiscountAmount = CalculateDiscountAmount(amount);
			result.SetDiscountDetails(FullDescription(amount, result.DiscountAmount), L5_Type, result.DiscountAmount, L5_Discount);
			if (L5_Discount != 0m)
			{
				result.InvoiceDescriptions = new ZString[] { InvoiceDescription() };
			}
			return result;
		}

		DiscountCalculation CalculateCommitment(ZDecimal amount, ZDecimal? licenceUnitRate = null, ZDecimal? licenceUnitAmount = null)
		{
			DiscountCalculation result = new DiscountCalculation();
			ZDecimal amountInBreakUnits = amount;

			if (L5_BreakUnits == BillingConstants.DiscountBreakUnit.LicenceUnits)
			{
				if ((licenceUnitRate == null) || (licenceUnitAmount == null))
				{
					return result;
				}

				if (licenceUnitRate.Value == 0)
				{
					result.ErrorDescriptions = new ZString[] { "Licence unit rate is zero and there is a commitment discount in licence units." +
						" Set a licence unit rate on the price list or change the commitment to currency." };
				}

				amountInBreakUnits = (ZDecimal)licenceUnitAmount;
			}

			if (L5_Discount == 0m && amountInBreakUnits >= L5_BreakAmount)
			{
				result.Amount = amount;
				return result;
			}

			ZDecimal commitmentAsMoney = (L5_BreakUnits == BillingConstants.DiscountBreakUnit.LicenceUnits) ? (ZDecimal)(L5_BreakAmount * (ZDecimal)licenceUnitRate) : L5_BreakAmount;
			ZDecimal amountToDiscount = (L5_BreakAmount > amountInBreakUnits) ? commitmentAsMoney : amount;

			result.Amount = amountToDiscount;
			result.DiscountAmount = CalculateDiscountAmount(amountToDiscount);

			if (L5_Discount == 0m)
			{
				ZString description = BillingConstants.GetDiscountTypeList().GetDescriptionFromCode(L5_Type);
				description += " Minimum Spend: " + result.Amount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
				result.SetDiscountDetails(description, L5_Type, result.DiscountAmount, 0);
			}
			else
			{
				result.InvoiceDescriptions = new ZString[] { InvoiceDescription() };
				result.SetDiscountDetails(FullDescription(amountToDiscount, result.DiscountAmount), L5_Type, result.DiscountAmount, L5_Discount);
			}

			return result;
		}

		internal DiscountCalculation CalculateTransactionalCommitment(decimal amount)
		{
			DiscountCalculation result = new DiscountCalculation();
			result.Amount = amount;
			result.DiscountAmount = CalculateDiscountAmount(amount);
			string description = string.Concat(L5_Description, L5_Description.IsEmpty ? "" : " ",
					BillingConstants.GetDiscountTypeList().GetDescriptionFromCode(L5_Type),
					" Discount: ", result.DiscountAmount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture),
					" (Minimum ", L5_Units, " Units, ", L5_Discount, "%)");

			result.SetDiscountDetails(description, L5_Type, result.DiscountAmount, L5_Discount);
			if (L5_Discount != 0m)
			{
				result.InvoiceDescriptions = new ZString[] { InvoiceDescription() };
			}
			return result;
		}

		DiscountCalculation CalculatePrepayment(ZDecimal amount)
		{
			DiscountCalculation result = new DiscountCalculation();
			amount = Math.Max(amount, L5_BreakAmount);

			result.Amount = amount;

			ZDecimal amountToDiscount = amount - L5_BreakAmount;
			if (amountToDiscount > 0m)
			{
				result.DiscountAmount = CalculateDiscountAmount(amountToDiscount);
				result.SetDiscountDetails(FullDescription(amountToDiscount, result.DiscountAmount), L5_Type, result.DiscountAmount, L5_Discount);
				result.InvoiceDescriptions = new ZString[] { InvoiceDescription() };
			}
			else
			{
				ZString description = BillingConstants.GetDiscountTypeList().GetDescriptionFromCode(L5_Type);
				description += " Minimum Spend: " + result.Amount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
				result.SetDiscountDetails(description, L5_Type, result.DiscountAmount, 0);
			}
			return result;
		}

		DiscountCalculation CalculateCapped(ZDecimal amount)
		{
			DiscountCalculation result = new DiscountCalculation();

			ZDecimal maximumCappedAmount = Utilities.Round(L5_BreakAmount * 100m / L5_Discount, BillingConstants.RoundingDecimals);
			amount = Math.Min(amount, maximumCappedAmount);

			result.Amount = amount;
			result.DiscountAmount = CalculateDiscountAmount(amount);
			result.SetDiscountDetails(FullDescription(amount, result.DiscountAmount), L5_Type, result.DiscountAmount, L5_Discount);
			if (L5_Discount != 0m)
			{
				result.InvoiceDescriptions = new ZString[] { InvoiceDescription() };
			}
			CappedDiscountAmount = result.DiscountAmount;

			return result;
		}

		public ZDecimal CalculateDiscountAmount(ZDecimal amountToDiscount)
		{
			return Utilities.Round(amountToDiscount * L5_Discount / 100m, BillingConstants.RoundingDecimals);
		}

		public DiscountCalculation CalculateModuleSpecific(ZDecimal amount, ZString moduleDescription)
		{
			DiscountCalculation result = Calculate(amount);
			result.SetDiscountDetails(moduleDescription.TrimStart() + " " + FullDescription(amount, result.DiscountAmount), L5_Type, result.DiscountAmount, L5_Discount);
			if (L5_Discount != 0m)
			{
				result.InvoiceDescriptions = new ZString[] { InvoiceDescription() };
			}
			return result;
		}

		ZString FullDescription(ZDecimal amount, ZDecimal discountAmount)
		{
			return FullDescription(amount, discountAmount, L5_Discount);
		}

		public ZString FullDescription(ZDecimal amount, ZDecimal discountAmount, ZDecimal discount)
		{
			return string.Concat(
					L5_Description,
					L5_Description.IsEmpty ? "" : " ",
					BillingConstants.GetDiscountTypeList().GetDescriptionFromCode(L5_Type),
					IsSurcharge ? ": " : " Discount: ",
					(-discountAmount).ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture),
					" (",
					-discount,
					"% * ",
					amount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture),
					")");
		}

		ZString InvoiceDescription()
		{
			if (L5_Type == BillingConstants.DiscountType.IncrementalVolume)
			{
				return string.Concat(BillingConstants.GetDiscountTypeList().GetDescriptionFromCode(L5_Type),
					" Discount");
			}
			else if (L5_Type == BillingConstants.DiscountType.Surcharge)
			{
				return string.Concat(
					L5_Description,
					" ",
					BillingConstants.GetDiscountTypeList().GetDescriptionFromCode(L5_Type),
					" of ",
					-L5_Discount, "%");
			}
			else
			{
				return string.Concat(BillingConstants.GetDiscountTypeList().GetDescriptionFromCode(L5_Type),
					" Discount of ", L5_Discount, "%");
			}
		}

		public DiscountCalculation CalculateMinimumFee(int unitCount, ZDecimal unitPrice)
		{
			if (!IsMinimumFee)
			{
				return new DiscountCalculation();
			}

			DiscountCalculation result = new DiscountCalculation();
			ZString discountDescription = "";
			ZString overTheLimitDescription = "";
			ZDecimal amount = 0m;

			if (unitCount <= L5_Units)
			{
				amount = L5_BreakAmount;
			}
			else
			{
				int unitsOverTheLimit = unitCount - L5_Units;
				amount = L5_BreakAmount + unitsOverTheLimit * unitPrice;
				overTheLimitDescription = string.Format(CultureInfo.CurrentCulture, " + {0} Transactions @ {1} Per Transaction", unitsOverTheLimit.ToString(CultureInfo.InvariantCulture), unitPrice.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture));
			}

			discountDescription = BillingConstants.GetDiscountTypeList().GetDescriptionFromCode(L5_Type);
			if (!L5_Description.IsEmpty)
			{
				discountDescription = L5_Description + " " + discountDescription;
			}
			discountDescription += ": " + L5_BreakAmount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture) + overTheLimitDescription;

			result.Amount = Utilities.Round(amount, BillingConstants.RoundingDecimals);
			result.DiscountAmount = 0m;
			result.SetDiscountDetails(discountDescription, L5_Type, result.DiscountAmount, 0);
			if (L5_Discount != 0m)
			{
				result.InvoiceDescriptions = new ZString[] { InvoiceDescription() };
			}
			return result;
		}

		public DiscountCalculation CalculateOnDemandMinimumFee(int unitCount)
		{
			if (!IsMinimumFee)
			{
				return new DiscountCalculation();
			}

			DiscountCalculation result = new DiscountCalculation();
			ZString discountDescription = "";
			ZString overTheLimitDescription = "";
			ZDecimal amount = 0m;

			if (unitCount <= L5_Units || L5_Units == 0)
			{
				amount = L5_BreakAmount;
			}
			else
			{
				ZDecimal unitPrice = L5_BreakAmount / L5_Units;
				int unitsOverTheLimit = unitCount - L5_Units;
				amount = unitCount * unitPrice;
				overTheLimitDescription = string.Format(CultureInfo.CurrentCulture, " + {0} Users @ {1} Per User", unitsOverTheLimit.ToString(CultureInfo.InvariantCulture), unitPrice.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture));
			}

			discountDescription = BillingConstants.GetDiscountTypeList().GetDescriptionFromCode(L5_Type);
			if (!L5_Description.IsEmpty)
			{
				discountDescription = L5_Description + " " + discountDescription;
			}
			discountDescription += ": " + L5_BreakAmount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture) + overTheLimitDescription;

			result.Amount = Utilities.Round(amount, BillingConstants.RoundingDecimals);
			result.DiscountAmount = 0m;
			result.SetDiscountDetails(discountDescription, L5_Type, result.DiscountAmount, 0);
			if (L5_Discount != 0m)
			{
				result.InvoiceDescriptions = new ZString[] { InvoiceDescription() };
			}
			return result;
		}

		public DiscountCalculation CalculateIncrementalVolume(int incrementalUnitCount, ZDecimal unitPrice)
		{
			DiscountCalculation result = new DiscountCalculation();

			if (!IsIncrementalVolume)
			{
				return result;
			}

			result.Amount = Utilities.Round(incrementalUnitCount * unitPrice, BillingConstants.RoundingDecimals);
			result.DiscountAmount = CalculateDiscountAmount(result.Amount);

			string description = string.Concat(L5_Description, L5_Description.IsEmpty ? "" : " ",
					BillingConstants.GetDiscountTypeList().GetDescriptionFromCode(L5_Type),
					" Discount: ", result.DiscountAmount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture),
					" (", incrementalUnitCount, " Units @ ", L5_Discount, "%)");

			result.SetDiscountDetails(description, L5_Type, result.DiscountAmount, L5_Discount);
			if (L5_Discount != 0m)
			{
				result.InvoiceDescriptions = new ZString[] { InvoiceDescription() };
			}
			return result;
		}

		#endregion

		#region Init Date Range

		public void InitDateRange()
		{
			if (!L5_StartDate.IsEmpty || !L5_EndDate.IsEmpty || L5_Duration.IsEmpty)
			{
				return;
			}

			ZDateTime startDate = ZDateTime.Today.AddMonths(-1);
			startDate = new ZDateTime(startDate.Year, startDate.Month, 1);
			ZDateTime endDate = startDate.AddMonths(L5_Duration).AddDays(-1);

			L5_StartDate = startDate;
			L5_EndDate = endDate;
		}

		#endregion

		#region Capped Discount Amount Usage

		public ZDecimal CappedDiscountAmount { get; private set; }

		const string CappedDiscountLogHeader = "Capped Discount Used";

		public void CreateCappedDiscountUsageLog(BusinessObjectFactory invoiceFactory, ZGuid invoicePK)
		{
			if (IsCapped
				&& CappedDiscountAmount != 0m
				&& Parent != null
				&& Parent.IsInDatabase
				&& Parent.Company != null
				&& Parent.Company.Header != null)
			{
				string cappedDiscountLogText = string.Format(CultureInfo.CurrentCulture, "{0}:{1} InvoicePK:{2}",
					CappedDiscountLogHeader,
					CappedDiscountAmount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture),
					invoicePK.ToString());

				EDIOrgHeader header = invoiceFactory.Load<EDIOrgHeader>(Parent.Company.Header.PK);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				header.Logs.AddNew(Events.EditedARecord, cappedDiscountLogText);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		protected ZDecimal CalculateCappedDiscountBreakAmount()
		{
			ZDecimal result = base.L5_BreakAmount;
			if (IsCapped && Parent != null && Parent.Company != null && Parent.Company.Header != null)
			{
				ZQuery discountLogsQuery = new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, CappedDiscountLogHeader);
				StmALog[] cappedDiscountLogs = Parent.Company.Header.Logs.Find(discountLogsQuery);

				if (cappedDiscountLogs.Length > 0)
				{
					ZQuery cancelledInvoicesQuery = new ZQuery();
					cancelledInvoicesQuery.AddToFilter(AccTransactionHeaderSchema.AH_OH, Parent.Company.Header.PK);
					cancelledInvoicesQuery.AddToFilter(AccTransactionHeaderSchema.AH_IsCancelled, true);
					cancelledInvoicesQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty);

					ARInvoice[] cancelledInvoices = Factory.Load<ARInvoice>(cancelledInvoicesQuery);
					IEnumerable<ZGuid> cancelledInvoicePKs = cancelledInvoices.Select(x => x.PK);

					foreach (StmALog cappedDiscountLog in cappedDiscountLogs)
					{
						ZGuid invoicePK = GetInvoicePKFromLog(cappedDiscountLog);
						if (!cancelledInvoicePKs.Contains(invoicePK))
						{
							ZDecimal amountAlreadyUsed = GetAmountFromLog(cappedDiscountLog);
							result -= amountAlreadyUsed;
						}
					}
				}
			}

			return result;
		}

		ZDecimal GetAmountFromLog(StmALog cappedDiscountLog)
		{
			int indexOfSeparator = cappedDiscountLog.SL_Reference.IndexOf(":", StringComparison.Ordinal);
			ZString reference = cappedDiscountLog.SL_Reference.SubstringSafe(indexOfSeparator + 1);

			indexOfSeparator = reference.IndexOf(" ", StringComparison.Ordinal);
			ZString amountAsString = reference.SubstringSafe(0, indexOfSeparator);

			ZDecimal result;
			if (!ZDecimal.TryParse(amountAsString, out result))
			{
				result = 0m;
			}

			return result;
		}

		ZGuid GetInvoicePKFromLog(StmALog cappedDiscountLog)
		{
			int indexOfSeparator = cappedDiscountLog.SL_Reference.IndexOf(':', 2);
			ZString invoicePKAsString = cappedDiscountLog.SL_Reference.SubstringSafe(indexOfSeparator + 1);

			return new ZGuid(invoicePKAsString);
		}

		#endregion

		#region OnSaving

		public override void OnSaving()
		{
			base.OnSaving();
			LogChanges();
		}

		void LogChanges()
		{
			if (Parent != null && Parent.IsInDatabase && Parent.Company != null && Parent.Company.Header != null)
			{
				string logText = string.Format(CultureInfo.CurrentCulture, "{9} Discount {0} {1}% Sys={2} Typ={3}{4}{5}{6}{7}{8}",
					L5_Description.Substring(0, Math.Min(L5_Description.Length, 8)),
					L5_Discount,
					L5_SystemCode,
					L5_Type,
					L5_BreakAmount != 0 ? " Brk=" + L5_BreakAmount : "",
					L5_Units != 0 ? " Unt=" + L5_Units : "",
					!L5_ModuleCode.IsEmpty ? " Mod=" + L5_ModuleCode : "",
					L5_StartDate.IsValid ? " St=" + L5_StartDate.ToShortDateString() : "",
					L5_EndDate.IsValid ? " En=" + L5_EndDate.ToShortDateString() : "",
					IsInDatabase ? "Edit" : "Add");
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Parent.Company.Header.Logs.AddNew(Events.EditedARecord, logText);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		#endregion

		#region ReadOnly

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return !EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion
	}
}

