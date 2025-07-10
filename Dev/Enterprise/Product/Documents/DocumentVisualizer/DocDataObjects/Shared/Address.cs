using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.MasterFiles;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public sealed class Address : DocDataObject, IAddress
	{
		public Address(BusinessObjectFactory factory)
			: base(factory)
		{
			Argument.NotNull(factory, nameof(factory));
			AddressPropertyInfos.ForEach(p => p.ValueChanged += AddressPropertyInfo_ValueChanged);
		}

		#region AddressIdentifier

		public ZGuid AddressIdentifier
		{
			get => addressIdentifier;
			set
			{
				if (SetNonPersistentPropertyValue(AddressIdentifierInfo, ref addressIdentifier, value))
				{
					Validate(AddressIdentifierInfo);
				}
			}
		}

		ZGuid addressIdentifier;

		public ZPropertyInfo AddressIdentifierInfo => GetZPropertyInfo(nameof(AddressIdentifier));

		#endregion

		#region HeaderIdentifier

		public ZGuid HeaderIdentifier
		{
			get => headerIdentifier;
			set
			{
				if (SetNonPersistentPropertyValue(HeaderIdentifierInfo, ref headerIdentifier, value))
				{
					Validate(HeaderIdentifierInfo);
				}
			}
		}

		ZGuid headerIdentifier;

		public ZPropertyInfo HeaderIdentifierInfo => GetZPropertyInfo(nameof(HeaderIdentifier));

		#endregion

		#region CompanyName

		[MaxLength(NotificationTypes.MessageError, 100)]
		public ZString CompanyName
		{
			get => companyName;
			set
			{
				if (SetNonPersistentPropertyValue(CompanyNameInfo, ref companyName, value))
				{
					Validate(CompanyNameInfo);
				}
			}
		}

		ZString companyName;

		public ZPropertyInfo CompanyNameInfo => GetZPropertyInfo(nameof(CompanyName));

		#endregion

		#region AddressLine1

		[MaxLength(NotificationTypes.MessageError, 50)]
		public ZString AddressLine1
		{
			get => addressLine1;
			set
			{
				if (SetNonPersistentPropertyValue(AddressLine1Info, ref addressLine1, value))
				{
					Validate(AddressLine1Info);
				}
			}
		}

		ZString addressLine1;

		public ZPropertyInfo AddressLine1Info => GetZPropertyInfo(nameof(AddressLine1));

		#endregion

		#region AddressLine2

		[MaxLength(NotificationTypes.MessageError, 50)]
		public ZString AddressLine2
		{
			get => addressLine2;
			set
			{
				if (SetNonPersistentPropertyValue(AddressLine2Info, ref addressLine2, value))
				{
					Validate(AddressLine2Info);
				}
			}
		}

		ZString addressLine2;

		public ZPropertyInfo AddressLine2Info => GetZPropertyInfo(nameof(AddressLine2));

		#endregion

		#region AdditionalAddressInformation

		[MaxLength(NotificationTypes.MessageError, 50)]
		public ZString AdditionalAddressInformation
		{
			get => additionalAddressInformation;
			set
			{
				if (SetNonPersistentPropertyValue(AdditionalAddressInformationInfo, ref additionalAddressInformation, value))
				{
					Validate(AdditionalAddressInformationInfo);
				}
			}
		}

		ZString additionalAddressInformation;

		public ZPropertyInfo AdditionalAddressInformationInfo => GetZPropertyInfo(nameof(AdditionalAddressInformation));

		#endregion

		#region City

		[MaxLength(NotificationTypes.MessageError, 50)]
		public ZString City
		{
			get => city;
			set
			{
				if (SetNonPersistentPropertyValue(CityInfo, ref city, value))
				{
					Validate(CityInfo);
				}
			}
		}

		ZString city;

		public ZPropertyInfo CityInfo => GetZPropertyInfo(nameof(City));

		#endregion

		#region State

		[MaxLength(NotificationTypes.MessageError, 25)]
		public ZString State
		{
			get => state;
			set
			{
				if (SetNonPersistentPropertyValue(StateInfo, ref state, value))
				{
					Validate(StateInfo);
				}
			}
		}

		ZString state;

		public ZPropertyInfo StateInfo => GetZPropertyInfo(nameof(State));

		#endregion

		#region Postcode

		[MaxLength(NotificationTypes.MessageError, 10)]
		public ZString Postcode
		{
			get => postcode;
			set
			{
				if (SetNonPersistentPropertyValue(PostcodeInfo, ref postcode, value))
				{
					Validate(PostcodeInfo);
				}
			}
		}

		ZString postcode;

		public ZPropertyInfo PostcodeInfo => GetZPropertyInfo(nameof(Postcode));

		#endregion

		#region Phone

		[MaxLength(NotificationTypes.MessageError, 20)]
		public ZString Phone
		{
			get => phone;
			set
			{
				if (SetNonPersistentPropertyValue(PhoneInfo, ref phone, value))
				{
					Validate(PhoneInfo);
				}
			}
		}

		ZString phone;

		public ZPropertyInfo PhoneInfo => GetZPropertyInfo(nameof(Phone));

		#endregion

		#region Fax

		[MaxLength(NotificationTypes.MessageError, 20)]
		public ZString Fax
		{
			get => fax;
			set
			{
				if (SetNonPersistentPropertyValue(FaxInfo, ref fax, value))
				{
					Validate(FaxInfo);
				}
			}
		}

		ZString fax;

		public ZPropertyInfo FaxInfo => GetZPropertyInfo(nameof(Fax));

		#endregion

		#region Email

		[MaxLength(NotificationTypes.MessageError, 60)]
		public ZString Email
		{
			get => email;
			set
			{
				if (SetNonPersistentPropertyValue(EmailInfo, ref email, value))
				{
					Validate(EmailInfo);
				}
			}
		}

		ZString email;

		public ZPropertyInfo EmailInfo => GetZPropertyInfo(nameof(Email));

		#endregion

		#region Contact

		[MaxLength(NotificationTypes.MessageError, 256)]
		public ZString Contact
		{
			get => contact;
			set
			{
				if (SetNonPersistentPropertyValue(ContactInfo, ref contact, value))
				{
					Validate(ContactInfo);
				}
			}
		}

		ZString contact;

		public ZPropertyInfo ContactInfo => GetZPropertyInfo(nameof(Contact));

		#endregion

		#region TaxNumber

		[MaxLength(NotificationTypes.MessageError, 254)]
		public ZString TaxNumber
		{
			get => taxNumber;
			set
			{
				if (SetNonPersistentPropertyValue(TaxNumberInfo, ref taxNumber, value))
				{
					Validate(TaxNumberInfo);
				}
			}
		}

		ZString taxNumber;

		public ZPropertyInfo TaxNumberInfo => GetZPropertyInfo(nameof(TaxNumber));

		#endregion

		#region TaxNumberType

		public ICodeDescription TaxNumberType
		{
			get => taxNumberType;
			set => taxNumberType = SetChild(taxNumberType, value);
		}

		ICodeDescription taxNumberType;

		#endregion

		#region Country

		public Country Country
		{
			get => country;
			set
			{
				var addressFormattedNeedUpdate = country != value || country != null && value != null && (country.Code != value.Code || country.Name != value.Name);
				country = SetChild(country, value);
				if (addressFormattedNeedUpdate)
				{
					UpdateAddressFormatted();

					if (country != null)
					{
						country.NameInfo.ValueChanged -= AddressPropertyInfo_ValueChanged;
						country.CodeInfo.ValueChanged -= AddressPropertyInfo_ValueChanged;
						country.NameInfo.ValueChanged += AddressPropertyInfo_ValueChanged;
						country.CodeInfo.ValueChanged += AddressPropertyInfo_ValueChanged;
					}
				}
			}
		}

		Country country;

		ICountry IAddress.Country => Country;

		#endregion

		#region AddressFormatted

		public ZString AddressFormatted
		{
			get => addressFormatted;
			set
			{
				if (SetNonPersistentPropertyValue(AddressFormattedInfo, ref addressFormatted, value))
				{
					Validate(AddressFormattedInfo);
				}
			}
		}

		ZString addressFormatted;

		public ZPropertyInfo AddressFormattedInfo => GetZPropertyInfo(nameof(AddressFormatted));

		IEnumerable<ZPropertyInfo> AddressPropertyInfos
		{
			get
			{
				yield return CompanyNameInfo;
				yield return AddressLine1Info;
				yield return AddressLine2Info;
				yield return CityInfo;
				yield return StateInfo;
				yield return PostcodeInfo;
			}
		}

		void AddressPropertyInfo_ValueChanged(object sender, System.EventArgs e)
		{
			UpdateAddressFormatted();
		}

		void UpdateAddressFormatted()
		{
			var formatter = ObjectFactory.Get<IGenericAddressFormatter>();
			AddressFormatted = formatter.PostalAddress(Factory, CompanyName, string.Empty, AddressLine1, AddressLine2, City, State, Postcode, (NoResString)Country?.Name, true, Country?.Code).Replace("\n", System.Environment.NewLine);
		}

		public void AddAddressFormattedValidationDependencies()
		{
			if (!AddedAddressFormattedValidationDependencies)
			{
				this.OnValueChanged(CompanyNameInfo.Name).Do(() => Validate(AddressFormattedInfo));
				this.OnValueChanged(AddressLine1Info.Name).Do(() => Validate(AddressFormattedInfo));
				this.OnValueChanged(AddressLine2Info.Name).Do(() => Validate(AddressFormattedInfo));
				this.OnValueChanged(CityInfo.Name).Do(() => Validate(AddressFormattedInfo));
				this.OnValueChanged(StateInfo.Name).Do(() => Validate(AddressFormattedInfo));
				this.OnValueChanged(PostcodeInfo.Name).Do(() => Validate(AddressFormattedInfo));
				Country?.OnValueChanged(Country.NameInfo.Name).Do(() => Validate(AddressFormattedInfo));
				Country?.OnValueChanged(Country.CodeInfo.Name).Do(() => Validate(AddressFormattedInfo));

				AddedAddressFormattedValidationDependencies = true;
			}
		}

		[IgnoreChanges]
		ZBool AddedAddressFormattedValidationDependencies { get; set; }

		#endregion

		public Unloco Unloco
		{
			get => unloco;
			set => unloco = SetChild(unloco, value);
		}
		Unloco unloco;

		IUnloco IAddress.Unloco => Unloco;

		public IReadOnlyCollection<IRegistrationNumber> RegistrationNumbers { get; set; }

		public override string ToString() => AddressFormatted;
	}
}
