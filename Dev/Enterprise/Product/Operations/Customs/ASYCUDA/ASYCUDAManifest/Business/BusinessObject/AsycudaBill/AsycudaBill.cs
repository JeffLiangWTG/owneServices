using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ASYCUDAManifest.Business
{
	public class AsycudaBill : ASYCUDA.Business.AsycudaBill, Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaBill
	{
		public new partial class Schema : ASYCUDA.Business.AsycudaBill.Schema
		{
			public const string SADOfficeCode = "SADOfficeCode";
			public const int SADOfficeCodeMaxLength = 3;
			public const string SADRegistrationSerial = "SADRegistrationSerial";
			public const int SADRegistrationSerialMaxLength = 3;
			public const string SADRegistrationNumber = "SADRegistrationNumber";
			public const int SADRegistrationNumberMaxLength = 10;
			public const string SADRegistrationDate = "SADRegistrationDate";
			public const string DefaultSADRegistrationSerial = "C";
		}

		public AsycudaBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region EGM(Export General Manifest)

		public ABLEntryNum SADRegistrationCusEntryNumber
		{
			get
			{
				if (sADRegistrationCusEntryNumber == null || sADRegistrationCusEntryNumber.IsDeleted || sADRegistrationCusEntryNumber.CE_EntryType != Constants.CustomsEntryType.SAD)
				{
					sADRegistrationCusEntryNumber = Common.CusEntryNumber.Load<ABLEntryNum>(this, Constants.CustomsEntryType.SAD, CountryCode);
					if (sADRegistrationCusEntryNumber != null)
					{
						RegisterEditableChildObject(sADRegistrationCusEntryNumber);
					}
				}
				return sADRegistrationCusEntryNumber;
			}
		}
		ABLEntryNum sADRegistrationCusEntryNumber;

		[ResourceStringData("AsycudaBill.SADOfficeCode", Caption = "SAD Office Code")]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.SADOfficeCodeList))]
		[MaxLength(Schema.SADOfficeCodeMaxLength)]
		public ZString SADOfficeCode
		{
			get => SADRegistrationCusEntryNumber?.CE_EntryLineReference ?? ZString.Empty;
			set
			{
				CheckMaximumLength(SADOfficeCodeInfo, value);
				var cusEntryNumber = SADRegistrationCusEntryNumber;
				var oldValue = cusEntryNumber?.CE_EntryLineReference ?? ZString.Empty;
				if (!value.IsEmpty)
				{
					if (cusEntryNumber == null)
					{
						sADRegistrationCusEntryNumber = CreateNewSADRegistration();
						RegisterEditableChildObject(sADRegistrationCusEntryNumber);
					}
					sADRegistrationCusEntryNumber.CE_EntryLineReference = value;
				}
				else
				{
					if (cusEntryNumber != null)
					{
						cusEntryNumber.CE_EntryLineReference = value;
					}
				}
				SADOfficeCodeInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					RegularBillValidation.ValidateSADOfficeCode();
				}
			}
		}

		public ZPropertyInfo SADOfficeCodeInfo => GetZPropertyInfo(Schema.SADOfficeCode);

		[ResourceStringData("AsycudaBill.SADRegistrationSerial", Caption = "Registration Serial")]
		[MaxLength(Schema.SADRegistrationSerialMaxLength)]
		public ZString SADRegistrationSerial
		{
			get => SADRegistrationCusEntryNumber?.CE_Category ?? ZString.Empty;
			set
			{
				CheckMaximumLength(SADRegistrationSerialInfo, value);
				var cusEntryNumber = SADRegistrationCusEntryNumber;
				var oldValue = cusEntryNumber?.CE_Category ?? ZString.Empty;
				if (!value.IsEmpty)
				{
					if (cusEntryNumber == null)
					{
						sADRegistrationCusEntryNumber = CreateNewSADRegistration();
						RegisterEditableChildObject(sADRegistrationCusEntryNumber);
					}
					sADRegistrationCusEntryNumber.CE_Category = value;
				}
				else
				{
					if (cusEntryNumber != null)
					{
						cusEntryNumber.CE_Category = value;
					}
				}
				SADRegistrationSerialInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo SADRegistrationSerialInfo => GetZPropertyInfo(Schema.SADRegistrationSerial);

		[ResourceStringData("AsycudaBill.SADRegistrationNumber", Caption = "Registration Number")]
		[MaxLength(Schema.SADRegistrationNumberMaxLength)]
		public ZString SADRegistrationNumber
		{
			get => SADRegistrationCusEntryNumber?.CE_EntryNum ?? ZString.Empty;
			set
			{
				CheckMaximumLength(SADRegistrationNumberInfo, value);
				var cusEntryNumber = SADRegistrationCusEntryNumber;
				var oldValue = cusEntryNumber?.CE_EntryNum ?? ZString.Empty;
				if (!value.IsEmpty)
				{
					if (cusEntryNumber == null)
					{
						sADRegistrationCusEntryNumber = CreateNewSADRegistration();
						RegisterEditableChildObject(sADRegistrationCusEntryNumber);
					}
					sADRegistrationCusEntryNumber.CE_EntryNum = value;
				}
				else
				{
					if (cusEntryNumber != null)
					{
						cusEntryNumber.CE_EntryNum = value;
					}
				}
				SADRegistrationNumberInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo SADRegistrationNumberInfo => GetZPropertyInfo(Schema.SADRegistrationNumber);

		[ResourceStringData("AsycudaBill.SADRegistrationDate", Caption = "Registration Date")]
		[BusinessObjectTestExclude]
		public ZDateTime SADRegistrationDate
		{
			get => SADRegistrationCusEntryNumber?.CE_IssueDate ?? ZDateTime.Empty;
			set
			{
				var cusEntryNumber = SADRegistrationCusEntryNumber;
				var oldValue = cusEntryNumber?.CE_IssueDate ?? ZDateTime.Empty;
				if (value.IsValid)
				{
					if (cusEntryNumber == null)
					{
						sADRegistrationCusEntryNumber = CreateNewSADRegistration();
						RegisterEditableChildObject(sADRegistrationCusEntryNumber);
					}
					sADRegistrationCusEntryNumber.CE_IssueDate = value;
				}
				else
				{
					if (cusEntryNumber != null)
					{
						cusEntryNumber.CE_IssueDate = value;
					}
				}
				SADRegistrationDateInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo SADRegistrationDateInfo => GetZPropertyInfo(Schema.SADRegistrationDate);

		protected override void DefaultOnCountryChanged()
		{
			base.DefaultOnCountryChanged();
			sADRegistrationCusEntryNumber = null;
		}

		ABLEntryNum CreateNewSADRegistration()
		{
			var sADRegistration = Common.CusEntryNumber.LoadOrCreate<ABLEntryNum>(this, Constants.CustomsEntryType.SAD, CountryCode);
			sADRegistration.CE_Category = Schema.DefaultSADRegistrationSerial;
			return sADRegistration;
		}

		#endregion

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;
		public new AsycudaPackCollection<AsycudaPack, AsycudaBill> Packs => (AsycudaPackCollection<AsycudaPack, AsycudaBill>)base.Packs;
		protected override ManifestBase.IAsycudaPackCollection<ManifestBase.AsycudaPack, ManifestBase.AsycudaBill> CreateNewAsycudaPackCollection() => new AsycudaPackCollection<AsycudaPack, AsycudaBill>(this);

		protected override Type GetPackTypeCore() => typeof(AsycudaPack);

		protected override Type GetPackageContainerLinkTypeCore() => typeof(AsycudaContainerBillOrPackageLink);

		public override MultilingualString GetWarningBeforeBeingDeleted()
		{
			MultilingualString result = (NoResString)string.Empty;
			if (HasManifestBeenSubmittedToCustoms)
			{
				result = ResString.GetMultilingualString("71944A7D-130E-4E46-8436-D5351B67630F", "Bill number {0} is already registered with Customs.\r\nAre you sure you want to delete it from the manifest?", ABL_BillNumber);
			}

			return result;
		}

		public override bool CanDelete => true;

		public new AsycudaBillLookups Lookups => (AsycudaBillLookups)base.Lookups;
		protected override ManifestBase.AsycudaBillLookups GetNewLookups() => new AsycudaBillLookups(this);
		protected override ManifestBase.AsycudaBillValidation GetNewValidationForRegularBill() => new AsycudaBillValidationForRegularBill(this);
		AsycudaBillValidationForRegularBill RegularBillValidation => Validation as AsycudaBillValidationForRegularBill;
	}
}
