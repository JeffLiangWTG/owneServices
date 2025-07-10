using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	[SystemDefinedValues]
	public class ICS2JobDocAddress : JobDocAddress
	{
		public ICS2JobDocAddress(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : JobDocAddress.Schema
		{
			public const string SubDivision = nameof(SubDivision);
			public const string Number = nameof(Number);
			public const string POBox = nameof(POBox);
		}

		public new ICS2JobDocAddressValidation Validation => (ICS2JobDocAddressValidation)base.Validation;

		protected override JobDocAddressValidation GetNewValidation() => new ICS2JobDocAddressValidation(this);

		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.ICS2JobDocAddress.SubDivision", Caption = "Facility Place Sub-Division", ShortCaption = "Sub-Division")]
		[MaxLength(35)]
		public ZString SubDivision
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.SubDivision);
			set
			{
				var oldValue = SubDivision;

				if (oldValue != value)
				{
					CheckMaximumLength(SubDivisionInfo, value);
					this.SetSystemDefinedValue(Schema.SubDivision, value);

					SubDivisionInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo SubDivisionInfo => GetZPropertyInfo(Schema.SubDivision);

		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.ICS2JobDocAddress.Number", Caption = "Facility Place Number", ShortCaption = "Number")]
		[MaxLength(35)]
		public ZString Number
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.Number);
			set
			{
				var oldValue = Number;

				if (oldValue != value)
				{
					CheckMaximumLength(NumberInfo, value);
					this.SetSystemDefinedValue(Schema.Number, value);

					NumberInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo NumberInfo => GetZPropertyInfo(Schema.Number);

		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.ICS2JobDocAddress.POBox", Caption = "Facility Place P.O. Box", ShortCaption = "P.O. Box")]
		[MaxLength(70)]
		public ZString POBox
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.POBox);
			set
			{
				var oldValue = POBox;

				if (oldValue != value)
				{
					CheckMaximumLength(POBoxInfo, value);
					this.SetSystemDefinedValue(Schema.POBox, value);

					POBoxInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo POBoxInfo => GetZPropertyInfo(Schema.POBox);

		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.ICS2JobDocAddress.E2_Postcode", Caption = "Facility Place Postcode", ShortCaption = "Postcode")]
		public override ZString E2_Postcode
		{
			get => base.E2_Postcode;
			set
			{
				base.E2_Postcode = value;

				if (!IsValidationSuspended)
				{
					Validation.CheckFacilityPlaceFieldsRequirement();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.ICS2JobDocAddress.E2_CompanyName", Caption = "Facility Place Name", ShortCaption = "Name")]
		public override ZString E2_CompanyName { get => base.E2_CompanyName; set => base.E2_CompanyName = value; }

		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.ICS2JobDocAddress.E2_Address1", Caption = "Facility Place Street", ShortCaption = "Street")]
		public override ZString E2_Address1 { get => base.E2_Address1; set => base.E2_Address1 = value; }

		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.ICS2JobDocAddress.E2_Address2", Caption = "Facility Place Street additional line", ShortCaption = "Street additional line")]
		public override ZString E2_Address2 { get => base.E2_Address2; set => base.E2_Address2 = value; }

		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.ICS2JobDocAddress.E2_State", Caption = "Facility Place State", ShortCaption = "State")]
		public override ZString E2_State { get => base.E2_State; set => base.E2_State = value; }

		[MaxLength(35)]
		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.ICS2JobDocAddress.E2_City", Caption = "Facility Place City", ShortCaption = "City")]
		public override ZString E2_City
		{
			get => base.E2_City;
			set
			{
				base.E2_City = value;

				if (!IsValidationSuspended)
				{
					Validation.CheckFacilityPlaceFieldsRequirement();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.ICS2JobDocAddress.E2_RN_NKCountryCode", Caption = "Facility Place Country", ShortCaption = "Country")]
		public override ZString E2_RN_NKCountryCode
		{
			get => base.E2_RN_NKCountryCode;
			set
			{
				base.E2_RN_NKCountryCode = value;

				if (!IsValidationSuspended)
				{
					Validation.CheckFacilityPlaceFieldsRequirement();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobDocAddressLookups.OrgHeader_List))]
		public new ZGuid OrganisationPK { get => base.OrganisationPK; set => base.OrganisationPK = value; }
	}
}
