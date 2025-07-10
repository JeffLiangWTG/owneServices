using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Integration.Accounting
{
	/// <summary>
	/// Lite weight view of AccTransactionHeader and related objects for e-Invoicing eligibility decider.
	/// </summary>
	/// <remarks>
	/// This is a data-only interface with 1:1 correspondence with underlying database fields.
	/// No methods are permitted; extension methods must be used.
	/// No properties may return null, except for the OriginalTransactionIfExists property; othewise an empty object or collection must be returned.
	/// Additional properties may be added if they conform to the above criteria.
	/// See ITransactionHeaderWrapper for a more complex interface.
	/// </remarks>
	public interface IEInvoicingEligibilityLiteTransaction
	{
		/// <summary>
		/// AH_GC -> GC_RN_NKCountryCode
		/// </summary>
		ZString CountryCode { get; }

		/// <summary>
		/// AH_Ledger
		/// </summary>
		ZString Ledger { get; }

		/// <summary>
		/// AH_TransactionType
		/// </summary>
		ZString TransactionType { get; }

		/// <summary>
		/// AH_ComplianceSubType
		/// </summary>
		ZString ComplianceSubType { get; }

		/// <summary>
		/// AH_TransactionReference
		/// </summary>
		ZString ComplianceNumber { get; }

		/// <summary>
		/// AH_TransactionCategory
		/// </summary>
		ZString TransactionCategory { get; }

		/// <summary>
		/// AH_TransactionNum
		/// </summary>
		ZString TransactionNumber { get; }

		/// <summary>
		///  AH_PlaceOfSupply
		/// </summary>
		ZString PlaceOfSupply { get; }

		/// <summary>
		/// AH_OH - Debtor for AR; Creditor for AP.
		/// </summary>
		IEInvoicingEligibilityLiteOrgHeader OrgHeader { get; }

		/// <summary>
		/// AH_GB -> GB_OH_OrgProxy
		/// </summary>
		IEInvoicingEligibilityLiteOrgHeader BranchOrgProxy { get; }

		/// <summary>
		/// AH_GC -> GC_OH_OrgProxy
		/// </summary>
		IEInvoicingEligibilityLiteOrgHeader CompanyOrgProxy { get; }

		/// <summary>
		/// AH_GC
		/// </summary>
		ZGuid CompanyPK { get; }

		/// <summary>
		/// AH_GB
		/// </summary>
		ZGuid BranchPK { get; }

		/// <summary>
		/// AH_InvoiceDate
		/// </summary>
		ZDateTime InvoiceDate { get; }

		/// <summary>
		/// AH_InvoiceAmount
		/// </summary>
		ZDecimal InvoiceAmount { get; }

		/// <summary>
		/// AH_IsCancelled
		/// </summary>
		ZBool IsCancelled { get; }

		/// <summary>
		/// AH_GovernmentAllocatedID
		/// </summary>
		ZString GovernmentAllocatedID { get; }

		/// <summary>
		/// AccTransactionHeader.GetOriginalTransactionIfTransactionIsReversed()
		/// This may exceptionally return null
		/// </summary>
		IEInvoicingEligibilityLiteTransaction OriginalTransactionIfExists { get; }

		/// <summary>
		/// AL_AH
		/// </summary>
		IReadOnlyCollection<IEInvoicingEligibilityLiteTransactionLine> Lines { get; }

		/// <summary>
		/// AH_OA_InvoiceAddressOverride
		/// </summary>
		IEInvoicingEligibilityLiteOrgAddress InvoiceOrgAddressOverride { get; }
	}

	/// <summary>
	/// OrgHeader
	/// </summary>
	public interface IEInvoicingEligibilityLiteOrgHeader
	{
		/// <summary>
		/// OH_Category
		/// </summary>
		ZString Category { get; }

		IEInvoicingEligibilityLiteOrgAddress MainAddress { get; }

		/// <summary>
		/// AH_OH / GB_OH_OrgProxy / GC_OH_OrgProxy -> OK_OH
		/// </summary>
		IReadOnlyCollection<IEInvoicingEligibilityLiteRegistrationCode> RegistrationCodes { get; }
	}

	/// <summary>
	/// OrgAddress
	/// </summary>
	public interface IEInvoicingEligibilityLiteOrgAddress
	{
		/// <summary>
		/// OA_RN_NKCountryCode
		/// </summary>
		ZString CountryCode { get; }
	}

	public interface IEInvoicingEligibilityLiteRegistrationCode
	{
		/// <summary>
		/// OK_RN_NKCodeCountry
		/// </summary>
		ZString CountryCode { get; }

		/// <summary>
		/// OK_CodeType
		/// </summary>
		ZString CodeType { get; }

		/// <summary>
		/// OK_CustomsRegNo
		/// </summary>
		ZString RegistrationNumber { get; }
	}

	/// <summary>
	/// AccTransactionLine
	/// </summary>
	public interface IEInvoicingEligibilityLiteTransactionLine
	{
		/// <summary>
		/// AL_AC / AL_AG
		/// </summary>
		ZGuid ChargePK { get; }

		/// <summary>
		/// AL_AC -> / AC_Code
		/// </summary>
		ZString ChargeCode { get; }

		/// <summary>
		/// AL_AC -> AC_ChargeType
		/// </summary>
		ZString ChargeType { get; }

		/// <summary>
		/// AL_AT -> AT_Type
		/// </summary>
		ZString TaxType { get; }

		/// <summary>
		/// AL_AT -> GetRate(AL_TaxDate)
		/// </summary>
		ZDecimal TaxRate { get; }

		/// <summary>
		/// AL_LineAmount
		/// </summary>
		ZDecimal LineAmount { get; }

		/// <summary>
		/// AL_OSAmount
		/// </summary>
		ZDecimal OSAmount { get; }

		/// <summary>
		/// AL_GSTVAT
		/// </summary>
		ZDecimal GSTVATAmount { get; }
	}
}
