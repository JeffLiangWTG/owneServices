using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.NL.Business;

[SingleObjectAroundARow]
[DependentBusinessObject(typeof(JobDeclaration), nameof(JobDeclaration.BorderTransports))]
[SystemDefinedValues]
public class BorderTransport : CusCodeData, IShortSequenceNumberLine
{
	public BorderTransport(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{ }

	public new class Schema : CusCodeData.Schema
	{
		public const string Nationality = "Nationality";
	}

	public new JobDeclaration Parent => (JobDeclaration)base.Parent;

	[List(nameof(Lookups) + "." + nameof(BorderTransportLookups.CY_CodeList))]
	[MaxLength(2)]
	[ResourceStringData("17232563-D34F-45C8-AB0C-75BE889BCDB2", Caption = "Type of ID")]
	public override ZString CY_Code
	{
		get => base.CY_Code;
		set => base.CY_Code = value;
	}

	[MaxLength(35)]
	public override ZString CY_Data
	{
		get => base.CY_Data;
		set => base.CY_Data = value;
	}

	public override ZShort CY_Order
	{
		get { return base.CY_Order; }
		set
		{
			if (value > 0)
			{
				base.CY_Order = value;
			}
		}
	}

	[ResourceStringData("8A5B4DFC-85DB-4898-9E55-6254ABD9804A", Caption = "Nationality")]
	[List(nameof(Lookups) + "." + nameof(BorderTransportLookups.TransportCountryList))]
	[MaxLength(2)]
	public virtual ZString Nationality
	{
		get { return this.GetSystemDefinedValue<ZString>(Schema.Nationality); }
		set
		{
			ZString oldValue = Nationality;
			CheckMaximumLength(NationalityInfo, value);
			if (oldValue != value)
			{
				this.SetSystemDefinedValue(Schema.Nationality, AddOnColumnDataType.Codes.String, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateNationality();
				}
			}
			NationalityInfo.RefreshBinding(oldValue);
		}
	}

	public virtual ZPropertyInfo NationalityInfo
	{
		get { return GetZPropertyInfo(Schema.Nationality); }
	}

	protected override CusCodeDataLookups GetNewLookups() => new BorderTransportLookups(this);

	public new BorderTransportLookups Lookups => (BorderTransportLookups)base.Lookups;

	protected override CusCodeDataValidation GetNewValidation() => new BorderTransportValidation(this);

	public new BorderTransportValidation Validation => (BorderTransportValidation)base.Validation;

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		CY_Type = CusCodeDataTypeList.Codes.TransportAtBorder;
	}

	protected override TypeLoaderCollection parentLoaders => new(typeof(JobDeclaration));

	#region IShortSequenceNumberLine

	ZShort ISequenceNumberLine<ZShort>.SequenceNumber
	{
		get => CY_Order;
		set => CY_Order = value;
	}
	ZGuid ISequenceNumberLine.FKToHeader => CY_ParentID;

	#endregion

	protected override ZString HumanReadableNameCore => Res.GetString("2EBA47F2-3A17-4CBE-A2FC-E2B91AB369A3", "Transport at border");
}
