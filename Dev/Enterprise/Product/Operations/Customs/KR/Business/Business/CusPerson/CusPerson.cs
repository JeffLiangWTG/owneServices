using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public class CusPerson : Customs.Business.CusPerson
	{
		public CusPerson(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusPerson.Schema
		{
			public const int RelationshipToDeclarant_MaxLength = 10;
			public const int JobCode_MaxLength = 2;
			public const int EntryStatus_MaxLength = 1;
			public const int ResidencyDate_MaxLength = 8;
			public const int NationalityClassCode_MaxLength = 1;

			public const string RelationshipToDeclarant = "RelationshipToDeclarant";
			public const string JobCode = "JobCode";
			public const string EntryStatus = "EntryStatus";
			public const string ResidencyStartDate = "ResidencyStartDate";
			public const string ResidencyEndDate = "ResidencyEndDate";
			public const string NationalityClassCode = "NationalityClassCode";
		}

		[ResourceStringData("463F1CEE-FFF8-478A-AB6B-C6D533A64C0C", Caption = "Full Name")]
		public override ZGuid CPN_PER_Person { get => base.CPN_PER_Person; set => base.CPN_PER_Person = value; }

		[MaxLength(Schema.RelationshipToDeclarant_MaxLength)]
		[ResourceStringData("F17390D3-B735-4DD6-80A5-A896738DB9DA", Caption = "Relationship")]
		[List(nameof(Lookups) + "." + nameof(CusPersonLookups.RelationshipCodeList))]
		public ZString RelationshipToDeclarant
		{
			get
			{
				return GetCountryValue(CusPersonCountryTypeList.Codes.RelationshipToDeclarant);
			}
			set
			{
				CheckMaximumLength(RelationshipToDeclarantInfo, value);
				var oldValue = RelationshipToDeclarant;
				SetCountryValue(CusPersonCountryTypeList.Codes.RelationshipToDeclarant, value);
				RelationshipToDeclarantInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidateRelationshipToDeclarant();
				}
			}
		}

		public ZPropertyInfo RelationshipToDeclarantInfo
		{
			get { return GetZPropertyInfo(Schema.RelationshipToDeclarant); }
		}

		[MaxLength(Schema.JobCode_MaxLength)]
		[ResourceStringData("C4B738BC-0D07-4DAF-BF65-90703C1BC1D6", Caption = "Job Code")]
		[List(nameof(Lookups) + "." + nameof(CusPersonLookups.ImmigrantJobCodeList))]
		public ZString JobCode
		{
			get
			{
				return GetCountryValue(CusPersonCountryTypeList.Codes.JobCode);
			}
			set
			{
				CheckMaximumLength(JobCodeInfo, value);
				var oldValue = JobCode;
				SetCountryValue(CusPersonCountryTypeList.Codes.JobCode, value);
				JobCodeInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidateJobCode();
				}
			}
		}

		public ZPropertyInfo JobCodeInfo
		{
			get { return GetZPropertyInfo(Schema.JobCode); }
		}

		[MaxLength(Schema.EntryStatus_MaxLength)]
		[ResourceStringData("84D486E8-7463-4429-973E-64A3F85BD2DA", Caption = "Entry Status")]
		[List(nameof(Lookups) + "." + nameof(CusPersonLookups.ImmigrantEntryStatusList))]
		public ZString EntryStatus
		{
			get
			{
				return GetCountryValue(CusPersonCountryTypeList.Codes.EntryStatus);
			}
			set
			{
				CheckMaximumLength(EntryStatusInfo, value);
				var oldValue = EntryStatus;
				SetCountryValue(CusPersonCountryTypeList.Codes.EntryStatus, value);
				EntryStatusInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidateEntryStatus();
				}
			}
		}

		public ZPropertyInfo EntryStatusInfo
		{
			get { return GetZPropertyInfo(Schema.EntryStatus); }
		}

		[MaxLength(Schema.ResidencyDate_MaxLength)]
		[ResourceStringData("ADBBE3B7-8DBD-4DAD-A924-13F4FAC6E70F", Caption = "Start Date of Stay")]
		public ZDate ResidencyStartDate
		{
			get
			{
				ZDate result = ZDate.Empty;
				if (DateTime.TryParseExact(GetCountryValue(CusPersonCountryTypeList.Codes.ResidencyStartDate), Constants.DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt1))
				{
					result = new ZDate(dt1);
				}

				return result;
			}
			set
			{
				var newValue = value.ToString(Constants.DateFormatType.Date);
				CheckMaximumLength(ResidencyStartDateInfo, newValue);
				var oldValue = ResidencyStartDate;
				SetCountryValue(CusPersonCountryTypeList.Codes.ResidencyStartDate, newValue);
				ResidencyStartDateInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidateResidencyStartDate();
				}
			}
		}

		public ZPropertyInfo ResidencyStartDateInfo
		{
			get { return GetZPropertyInfo(Schema.ResidencyStartDate); }
		}

		[MaxLength(Schema.ResidencyDate_MaxLength)]
		[ResourceStringData("0F6ED812-897B-49F4-8A06-24341B58FDA5", Caption = "End Date of Stay")]
		public ZDate ResidencyEndDate
		{
			get
			{
				ZDate result = ZDate.Empty;
				if (DateTime.TryParseExact(GetCountryValue(CusPersonCountryTypeList.Codes.ResidencyEndDate), Constants.DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt1))
				{
					result = new ZDate(dt1);
				}

				return result;
			}
			set
			{
				var newValue = value.ToString(Constants.DateFormatType.Date);
				CheckMaximumLength(ResidencyEndDateInfo, newValue);
				var oldValue = ResidencyEndDate;
				SetCountryValue(CusPersonCountryTypeList.Codes.ResidencyEndDate, newValue);
				ResidencyEndDateInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidateResidencyEndDate();
				}
			}
		}

		public ZPropertyInfo ResidencyEndDateInfo
		{
			get { return GetZPropertyInfo(Schema.ResidencyEndDate); }
		}

		[MaxLength(Schema.NationalityClassCode_MaxLength)]
		public ZString NationalityClassCode
		{
			get
			{
				return GetCountryValue(CusPersonCountryTypeList.Codes.NationalityClassification);
			}
			set
			{
				CheckMaximumLength(NationalityClassCodeInfo, value);
				var oldValue = NationalityClassCode;
				SetCountryValue(CusPersonCountryTypeList.Codes.NationalityClassification, value);
				NationalityClassCodeInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidateNationalityClassCode();
				}
			}
		}

		public ZPropertyInfo NationalityClassCodeInfo
		{
			get { return GetZPropertyInfo(Schema.NationalityClassCode); }
		}

		ZString GetCountryValue(string dataType)
		{
			return Countries.Cast<CusPersonCountry>().FirstOrDefault(x => x.CPC_Type == dataType)?.CPC_Value ?? ZString.Empty;
		}

		void SetCountryValue(string dataType, ZString value)
		{
			var cusPersonCountry = Countries.Cast<CusPersonCountry>().FirstOrDefault(c => c.CPC_Type == dataType);
			if (cusPersonCountry == null && !value.IsEmpty)
			{
				cusPersonCountry = Countries.AddNew();
				cusPersonCountry.CPC_Type = dataType;
			}
			if (cusPersonCountry != null)
			{
				if (value.IsEmpty)
				{
					Countries.RemoveAndDelete(cusPersonCountry);
				}
				else
				{
					cusPersonCountry.CPC_Value = value;
				}
			}
		}

		public JobDeclaration ParentJobDeclaration => CPN_ParentTableCode == JobDeclarationSchema.Constants.Prefix ? Factory.Load<JobDeclaration>(CPN_ParentTableCode, CPN_ParentID) : null;

		[ResourceStringData("E59830F5-D7DE-4631-84D4-9091C66F9C31", Caption = "Full Name")]
		public ZString PersonFullName => Person?.PER_FullName ?? ZString.Empty;
		public ZString PersonHomePhoneNumber => ParentJobDeclaration?.StevedoreCompany?.E2_Phone ?? ZString.Empty;
		public ZString PersonMobilePhoneNumber => Person?.PER_MobilePhone ?? ZString.Empty;

		[ResourceStringData("AE3B52B8-92E5-4AB7-BF2F-876E3334E915", Caption = "Birthday")]
		public ZDate PersonBirthDate => Person?.PER_BirthDate ?? ZDate.Empty;

		[ResourceStringData("D20B2D04-82FC-48C2-8DEC-6C1F812356B0", Caption = "Passport Number")]
		public ZString Passport => Person?.PER_Passport ?? ZString.Empty;

		public new ICusPersonCountryCollection<CusPersonCountry> Countries => (CusPersonCountryCollection<CusPersonCountry>)base.Countries;

		protected override ICusPersonCountryCollection<Customs.Business.CusPersonCountry> CreateNewCusPersonCountryCollection() => new CusPersonCountryCollection<CusPersonCountry>(this);

		public new CusPersonValidation Validation => (CusPersonValidation)base.Validation;
		protected override Customs.Business.CusPersonValidation GetNewValidation()
		{
			if (ParentJobDeclaration?.IsLocalExport ?? false)
			{
				return new LocalExportCusPersonValidation(this);
			}
			else if (ParentJobDeclaration?.IsPersonalItemDeclaration ?? false)
			{
				return new PIDCusPersonValidation(this);
			}
			else
			{
				return new CusPersonValidation(this);
			}
		}

		public new CusPersonLookups Lookups => (CusPersonLookups)base.Lookups;
		protected override Customs.Business.CusPersonLookups GetNewLookups() => new CusPersonLookups(this);
	}
}
