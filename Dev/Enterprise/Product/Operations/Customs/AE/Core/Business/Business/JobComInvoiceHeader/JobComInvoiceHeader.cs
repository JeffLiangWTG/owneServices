using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AE.Business;

public class JobComInvoiceHeader : TypeSafeJobComInvoiceHeader, Integration.Customs.AE.IJobComInvoiceHeader
{
	public JobComInvoiceHeader(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public override ZString JZ_RN_NKDefaultOrigin
	{
		get { return base.JZ_RN_NKDefaultOrigin; }
		set
		{
			var hasChanged = JZ_RN_NKDefaultOrigin != value;
			base.JZ_RN_NKDefaultOrigin = value;
			if (!IsCopying && hasChanged)
			{
				JobComInvoiceLines.MarkAsNeedingValidation();
			}
		}
	}

	[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.InvoiceTypeList))]
	[ResourceStringData("E4AA6F51-B6F9-4156-8282-A0B61FED9B80", Caption = "Invoice Type")]
	public override ZInt JZ_InvoiceType
	{
		get => base.JZ_InvoiceType;
		set => base.JZ_InvoiceType = value;
	}

	[ResourceStringData("81399399-DFCE-47E7-BF63-BB1EBC0C3626", Caption = "Total No Of Pages")]
	public override ZInt JZ_TotNoOfInvPages
	{
		get => base.JZ_TotNoOfInvPages;
		set => base.JZ_TotNoOfInvPages = value;
	}

	[MaxLength(20)]
	[ResourceStringData("DCCA9086-CD6C-4633-8C25-B2B1DB6817AF", Caption = "Attestation Number", ShortCaption ="Attestation No")]
	public override ZString JZ_AttestationNo
	{
		get => base.JZ_AttestationNo;
		set => base.JZ_AttestationNo = value;
	}

	[ResourceStringData("Enterprise.Customs.AE.Business.JobComInvoiceHeader|JZ_PaymentMethod", Caption = "Payment Method")]
	public override ZString JZ_PaymentMethod { get => base.JZ_PaymentMethod; set => base.JZ_PaymentMethod = value; }

	[ResourceStringData("Enterprise.Customs.AE.Business.JobComInvoiceHeader|JZ_ValuationCode", Caption = "Valuation Code")]
	public override ZString JZ_ValuationCode { get => base.JZ_ValuationCode; set => base.JZ_ValuationCode = value; }

	protected override ZString LocalCurrencyCodeCore
	{
		get { return JobDeclaration.LocalCurrencyConstantCode; }
	}

	public override int PackagesFreeStore
	{
		get { return 0; }
	}

	public override int PackagesBond
	{
		get { return 0; }
	}

	protected override ZString DefaultDataGroupingForTariffsCore => PersistentDeclaration is JobDeclaration declaration ? declaration.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff) : AEConstants.DefaultDataGroupingForTariffs;
}
