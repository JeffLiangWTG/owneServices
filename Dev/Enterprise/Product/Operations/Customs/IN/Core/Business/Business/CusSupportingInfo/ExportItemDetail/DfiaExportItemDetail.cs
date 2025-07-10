using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common.IN;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IN.Business;

public sealed class DfiaExportItemDetail : CusSupportingInfoWithSerialNo, IHugeSequenceNumberLine, Integration.Customs.ICusSupportingInfoTypeSupporter, ICusSupportingInfoWithSerialNoParent
{
	public DfiaExportItemDetail(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : AutoINCusSupportingInfo.Schema
	{
		public const int ReferenceNumberMaxLength = 25;
		public const int ReferenceNumber2MaxLength = 10;
		public const int QuantityDecimalPlaces = 3;
	}

	public new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

	[MaxLength(Schema.ReferenceNumberMaxLength)]
	[ResourceStringData("Enterprise.Customs.IN.Business.DfiaExportItemDetail|CSI_ReferenceNumber", Caption = "License Number", MediumCaption = "Lic. No.", ShortCaption = "Lic.")]
	public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

	[ResourceStringData("Enterprise.Customs.IN.Business.DfiaExportItemDetail|CSI_DateOfIssue", Caption = "License Date", MediumCaption = "Date", ShortCaption = "Date")]
	public override ZDateTime CSI_DateOfIssue { get => base.CSI_DateOfIssue; set => base.CSI_DateOfIssue = value; }

	[MaxLength(Schema.ReferenceNumber2MaxLength)]
	[ResourceStringData("Enterprise.Customs.IN.Business.DfiaExportItemDetail|CSI_ReferenceNumber2", Caption = "License Export Item Serial Number", MediumCaption = "Lic. EXP. Item. Sr. No.", ShortCaption = "Lic. Sr. No.")]
	public override ZString CSI_ReferenceNumber2 { get => base.CSI_ReferenceNumber2; set => base.CSI_ReferenceNumber2 = value; }

	[DecimalPlaces(Schema.QuantityDecimalPlaces)]
	[ResourceStringData("Enterprise.Customs.IN.Business.DfiaExportItemDetail|CSI_Quantity", Caption = "License Export Quantity", MediumCaption = "Lic. Exp. Qty.", ShortCaption = "Qty.")]
	public override ZDecimal CSI_Quantity { get => base.CSI_Quantity; set => base.CSI_Quantity = value; }

	[ResourceStringData("Enterprise.Customs.IN.Business.DfiaExportItemDetail|CSI_UnitOfQuantity", Caption = "License Export Quantity Unit", MediumCaption = "UOM", ShortCaption = "UOM")]
	public override ZString CSI_UnitOfQuantity { get => base.CSI_UnitOfQuantity; set => base.CSI_UnitOfQuantity = value; }

	[UniversalCopyCollectionEntity(CusSupportingInfoSchema.Constants.TableName, CusSupportingInfoSchema.Constants.CSI_ParentTableCode)]
	[ChildEditable(true)]
	public DfiaImportItemDetailCollection DfiaImportItemDetails
	{
		get
		{
			if (fDfiaImportItemDetails == null)
			{
				fDfiaImportItemDetails = new DfiaImportItemDetailCollection(this);
				fDfiaImportItemDetails.Load();
				RegisterEditableChildObject(fDfiaImportItemDetails);
			}
			return fDfiaImportItemDetails;
		}
	}

	DfiaImportItemDetailCollection fDfiaImportItemDetails;

	public HugeSequenceNumberGenerator DfiaImportItemDetailsLineNumberGenerator => fDfiaImportItemDetailsLineNumberGenerator ??= new HugeSequenceNumberGenerator(() => DfiaImportItemDetails);
	HugeSequenceNumberGenerator fDfiaImportItemDetailsLineNumberGenerator;

	public override void Delete()
	{
		using (this.GetLineNumberSuspenders())
		{
			base.Delete();
		}
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		CSI_Type = CusSupportingInfoTypeList.Codes.DutyFreeImportAuthorization;
		CSI_Code = Constants.CusSupportingInfo.DutyFreeImportAuthorization;
		CSI_SubType = Constants.CusSupportingInfo.Export;
	}

	protected override CusSupportingInfoLookups GetNewLookups() => new DfiaExportItemDetailLookup(this);

	protected override CusSupportingInfoValidation GetNewValidation() => new DfiaExportItemDetailValidation(this);

	#region ICusSupportingInfoTypeSupporter

	IDictionary<ZString, Type> Integration.Customs.ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes() => new Dictionary<ZString, Type>
	{
		{ CusSupportingInfoTypeList.Codes.DutyFreeImportAuthorization, typeof(DfiaImportItemDetail) }
	};

	IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
	{
		yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
	}

	#endregion

	HugeSequenceNumberGenerator ICusSupportingInfoWithSerialNoParent.GetSequenceNumberGenerator(string type)
	{
		return type switch
		{
			CusSupportingInfoTypeList.Codes.DutyFreeImportAuthorization => DfiaImportItemDetailsLineNumberGenerator,
			_ => null
		};
	}
}
