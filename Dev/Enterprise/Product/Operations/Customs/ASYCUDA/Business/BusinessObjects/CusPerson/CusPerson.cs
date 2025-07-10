using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business
{
	[SingleObjectAroundARow]
	public class CusPerson : Customs.Business.CusPerson, Integration.Customs.ASYCUDA.ICusPerson
	{
		public CusPerson(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		public new class Schema : Customs.Business.AutoCusPerson.Schema
		{
			public const string PersonBirthDate = "PersonBirthDate";
			public const string PersonCountry = "PersonCountry";
			public const string PersonFullName = "PersonFullName";
			public const string PersonGender = "PersonGender";
			public const string PersonIdentificationNumber = "PersonIdentificationNumber";
			public const string PersonNationality = "PersonNationality";
			public const string PersonPassport = "PersonPassport";
			public const string PersonPassportExpiry = "PersonPassportExpiry";
			public const string PersonPassportPlaceOfIssue = "PersonPassportPlaceOfIssue";
		}

		public static readonly CusPersonTypeDecider TypeDecider = new CusPersonTypeDecider();

		public Type GetPersonCountryType() => GetPersonCountryTypeCore();

		protected virtual Type GetPersonCountryTypeCore() => typeof(CusPersonCountry);

		public override GlbPerson Person => person ?? (person = base.Person);
		GlbPerson person;

		[ResourceStringData("9FF3E3BC-69BE-4339-9BF0-0AB75078EF14", Caption = "Passenger")]
		public override ZBool CPN_IsPassenger { get => base.CPN_IsPassenger; set => base.CPN_IsPassenger = value; }

		[List(nameof(Lookups) + "." + nameof(CusPersonLookups.GlobalPersonsList))]
		[ResourceStringData("15F331B1-B8C3-4E5A-AF8B-E2BC4C82F0FD", Caption = "Person")]
		public override ZGuid CPN_PER_Person
		{
			get => base.CPN_PER_Person;
			set
			{
				var oldValue = CPN_PER_Person;
				base.CPN_PER_Person = value;
				if (!IsCopying && oldValue != CPN_PER_Person)
				{
					person = null;
				}
			}
		}

		public AsycudaManifestHeader Header => Factory.Load<AsycudaManifestHeader>(CPN_ParentID);

		[ReadOnly(true)]
		[RelatedBusinessObject(nameof(Header))]
		public override ZGuid CPN_ParentID { get => base.CPN_ParentID; set => base.CPN_ParentID = value; }

		[ReadOnly(true)]
		public override ZString CPN_ParentTableCode { get => base.CPN_ParentTableCode; set => base.CPN_ParentTableCode = value; }

		public new Customs.Business.ICusPersonCountryCollection<CusPersonCountry> Countries => (Customs.Business.ICusPersonCountryCollection<CusPersonCountry>)base.Countries;

		protected override Customs.Business.ICusPersonCountryCollection<Customs.Business.CusPersonCountry> CreateNewCusPersonCountryCollection() => new CusPersonCountryCollection<CusPersonCountry>(this);

		public override void Delete()
		{
			Countries.RemoveAndDeleteAll();
			base.Delete();
		}

		protected override Customs.Business.CusPersonValidation GetNewValidation() => new CusPersonValidation(this);

		public new CusPersonValidation Validation => (CusPersonValidation)base.Validation;

		protected override Customs.Business.CusPersonLookups GetNewLookups() => new CusPersonLookups(this);

		public new CusPersonLookups Lookups => (CusPersonLookups)base.Lookups;

		#region GlbPerson Wrapped Properties for Display

		[ResourceStringData("A963C7C0-3A84-4B25-B810-34B4F1AD8DDC", Caption = "Name")]
		public ZString PersonFullName => Person?.PER_FullName ?? ZString.Empty;

		[ResourceStringData("735A487B-EAA3-4DD6-A351-44E92711DD6B", Caption = "Gender")]
		public ZString PersonGender => Person?.PER_Gender ?? ZString.Empty;

		[ResourceStringData("52821F9C-229A-4BD6-A061-EB27B92180BA", Caption = "Birth Date")]
		public ZDate PersonBirthDate => Person?.PER_BirthDate ?? ZDate.Empty;

		[ResourceStringData("538EC6C1-E507-4CE5-AA2F-0D8FE647FB6B", Caption = "Residence")]
		public ZString PersonCountry => Person?.PER_RN_NKCountry ?? ZString.Empty;

		[ResourceStringData("2AA759B5-5A9C-481E-9DEA-2A37EC0D8A74", Caption = "Nationality")]
		public ZString PersonNationality => Person?.PER_RN_NKNationalityCodeISO ?? ZString.Empty;

		[ResourceStringData("BB8E1F9D-8D3F-4FB8-8143-8778F250B90D", Caption = "Identification Number")]
		public ZString PersonIdentificationNumber => Person?.PER_DriversLicenseNumber ?? ZString.Empty;

		[ResourceStringData("283D571E-5E42-476A-B9A8-40C3C0DF8235", Caption = "Passport")]
		public ZString PersonPassport => Person?.PER_Passport ?? ZString.Empty;

		[ResourceStringData("AFA97210-27C0-4B96-A8FE-BBC0A2410E01", Caption = "Passport Place Of Issue")]
		public ZString PersonPassportPlaceOfIssue => Person?.PER_PassportPlaceOfIssue ?? ZString.Empty;

		[ResourceStringData("F79B051D-7A13-4E27-B855-CEBEDB8D6750", Caption = "Passport Expires")]
		public ZDate PersonPassportExpiry => Person?.PER_PassportExpiryDate ?? ZDate.Empty;

		#endregion

		public ZBool IsDriver => !CPN_IsPassenger;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			base.CPN_ParentTableCode = AsycudaManifestHeaderSchema.Constants.Prefix;
		}

		public override bool SupportsNotes => false;

		#region FetchStrategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new CusPersonFetchStrategy(this);

		#endregion
	}
}
