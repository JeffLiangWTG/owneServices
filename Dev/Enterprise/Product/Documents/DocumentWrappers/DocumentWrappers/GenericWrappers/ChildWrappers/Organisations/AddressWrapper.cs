using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.ChildWrappers.Organisations;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("CompanyNameAndAddress")]
	public class AddressWrapper : GenericWrapper
	{
		public AddressWrapper(JobDocAddress docAddress, BusinessObjectFactory factory)
			: base(docAddress, factory)
		{
			Argument.NotNull(factory, "factory");
			if (docAddress != null)
			{
				jobDocAddress = docAddress;
				orgAddress = docAddress.Address;
				Setup(docAddress);
			}
		}

		public AddressWrapper(JobDocAddress docAddress, ZString pCountryCode, MultilingualString countryName, ZString countryForFormatter, BusinessObjectFactory factory)
			: base(docAddress, factory)
		{
			Argument.NotNull(factory, "factory");
			if (docAddress != null)
			{
				jobDocAddress = docAddress;
				orgAddress = docAddress.Address;
				countryCode = pCountryCode;
				SetupWithDefaultFormatterCountry(docAddress, countryForFormatter, countryName);
			}
		}

		public AddressWrapper(OrgAddress address, ContactType contactType, BusinessObjectFactory factory)
			: base(address, factory)
		{
			Argument.NotNull(factory, "factory");
			orgAddress = address;
			this.contactType = contactType;
			if (orgAddress != null)
			{
				Setup(orgAddress, contactType);
			}
		}

		public AddressWrapper(ZString companyNameAndAddress, BusinessObjectFactory factory)
			: base(null, factory)
		{
			Argument.NotNull(factory, "factory");
			SetupNameAndAddress(companyNameAndAddress.Split('\n')[0].Trim(), companyNameAndAddress);
		}

		public AddressWrapper(OrganisationUsageType usageType, JobDocAddress docAddress, BusinessObjectFactory factory)
			: this(docAddress, factory)
		{
			Organization = new OrganisationWrapper(usageType, docAddress, factory);
		}

		public AddressWrapper(OrganisationUsageType usageType, OrgAddress address, ContactType contactType, BusinessObjectFactory factory)
			: this(address, contactType, factory)
		{
			Organization = new OrganisationWrapper(usageType, address, contactType, factory);
		}

		public AddressWrapper(OrganisationUsageType usageType, ZString companyNameAndAddress, BusinessObjectFactory factory)
			: this(companyNameAndAddress, factory)
		{
			Organization = new OrganisationWrapper(usageType, companyNameAndAddress, factory);
		}

		/// <summary>
		/// Called by the ContactWrapper, should not be used for anything else.
		/// </summary>
		/// <param name="address"></param>
		/// <param name="contactName"></param>
		/// <param name="factory"></param>
		public AddressWrapper(OrgAddress address, ZString contactName, BusinessObjectFactory factory)
			: base(address, factory)
		{
			Argument.NotNull(factory, "factory");
			if (address != null)
			{
				orgAddress = address;
				contactNameCache = contactName;
				SetAddressToDisplayAndAddressFormatterToDisplay(orgAddress);
			}
		}

		public static AddressWrapper Empty(BusinessObjectFactory factory)
		{
			return new AddressWrapper((JobDocAddress)null, factory);
		}

		void SetupWithDefaultFormatterCountry(JobDocAddress docAddress, ZString countryForFormatter, MultilingualString countryName)
		{
			if (!docAddress.E2_Contact.IsEmpty)
			{
				SetAddressToDisplayAndAddressFormatterToDisplayWithDefaultCountryForFormatter(orgAddress, countryForFormatter, countryName);
			}
			else
			{
				SetupWithDefaultCountryForFormatterAndContactType(orgAddress, docAddress.DefaultContactType, countryForFormatter, countryName);
			}
		}

		void Setup(JobDocAddress docAddress)
		{
			if (!docAddress.E2_AddressOverride && orgAddress != null)
			{
				if (!docAddress.E2_Contact.IsEmpty)
				{
					SetAddressToDisplayAndAddressFormatterToDisplay(orgAddress);
				}
				else
				{
					Setup(orgAddress, docAddress.DefaultContactType);
				}
			}
		}

		void SetupWithDefaultCountryForFormatterAndContactType(OrgAddress address, ContactType contactType, ZString countryForFormatter, MultilingualString countryName)
		{
			var orgContactName = ZString.Empty;
			OrgHeader organisation = address.Header;
			if (organisation != null)
			{
				OrgContact contact = new DefaultContactFinder(organisation, true).DefaultContact(contactType);
				if (contact != null)
				{
					orgContactName = contact.OC_ContactName;
				}
			}
			contactNameCache = orgContactName;
			SetAddressToDisplayAndAddressFormatterToDisplayWithDefaultCountryForFormatter(address, countryForFormatter, countryName);
		}

		void Setup(OrgAddress address, ContactType contactType)
		{
			var orgContactName = ZString.Empty;
			OrgHeader organisation = address.Header;
			if (organisation != null)
			{
				OrgContact contact = new DefaultContactFinder(organisation, true).DefaultContact(contactType);
				if (contact != null)
				{
					orgContactName = contact.OC_ContactName;
				}
			}
			contactNameCache = orgContactName;
			SetAddressToDisplayAndAddressFormatterToDisplay(address);
		}

		OrgAddress addressForTranslate;
		ZString currentAddressLanguage;

		bool AddressAndAddressFormatterToDisplayNeedToBeSet()
		{
			return !currentAddressLanguage.IsEmpty && currentAddressLanguage != Res.CurrentLanguage && addressForTranslate != null && !addressForTranslate.IsDeleted;
		}

		void SetAddressToDisplayAndAddressFormatterToDisplayWithDefaultCountryForFormatter(OrgAddress address, ZString countryForFormatter, MultilingualString countryName)
		{
			addressForTranslate = address;
			ISupportWebAddressValidation addressToDisplay = address;
			AddressFormatter addressFormatterToDisplay = null;

			if (address.Language != Res.CurrentLanguage)
			{
				var translatedAddress = address.GetTranslatedAddressInSpecificLanguage(Res.CurrentLanguage);
				if (translatedAddress != null)
				{
					addressToDisplay = translatedAddress;
					addressFormatterToDisplay = new AddressFormatter(address.Factory, addressToDisplay, countryName, countryForFormatter, false, jobDocAddress?.GetTranslatedAdditionalAddressInSpecificLanguageWithFallback(Res.CurrentLanguage));
				}
			}

			currentAddressLanguage = Res.CurrentLanguage;

			if (addressFormatterToDisplay == null)
			{
				addressFormatterToDisplay = new PostalAddressFormatter(address, countryForFormatter, countryName, jobDocAddress != null && !jobDocAddress.E2_AddressOverride && !jobDocAddress.E2_AdditionalAddressInformation.IsEmpty ? jobDocAddress.E2_AdditionalAddressInformation : null);
			}

			SetupNameAndAddress(addressFormatterToDisplay);

			addressLine1 = GetZStringInCorrectCase(addressToDisplay.Address1);
			addressLine2 = GetZStringInCorrectCase(addressToDisplay.Address2);
			city = GetZStringInCorrectCase(addressToDisplay.City);
			state = GetZStringInCorrectCase(addressToDisplay.StateCode);
			postCode = GetZStringInCorrectCase(addressToDisplay.Postcode);
		}

		void SetAddressToDisplayAndAddressFormatterToDisplay(OrgAddress address)
		{
			addressForTranslate = address;
			ISupportWebAddressValidation addressToDisplay = address;
			AddressFormatter addressFormatterToDisplay = null;

			if (address.Language != Res.CurrentLanguage)
			{
				var translatedAddress = address.GetTranslatedAddressInSpecificLanguage(Res.CurrentLanguage);
				if (translatedAddress != null)
				{
					addressToDisplay = translatedAddress;
					addressFormatterToDisplay = new AddressFormatter(address.Factory, addressToDisplay, address.CountryName, address.OA_RN_NKCountryCode, false, jobDocAddress?.GetTranslatedAdditionalAddressInSpecificLanguageWithFallback(Res.CurrentLanguage));
				}
			}

			currentAddressLanguage = Res.CurrentLanguage;

			if (addressFormatterToDisplay == null)
			{
				addressFormatterToDisplay = new PostalAddressFormatter(address, jobDocAddress != null && !jobDocAddress.E2_AddressOverride && !jobDocAddress.E2_AdditionalAddressInformation.IsEmpty ? jobDocAddress.E2_AdditionalAddressInformation : null);
			}

			SetupNameAndAddress(addressFormatterToDisplay);

			addressLine1 = GetZStringInCorrectCase(addressToDisplay.Address1);
			addressLine2 = GetZStringInCorrectCase(addressToDisplay.Address2);
			city = GetZStringInCorrectCase(addressToDisplay.City);
			state = GetZStringInCorrectCase(addressToDisplay.StateCode);
			postCode = GetZStringInCorrectCase(addressToDisplay.Postcode);
		}

		void SetupNameAndAddress(AddressFormatter formatter)
		{
			addressFormatter = formatter;
		}

		void SetupNameAndAddress(ZString initialCompanyName, ZString initialCompanyNameAndAddress)
		{
			var orgAllowMixedCase = Env.Registry.OrgAllowMixedCase;
			companyName = orgAllowMixedCase ? initialCompanyName : initialCompanyName.ToUpperInvariant();
			companyNameAndAddress = orgAllowMixedCase ? initialCompanyNameAndAddress : initialCompanyNameAndAddress.ToUpperInvariant();
		}

		void AppendScheduleToStringBuilder(StringBuilder sb, string localName, IEnumerable<OrgTimetable> schedule)
		{
			if (schedule.IsNullOrEmpty())
			{
				return;
			}

			if (sb.Length > 0)
			{
				sb.Append(" ");
			}
			sb.Append("[");
			sb.Append(localName);
			sb.Append("] ");
			sb.Append(SectionInfo(schedule.First()));

			foreach (var item in schedule.Skip(1))
			{
				sb.Append(", ");
				sb.Append(SectionInfo(item));
			}
		}

		(bool matches, IEnumerable<OrgTimetable> schedule) AllDaysMatch(IEnumerable<IEnumerable<OrgTimetable>> timetables)
		{
			bool TimesAreEqual(IEnumerable<OrgTimetable> schedule1, IEnumerable<OrgTimetable> schedule2)
			{
				// This is equivalent to an XOR statement
				if (schedule1.IsNullOrEmpty() != schedule2.IsNullOrEmpty())
				{
					return false;
				}

				return schedule1.Count() == schedule2.Count()
					&& schedule1
					.Zip(schedule2, (x, y) => x.OTT_TimeFrom == y.OTT_TimeFrom && x.OTT_TimeTo == y.OTT_TimeTo)
					.All(x => x);
			}

			return timetables.Skip(1)
				.Aggregate<IEnumerable<OrgTimetable>, (bool matches, IEnumerable<OrgTimetable> schedule)>(
					(true, timetables.First()),
					(acc, schedule) => (acc.matches && TimesAreEqual(acc.schedule, schedule), schedule)
				);
		}

		string WeekdayString => Res.GetString("13ec1ee1-ae25-474c-a034-308f6c0117ba", "Weekday");
		string EverydayString => Res.GetString("d86872a1-433e-4e3e-8c27-0bc4e721d64d", "Everyday");

		string GetTimetableByType(string type)
		{
			if (Timetables.NotApplicable)
			{
				return string.Empty;
			}

			// Sort relevant timetables and extract their timeslot information
			IComparer comparer = new OrgTimetableComparer();
			var result = new StringBuilder();
			Timetables.ApplySort(comparer);
			var filteredTimetables = Timetables
				.Where(timetable => timetable.OTT_Type == type);

			if (Timetables.WeekdayRange)
			{
				AppendScheduleToStringBuilder(result, WeekdayString, filteredTimetables);
				return result.ToString();
			}

			// Assign timeslots and their localisations to lookups based on their DayOfWeek
			var daySchedules = new Dictionary<string, List<OrgTimetable>>();
			var localizedDays = new Dictionary<string, string>();
			var weekDays = Country?.WeekDays;
			if (weekDays.IsNullOrEmpty())
			{
				weekDays = GetDefaultWeekdays();
			}

			var workingDays = Country?.WorkingDays;
			if (workingDays.IsNullOrEmpty())
			{
				workingDays = GetDefaultWorkingDays();
			}

			try
			{
				foreach (var weekDay in weekDays)
				{
					daySchedules[weekDay] = new List<OrgTimetable>();
					localizedDays[weekDay] = weekDay;
				}

				foreach (var ttb in filteredTimetables)
				{
					daySchedules[ttb.DayOfWeek].Add(ttb);
					localizedDays[ttb.DayOfWeek] = ttb.DayOfWeekLocalized;
				}

				// Return [Everyday] aggregation if appropriate
				var everydayMatchingResult = AllDaysMatch(weekDays.Select(weekDay => daySchedules[weekDay]));
				if (everydayMatchingResult.matches)
				{
					AppendScheduleToStringBuilder(result, EverydayString, everydayMatchingResult.schedule);
					return result.ToString();
				}

				// Return [Weekday] aggregation if appropriate
				var workingDaysMatchingResult = AllDaysMatch(workingDays.Select(weekDay => daySchedules[weekDay]));
				if (workingDaysMatchingResult.matches)
				{
					AppendScheduleToStringBuilder(result, WeekdayString, workingDaysMatchingResult.schedule);
					return result.ToString();
				}

				// Return original timetables
				foreach (var day in weekDays)
				{
					AppendScheduleToStringBuilder(result, localizedDays[day], daySchedules[day]);
				}
			}
			catch (KeyNotFoundException ex)
			{
				var weekDaysJoined = string.Join(",", weekDays);
				var workingDaysJoined = string.Join(",", workingDays);
				var timeTableDays = string.Join(",", filteredTimetables.Select(t => t.DayOfWeek));
				var dayScheduleKeys = string.Join(",", daySchedules.Keys);
				var localizedDaysKeys = string.Join(",", localizedDays.Keys);

				ErrorReporter.ReportOnce($"Dictionary key exception for timetable days: [{timeTableDays}]. Searched dictionaries are daySchedules: [{dayScheduleKeys}], localizedDays: [{localizedDaysKeys}]. Weekdays are: [{weekDaysJoined}], working days are: [{workingDaysJoined}]", ex);
			}

			return result.ToString();
		}

		static ZString[] GetDefaultWeekdays()
		{
			return new ZString[]
			{
				AutoDayOfWeekCodeList.Codes.Sunday,
				AutoDayOfWeekCodeList.Codes.Monday,
				AutoDayOfWeekCodeList.Codes.Tuesday,
				AutoDayOfWeekCodeList.Codes.Wednesday,
				AutoDayOfWeekCodeList.Codes.Thursday,
				AutoDayOfWeekCodeList.Codes.Friday,
				AutoDayOfWeekCodeList.Codes.Saturday
			};
		}

		static ZString[] GetDefaultWorkingDays()
		{
			return new ZString[]
			{
				AutoDayOfWeekCodeList.Codes.Monday,
				AutoDayOfWeekCodeList.Codes.Tuesday,
				AutoDayOfWeekCodeList.Codes.Wednesday,
				AutoDayOfWeekCodeList.Codes.Thursday,
				AutoDayOfWeekCodeList.Codes.Friday,
			};
		}

		string SectionInfo(OrgTimetable item)
		{
			return item.OTT_TimeFrom.ToShortTimeString() + WideDash + item.OTT_TimeTo.ToShortTimeString();
		}

		string WideDash => " " + (NoResString)"–" + " ";

		public RegistrationNumberCodeWrapperCollection CustomsCodes => fCustomsCodes ?? (fCustomsCodes = new RegistrationNumberCodeWrapperCollection(orgAddress?.CustomsCodes, Factory));
		RegistrationNumberCodeWrapperCollection fCustomsCodes;

		public string WrappedLanguage { get; } = Res.CurrentLanguage;

		public CountryWrapper Country => fCountry ?? (fCountry = new CountryWrapper(CountryCode, Factory));
		CountryWrapper fCountry;

		ZString? GetZStringInCorrectCase(ZString? str)
		{
			return Env.Registry.OrgAllowMixedCase ? str : str?.ToUpperInvariant();
		}

		ZString CountryCode
		{
			get
			{
				var result = (countryCode ?? (countryCode = GetZStringInCorrectCase(orgAddress?.OA_RN_NKCountryCode))) ?? ZString.Empty;
				if (jobDocAddress != null && (jobDocAddress.E2_AddressOverride || orgAddress == null))
				{
					result = (countryCode = jobDocAddress.E2_RN_NKCountryCode) ?? ZString.Empty;
				}
				return result;
			}
		}
		ZString? countryCode;

		public LocationWrapper Location => fLocation ?? (fLocation = new LocationWrapper(LocationCode, Factory));
		LocationWrapper fLocation;

		ZString LocationCode => (locationCode ?? (locationCode = GetZStringInCorrectCase(orgAddress?.OA_RL_NKRelatedPortCode))) ?? ZString.Empty;
		ZString? locationCode;

		public ZString CompanyCode => (companyCode ?? (companyCode = GetZStringInCorrectCase(orgAddress?.Header?.OH_Code))) ?? ZString.Empty;
		ZString? companyCode;

		public ZString CompanyName => AddressFormatter != null ? (ZString)AddressFormatter.CompanyName() : companyName;
		ZString companyName;

		public ZString CompanyNameAndAddress => AddressFormatter != null ? (ZString)AddressFormatter.PostalAddress() : companyNameAndAddress;
		ZString companyNameAndAddress;

		AddressFormatter AddressFormatter
		{
			get
			{
				if (AddressAndAddressFormatterToDisplayNeedToBeSet())
				{
					SetAddressToDisplayAndAddressFormatterToDisplay(orgAddress);
				}
				else if (jobDocAddress != null && (jobDocAddress.E2_AddressOverride || orgAddress == null))
				{
					addressFormatter = addressFormatter ?? new PostalAddressFormatter(jobDocAddress);
				}
				return addressFormatter;
			}
		}
		AddressFormatter addressFormatter;

		public ZString Address
		{
			get
			{
				if (AddressFormatter != null)
				{
					return AddressFormatter.PostalAddressWithoutCompanyName();
				}
				else
				{
					var address = CompanyNameAndAddress;
					if (!address.IsEmpty && !CompanyName.IsEmpty)
					{
						address = address.Replace(CompanyName, ZString.Empty).Trim();
					}

					return address;
				}
			}
		}

		public ZString AddressCaption => (jobDocAddress?.DocAddressType == DocAddressType.None ? ZString.Empty : jobDocAddress?.AddressCaption) ?? ZString.Empty;

		public ZString AddressAsASingleLine => Address.Replace("\r\n", " ").Replace("\n", " ").Replace("  ", " ");

		bool UseJobDocAddress()
		{
			return jobDocAddress != null && (jobDocAddress.E2_AddressOverride || orgAddress == null);
		}

		bool UseJobDocAddressContact()
		{
			return jobDocAddress != null && (jobDocAddress.E2_AddressOverride || orgAddress == null || !jobDocAddress.E2_Contact.IsEmpty);
		}

		public ZString AddressLine1
		{
			get
			{
				if (AddressAndAddressFormatterToDisplayNeedToBeSet())
				{
					SetAddressToDisplayAndAddressFormatterToDisplay(orgAddress);
				}
				else if (UseJobDocAddress() && !addressLine1.HasValue)
				{
					addressLine1 = GetZStringInCorrectCase(jobDocAddress.E2_Address1);
				}
				return addressLine1 ?? ZString.Empty;
			}
		}
		ZString? addressLine1;

		public ZString AddressLine2
		{
			get
			{
				if (AddressAndAddressFormatterToDisplayNeedToBeSet())
				{
					SetAddressToDisplayAndAddressFormatterToDisplay(orgAddress);
				}
				else if (UseJobDocAddress() && !addressLine2.HasValue)
				{
					addressLine2 = GetZStringInCorrectCase(jobDocAddress.E2_Address2);
				}
				return addressLine2 ?? ZString.Empty;
			}
		}
		ZString? addressLine2;

		public ZString City
		{
			get
			{
				if (AddressAndAddressFormatterToDisplayNeedToBeSet())
				{
					SetAddressToDisplayAndAddressFormatterToDisplay(orgAddress);
				}
				else if (UseJobDocAddress() && !city.HasValue)
				{
					city = GetZStringInCorrectCase(jobDocAddress.E2_City);
				}
				return city ?? ZString.Empty;
			}
		}
		ZString? city;

		public ZString ShortCode => (shortCode ?? (shortCode = GetZStringInCorrectCase(orgAddress?.OA_Code))) ?? ZString.Empty;
		ZString? shortCode;

		public ZString State
		{
			get
			{
				if (AddressAndAddressFormatterToDisplayNeedToBeSet())
				{
					SetAddressToDisplayAndAddressFormatterToDisplay(orgAddress);
				}
				else if (UseJobDocAddress() && !state.HasValue)
				{
					state = GetZStringInCorrectCase(jobDocAddress.E2_State);
				}
				return state ?? ZString.Empty;
			}
		}
		ZString? state;

		public ZString PostCode
		{
			get
			{
				if (AddressAndAddressFormatterToDisplayNeedToBeSet())
				{
					SetAddressToDisplayAndAddressFormatterToDisplay(orgAddress);
				}
				else if (UseJobDocAddress() && !postCode.HasValue)
				{
					postCode = GetZStringInCorrectCase(jobDocAddress.E2_Postcode);
				}
				return postCode ?? ZString.Empty;
			}
		}
		ZString? postCode;

		public virtual ZString ContactName
		{
			get
			{
				if (!contactNameCache.HasValue)
				{
					contactNameCache = (!jobDocAddress?.E2_Contact.IsEmpty ?? ZBool.False) ? jobDocAddress.E2_Contact : ZString.Empty;
				}
				return contactNameCache.Value;
			}
		}
		ZString? contactNameCache;

		public ZString Phone
		{
			get
			{
				var result = (phone ?? (phone = orgAddress?.OA_Phone_Formatted)) ?? ZString.Empty;
				if (UseJobDocAddressContact())
				{
					result = (phone = jobDocAddress.E2_Phone) ?? ZString.Empty;
				}
				return result;
			}
		}
		ZString? phone;

		public ZString Fax
		{
			get
			{
				var result = (fax ?? (fax = orgAddress?.OA_Fax_Formatted)) ?? ZString.Empty;
				if (UseJobDocAddressContact())
				{
					result = (fax = jobDocAddress.E2_Fax) ?? ZString.Empty;
				}
				return result;
			}
		}
		ZString? fax;

		public ZString Mobile
		{
			get
			{
				var result = (mobile ?? (mobile = orgAddress?.OA_Mobile_Formatted)) ?? ZString.Empty;
				if (jobDocAddress != null && !jobDocAddress.E2_Contact.IsEmpty)
				{
					result = (mobile = jobDocAddress.E2_Mobile) ?? ZString.Empty;
				}
				return result;
			}
		}
		ZString? mobile;

		public ZString Email
		{
			get
			{
				var result = (email ?? (email = orgAddress?.OA_Email)) ?? ZString.Empty;
				if (UseJobDocAddressContact())
				{
					result = (email = jobDocAddress.E2_Email) ?? ZString.Empty;
				}
				return result;
			}
		}
		ZString? email;

		public OrgTimetableCollection Timetables => timetables ?? (timetables = GetOrgTimetables(orgAddress));

		OrgTimetableCollection timetables;

		OrgTimetableCollection GetOrgTimetables(OrgAddress orgAddress)
		{
			if (orgAddress == null)
			{
				return null;
			}

			orgAddress.Timetables.ReInitializeRangeType();
			return orgAddress.Timetables;
		}

		public ZString PickupTimetable => Timetables != null
			? (pickupTimetable ?? (pickupTimetable = GetTimetableByType(OrgTimetableType.Codes.Pickup))) ?? ZString.Empty
			: ZString.Empty;
		ZString? pickupTimetable;

		public ZString DeliverTimetable => Timetables != null
			? (deliverTimetable ?? (deliverTimetable = GetTimetableByType(OrgTimetableType.Codes.Deliver))) ?? ZString.Empty
			: ZString.Empty;
		ZString? deliverTimetable;

		public ZString PickupFromTime => orgAddress != null
			? (pickupFromTime ?? (pickupFromTime = new TotalHoursHelper().GetTextFromTime(orgAddress.OA_PickupFromTimeOnly))) ?? ZString.Empty
			: ZString.Empty;
		ZString? pickupFromTime;

		public ZString PickupToTime => orgAddress != null
			? (pickupToTime ?? (pickupToTime = new TotalHoursHelper().GetTextFromTime(orgAddress.OA_PickupToTimeOnly))) ?? ZString.Empty
			: ZString.Empty;
		ZString? pickupToTime;

		public ZString DeliverFromTime => orgAddress != null
			? (deliverFromTime ?? (deliverFromTime = new TotalHoursHelper().GetTextFromTime(orgAddress.OA_DeliverFromTimeOnly))) ?? ZString.Empty
			: ZString.Empty;
		ZString? deliverFromTime;

		public ZString DeliverToTime => orgAddress != null
			? (deliverToTime ?? (deliverToTime = new TotalHoursHelper().GetTextFromTime(orgAddress.OA_DeliverToTimeOnly))) ?? ZString.Empty
			: ZString.Empty;
		ZString? deliverToTime;

		public ZString DoNotAttendFromTime => orgAddress != null
			? (doNotAttendFromTime ?? (doNotAttendFromTime = new TotalHoursHelper().GetTextFromTime(orgAddress.OA_DoNotAttendFrom))) ?? ZString.Empty
			: ZString.Empty;
		ZString? doNotAttendFromTime;

		public ZString DoNotAttendToTime => orgAddress != null
			? (doNotAttendToTime ?? (doNotAttendToTime = new TotalHoursHelper().GetTextFromTime(orgAddress.OA_DoNotAttendTo))) ?? ZString.Empty
			: ZString.Empty;
		ZString? doNotAttendToTime;

		public ZString FurtherConstraints => (furtherConstraints ?? (furtherConstraints = orgAddress?.OA_LoadingUnloadingConstraints)) ?? ZString.Empty;
		ZString? furtherConstraints;

		public ZString OtherWarehouseFacilities => (otherWarehouseFacilities ?? (otherWarehouseFacilities = orgAddress?.OA_OtherWarehouseFacilities)) ?? ZString.Empty;
		ZString? otherWarehouseFacilities;

		public ZString DeliveryRoute => (deliveryRoute ?? (deliveryRoute = orgAddress?.OA_DeliveryRoute)) ?? ZString.Empty;
		ZString? deliveryRoute;

		public ZShort DeliveryRouteSequence => deliveryRouteSequence ?? (deliveryRouteSequence = orgAddress?.OA_DeliveryRouteSequence) ?? ZShort.Zero;
		ZShort? deliveryRouteSequence;

		public ZBool HasDockLeveller => (hasDockLeveller ?? (hasDockLeveller = orgAddress?.OA_DockLeveler)) ?? ZBool.False;
		ZBool? hasDockLeveller;

		public ZBool HasPalletJack => (hasPalletJack ?? (hasPalletJack = orgAddress?.OA_PalletJack)) ?? ZBool.False;
		ZBool? hasPalletJack;

		public ZBool HasForkLift => (hasForkLift ?? (hasForkLift = orgAddress?.OA_ForkLift)) ?? ZBool.False;
		ZBool? hasForkLift;

		public ZGuid AddressPK => (addressPK ?? (addressPK = orgAddress?.PK)) ?? ZGuid.Empty;
		ZGuid? addressPK;

		OrganisationWrapper organizationWrapper;

		public OrganisationWrapper Organization
		{
			get
			{
				if (organizationWrapper == null)
				{
					if (jobDocAddress != null)
					{
						organizationWrapper = new OrganisationWrapper(OrganisationUsageType.EmptyUsage, jobDocAddress, Factory);
					}
					else if (orgAddress != null && contactType != null)
					{
						organizationWrapper = new OrganisationWrapper(OrganisationUsageType.EmptyUsage, orgAddress,
							contactType, Factory);
					}
					else
					{
						organizationWrapper = new OrganisationWrapper(OrganisationUsageType.EmptyUsage, companyNameAndAddress, Factory);
					}
				}

				return organizationWrapper;
			}
			private set
			{
				organizationWrapper = value;
			}
		}

		public AviationSecurityWrapper AviationSecurity => aviationSecurity ?? (aviationSecurity = GetAviationSecurity());
		AviationSecurityWrapper aviationSecurity;

		public ZBool HasWarehousing => HasDockLeveller || HasPalletJack || HasForkLift || !OtherWarehouseFacilities.IsEmpty;

		public ZBool HasLoadingUnloadingConstraints => !AccessPoint.Code.IsEmpty
			|| !ContainerHandling.Code.IsEmpty
			|| !CommunicationRequired.Code.IsEmpty
			|| !LabourRequired.Code.IsEmpty
			|| !DockHeight.Code.IsEmpty
			|| !FurtherConstraints.IsEmpty;

		public CodeAndDescriptionWrapper AccessPoint => fAccessPoint ?? (fAccessPoint = GetAccessPoint());
		CodeAndDescriptionWrapper fAccessPoint;

		public CodeAndDescriptionWrapper CommunicationRequired => fCommunicationRequired ?? (fCommunicationRequired = GetCommunicationRequired());
		CodeAndDescriptionWrapper fCommunicationRequired;

		public CodeAndDescriptionWrapper ContainerHandling => fContainerHandling ?? (fContainerHandling = GetContainerHandling());
		CodeAndDescriptionWrapper fContainerHandling;

		public CodeAndDescriptionWrapper DockHeight => fDockHeight ?? (fDockHeight = GetDockHeight());
		CodeAndDescriptionWrapper fDockHeight;

		public CodeAndDescriptionWrapper LabourRequired => fLabourRequired ?? (fLabourRequired = GetLabourRequired());
		CodeAndDescriptionWrapper fLabourRequired;

		#region OtherWrappers

		AviationSecurityWrapper GetAviationSecurity()
		{
			if (orgAddress != null && SupplyChainSecurityConfiguration.IsEnabled)
			{
				var countryData = orgAddress.KnownShipperDetails.Cast<OrgCountryData>().FirstOrDefault();
				if (countryData != null)
				{
					return new AviationSecurityWrapper(countryData, Factory);
				}
			}
			return new AviationSecurityWrapper(Factory.GetNull<OrgCountryData>(), Factory);
		}

		#endregion

		#region SameAs

		public ZBool SameAs(AddressWrapper address)
		{
			return CompanyNameAndAddress.Replace("\r\n", " ").Replace("\n", " ").EqualsIgnoringCase(address.CompanyNameAndAddress.Replace("\r\n", " ").Replace("\n", " "));
		}

		#endregion

		#region CodeAndDescriptions

		protected virtual CodeAndDescriptionWrapper GetAccessPoint()
		{
			return (orgAddress == null) ? CodeAndDescriptionWrapper.Empty : new CodeAndDescriptionWrapper(orgAddress.OA_AccessPoint, orgAddress.OA_AccessPoint_List, Factory);
		}

		protected virtual CodeAndDescriptionWrapper GetCommunicationRequired()
		{
			return (orgAddress == null) ? CodeAndDescriptionWrapper.Empty : new CodeAndDescriptionWrapper(orgAddress.OA_CommunicationRequired, orgAddress.OA_CommunicationRequired_List, Factory);
		}

		protected virtual CodeAndDescriptionWrapper GetContainerHandling()
		{
			return (orgAddress == null) ? CodeAndDescriptionWrapper.Empty : new CodeAndDescriptionWrapper(orgAddress.OA_ContainerHandling, orgAddress.OA_ContainerHandling_List, Factory);
		}

		protected virtual CodeAndDescriptionWrapper GetDockHeight()
		{
			return (orgAddress == null) ? CodeAndDescriptionWrapper.Empty : new CodeAndDescriptionWrapper(orgAddress.OA_Dock_Height, orgAddress.OA_Dock_Height_List, Factory);
		}

		protected virtual CodeAndDescriptionWrapper GetLabourRequired()
		{
			return (orgAddress == null) ? CodeAndDescriptionWrapper.Empty : new CodeAndDescriptionWrapper(orgAddress.OA_LabourRequired, orgAddress.OA_LabourRequired_List, Factory);
		}

		#endregion

		#region SupplyChainSecurityConfiguration

		SupplyChainSecurityConfiguration SupplyChainSecurityConfiguration => supplyChainSecurityConfiguration ?? (supplyChainSecurityConfiguration = SupplyChainSecurityConfiguration.New());
		SupplyChainSecurityConfiguration supplyChainSecurityConfiguration;

		#endregion

		#region Implementation

		readonly OrgAddress orgAddress;
		readonly JobDocAddress jobDocAddress;
		readonly ContactType contactType;

		#endregion
	}
}
