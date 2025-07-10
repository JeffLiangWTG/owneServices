using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.NL.Business.Common;

namespace Enterprise.Customs.NL.Business.Declaration;

public class AdditionalInfo : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo
{
	public AdditionalInfo(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	[ResourceStringData("BA22E1FE-EF95-47E6-A8B2-FB474E24348B", Caption = "Kind of document", ShortCaption = "Kind")]
	public override ZString CSI_SubType
	{
		get => base.CSI_SubType;
		set => base.CSI_SubType = value;
	}

	public new AdditionalInfoValidation Validation => (AdditionalInfoValidation)base.Validation;

	protected override CusSupportingInfoValidation GetNewValidation() => new AdditionalInfoValidation(this);

	[MaxLength(70)]
	public override ZString CSI_ReferenceNumber
	{
		get => base.CSI_ReferenceNumber;
		set => base.CSI_ReferenceNumber = value;
	}

	[ReadOnly(true)]
	public override ZString CSI_Description
	{
		get => base.CSI_Description;
		set => base.CSI_Description = value;
	}

	[ResourceStringData("0A416E2E-3A25-4D43-A929-304BBD68A292", Caption = "Type")]
	public override ZString CSI_Code
	{
		get => base.CSI_Code;
		set
		{
			var oldValue = CSI_Code;
			base.CSI_Code = value;
			if (!IsCopying && oldValue != CSI_Code)
			{
				if (!IsValidationSuspended)
				{
					Validation.ValidateCSI_ReferenceNumber();
				}

				DefaultReferenceNumber();
			}
		}
	}

	public void DefaultReferenceNumber()
	{
		var declaration = Declaration;
		if (declaration != null && IsATransportDocument)
		{
			var csiCode = CSI_Code;
			if (csiCode == NLConstants.TransportContractTypes.WaybillFreightForwarder || csiCode == NLConstants.TransportContractTypes.BillOfLadingFreightForwarder)
			{
				CSI_ReferenceNumber = declaration.JE_HouseBill;
			}
			else if (csiCode == NLConstants.TransportContractTypes.MasterBillOfLading || csiCode == NLConstants.TransportContractTypes.MaritimeBillofLading || csiCode == NLConstants.TransportContractTypes.MasterAirWaybill)
			{
				CSI_ReferenceNumber = declaration.JE_MasterBill;
			}
		}
	}

	public bool HasDuplicateInAdditionalInfoCollectionOnInvoiceLineParent => Parent is JobComInvoiceLine invoiceLine && invoiceLine.AdditionalInfos.Cast<AdditionalInfo>().Any(i => i.PK != PK && i.CSI_SubType == CSI_SubType && i.CSI_Code == CSI_Code && i.CSI_ReferenceNumber == CSI_ReferenceNumber);
	public bool HasDuplicateInAdditionalInfoCollectionOnEntryInstructionParent => Parent is CusEntryInstruction entryInstruction && entryInstruction.AdditionalInfos.Cast<AdditionalInfo>().Any(i => i.PK != PK && i.CSI_SubType == CSI_SubType && i.CSI_Code == CSI_Code && i.CSI_ReferenceNumber == CSI_ReferenceNumber);
}
