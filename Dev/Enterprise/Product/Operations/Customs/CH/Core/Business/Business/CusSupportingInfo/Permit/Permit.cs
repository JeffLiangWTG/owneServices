using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.CH.Business;

public class Permit : CusSupportingInfo, ICusCodeDataTypeSupporter
{
	public new class Schema : CusSupportingInfo.Schema
	{
		public new const int CSI_CodeMaxLength = 2;
		public new const int CSI_IssuerTypeMaxLength = 2;
		public new const int CSI_ReferenceNumberMaxLength = 35;
		public new const int CSI_DescriptionMaxLength = 70;
	}

	public Permit(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	#region ICusCodeDataTypeSupporter Members

	IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
	{
		return new Dictionary<ZString, Type>()
			{
				{ CusCodeDataTypeList.Codes.PermitItemDetails, typeof(PermitItemDetail) }
			};
	}

	IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
	{
		yield return new CusCodeDataTypeSupporterFetchStrategy(this);
	}

	#endregion

	[ChildEditable(true)]
	[UniversalCopyCollectionEntity(CusCodeDataSchema.Constants.TableName, CusCodeDataSchema.Constants.CY_ParentID, CusCodeDataSchema.Constants.CY_ParentTableCode)]
	public PermitItemDetailCollection PermitItemDetails
	{
		get
		{
			if (permitItemDetails == null)
			{
				permitItemDetails = new PermitItemDetailCollection(this);
				RegisterEditableChildObject(permitItemDetails);
				permitItemDetails.Load();
			}

			return permitItemDetails;
		}
	}
	PermitItemDetailCollection permitItemDetails;

	public new PermitLookups Lookups => (PermitLookups)base.Lookups;

	protected override CusSupportingInfoLookups GetNewLookups() => new PermitLookups(this);

	public new PermitValidation Validation => (PermitValidation)base.Validation;

	protected override CusSupportingInfoValidation GetNewValidation() => new PermitValidation(this);

	public new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

	public override bool SupportsNotes => false;

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();

		CSI_Type = Common.CH.CusSupportingInfoTypeList.Codes.Permit;
		CSI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
	}

	#region Properties

	[ResourceStringData("C89648B7-8AC7-46DD-A862-BF2B982D0448", Caption = "Type")]
	[MaxLength(Schema.CSI_CodeMaxLength)]
	[List(nameof(Lookups) + "." + nameof(PermitLookups.PermitTypeCodeList))]
	public override ZString CSI_Code
	{
		get => base.CSI_Code;
		set
		{
			base.CSI_Code = value;
			if (!IsValidationSuspended)
			{
				Validation.ValidateCSI_IssuerType();
				ValidateAdditionalTaxesTariff();
			}
		}
	}

	[ResourceStringData("27E9E6D3-1EE2-47AA-9B8D-F2D48F69C7AC", Caption = "Authority")]
	[MaxLength(Schema.CSI_IssuerTypeMaxLength)]
	[List(nameof(Lookups) + "." + nameof(PermitLookups.PermitAuthorityCodeList))]
	public override ZString CSI_IssuerType
	{
		get => base.CSI_IssuerType;
		set
		{
			base.CSI_IssuerType = value;
			if (!IsValidationSuspended)
			{
				Validation.ValidateCSI_Code();
				ValidateAdditionalTaxesTariff();
			}
		}
	}

	[ResourceStringData("FAB8894F-371F-4964-B28D-3D5AF3C89445", Caption = "Number")]
	[MaxLength(Schema.CSI_ReferenceNumberMaxLength)]
	public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

	[ResourceStringData("B75AEE12-728A-4989-B3A6-05FF9B48E310", Caption = "Issue Date")]
	public override ZDateTime CSI_DateOfIssue { get => base.CSI_DateOfIssue; set => base.CSI_DateOfIssue = value; }

	[ResourceStringData("5705A6D1-B7E4-4A98-B142-C0F898755263", Caption = "Additional Information", MediumCaption = "Addl. Information", ShortCaption = "Addl. Inf.")]
	[MaxLength(Schema.CSI_DescriptionMaxLength)]
	public override ZString CSI_Description { get => base.CSI_Description; set => base.CSI_Description = value; }

	public ZBool SupportsPermitItemDetails => PermitCodes.IsEPermit(CSI_Code);

	public ZBool HasQuantityAndLineNumberItemDetails => Factory.GetCached(ref hasQuantityAndLineNumberItemDetails, () =>  PermitItemDetails.ContainsCode(PermitItemDetailKeyList.Codes.Key1) && PermitItemDetails.ContainsCode(PermitItemDetailKeyList.Codes.Key2));
	CachedProperty<ZBool> hasQuantityAndLineNumberItemDetails;

	#endregion

	protected override void OnFactorySaving()
	{
		if (!SupportsPermitItemDetails)
		{
			PermitItemDetails.RemoveAndDeleteAll();
		}

		base.OnFactorySaving();
	}

	void ValidateAdditionalTaxesTariff()
	{
		if (Parent == null)
		{
			return;
		}

		foreach (var cusLineTariffDetail in Parent.AdditionalTaxes)
		{
			cusLineTariffDetail.Validation.ValidateBZ_Tariff();
		}
	}

	protected override ZString HumanReadableNameCore => Res.GetString("6D184C75-5374-428A-99BB-9F5D5B10A5D8", "Permit");
}
