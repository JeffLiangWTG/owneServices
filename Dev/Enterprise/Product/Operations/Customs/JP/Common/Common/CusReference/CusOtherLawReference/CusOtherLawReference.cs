using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.Common;

public class CusOtherLawReference : CusReference
{
	public CusOtherLawReference(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public static class Constants
	{
		public const string MS = "MS";
		public const string MM = "MM";
	}

	[List(nameof(Lookups) + "." + nameof(CusOtherLawReferenceLookups.ReferenceList))]
	[ResourceStringData("AC0679E1-ECC9-4D12-9B94-83795FA0FAC1", Caption = "Code", FullDescription = "Code for verification based on other laws and regulations (other than customs laws)")]
	[MaxLength(2)]
	public override ZString CFR_Reference
	{
		get => base.CFR_Reference;
		set => base.CFR_Reference = value;
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		CFR_Type = CusReferenceTypeList.Codes.OtherLawReference;
	}

	[ResourceStringData("D3E4D04E-2D9C-4766-9C4E-BBE5FC37167E", Caption = "Description", ShortCaption = "Desc.")]
	public ZString CodeDescription => Lookups.ReferenceList.GetDescriptionFromCode(CFR_Reference);

	public new CusOtherLawReferenceLookups Lookups => base.Lookups as CusOtherLawReferenceLookups;

	protected override CusReferenceLookups GetNewLookups() => new CusOtherLawReferenceLookups(this);

	protected override CusReferenceValidation GetNewValidation() => new CusOtherLawReferenceValidation(this);

	protected override ZString HumanReadableNameCore => Res.GetString("A8992C17-026E-4B2D-B32A-30B02A1BD948", "Other Law");

	#region Other Law Type

	public ZBool IsRoadTranportVehicleLaw => CurrentOtherLawType == OtherLawType.RoadTranportVehicleLaw;

	public OtherLawType CurrentOtherLawType => GetCurrentOtherLawType();

	OtherLawType GetCurrentOtherLawType()
	{
		if (roadTransportVehicleLawCodes.Contains(CFR_Reference))
		{
			return OtherLawType.RoadTranportVehicleLaw;
		}
		return OtherLawType.Other;
	}

	public enum OtherLawType
	{
		Other = 0,
		RoadTranportVehicleLaw = 1,
	}

	readonly ImmutableHashSet<string> roadTransportVehicleLawCodes = new[]
	{
		Constants.MS,
		Constants.MM
	}.ToImmutableHashSet();

	#endregion

	#if DEBUG

	protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
	{
		base.FillWithValidTestDataCore(kind, propertyPath);
		CFR_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
	}

	#endif
}
