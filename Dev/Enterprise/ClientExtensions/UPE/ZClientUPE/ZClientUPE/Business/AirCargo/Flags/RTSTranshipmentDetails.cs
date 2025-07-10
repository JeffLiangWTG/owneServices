using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Client.UPE.Business
{
	public abstract class RTSTranshipmentDetails : UPECusHAWBFlagDetails
	{
		public RTSTranshipmentDetails(UPECusHAWB uPECusHAWB)
			: base(uPECusHAWB)
		{
			InitialiseValues();
		}

		#region New Properties

		#region OriginPort

		[MaxLength(CusHAWB.Schema.CS_RL_NKOriginMaxLength)]
		public ZString OriginPort
		{
			get { return fOriginPort; }
			set
			{
				if (fOriginPort != value)
				{
					CheckMaximumLength(OriginPortInfo, value);
					SetNonPersistentPropertyValue(OriginPortInfo, ref fOriginPort, value);
					ValidateOriginPort();
				}
			}
		}

		public ZPropertyInfo OriginPortInfo
		{
			get { return GetZPropertyInfo(nameof(OriginPort)); }
		}

		public void ValidateOriginPort()
		{
			if (!IsValidationSuspended)
			{
				OriginPortInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(OriginPortInfo);
				ListValidation.ErrorIfInvalidCode(OriginPortInfo, Lookups.OriginPortList);
			}
		}

		protected ZString fOriginPort;

		#endregion

		#region DestinationPort

		[MaxLength(CusHAWB.Schema.CS_RL_NKDestinationMaxLength)]
		public ZString DestinationPort
		{
			get { return fDestinationPort; }
			set
			{
				if (fDestinationPort != value)
				{
					CheckMaximumLength(DestinationPortInfo, value);
					SetNonPersistentPropertyValue(DestinationPortInfo, ref fDestinationPort, value);
					ValidateDestinationPort();
				}
			}
		}

		public ZPropertyInfo DestinationPortInfo
		{
			get { return GetZPropertyInfo(nameof(DestinationPort)); }
		}

		public void ValidateDestinationPort()
		{
			if (!IsValidationSuspended)
			{
				DestinationPortInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(DestinationPortInfo);
				ListValidation.ErrorIfInvalidCode(DestinationPortInfo, Lookups.DestinationPortList);
			}
		}

		ZString fDestinationPort;

		#endregion

		#region Name

		public ZString Name
		{
			get { return fName; }
			set
			{
				if (fName != value)
				{
					CheckMaximumLength(NameInfo, value);
					SetNonPersistentPropertyValue(NameInfo, ref fName, value);
					ValidateName();
				}
			}
		}

		public ZPropertyInfo NameInfo
		{
			get { return GetZPropertyInfo(nameof(Name)); }
		}

		protected abstract int Name_MaxLength { get; }

		public void ValidateName()
		{
			if (!IsValidationSuspended)
			{
				NameInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(NameInfo);
			}
		}

		protected ZString fName;

		#endregion

		#region Street

		public ZString Street
		{
			get { return fStreet; }
			set
			{
				if (fStreet != value)
				{
					CheckMaximumLength(StreetInfo, value);
					SetNonPersistentPropertyValue(StreetInfo, ref fStreet, value);
					ValidateStreet();
				}
			}
		}

		public ZPropertyInfo StreetInfo
		{
			get { return GetZPropertyInfo(nameof(Street)); }
		}

		protected abstract int Street_MaxLength { get; }

		public void ValidateStreet()
		{
			if (!IsValidationSuspended)
			{
				StreetInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(StreetInfo);
			}
		}

		protected ZString fStreet;

		#endregion

		#region Street2

		public ZString Street2
		{
			get { return fStreet2; }
			set
			{
				if (fStreet2 != value)
				{
					CheckMaximumLength(Street2Info, value);
					SetNonPersistentPropertyValue(Street2Info, ref fStreet2, value);
				}
			}
		}

		public ZPropertyInfo Street2Info
		{
			get { return GetZPropertyInfo(nameof(Street2)); }
		}

		protected abstract int Street2_MaxLength { get; }

		protected ZString fStreet2;

		#endregion

		#region City

		public ZString City
		{
			get { return fCity; }
			set
			{
				if (fCity != value)
				{
					CheckMaximumLength(CityInfo, value);
					SetNonPersistentPropertyValue(CityInfo, ref fCity, value);
					ValidateCity();
				}
			}
		}

		public ZPropertyInfo CityInfo
		{
			get { return GetZPropertyInfo(nameof(City)); }
		}

		protected abstract int City_MaxLength { get; }

		public void ValidateCity()
		{
			if (!IsValidationSuspended)
			{
				CityInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(CityInfo);
			}
		}

		protected ZString fCity;

		#endregion

		#region State

		public ZString State
		{
			get { return fState; }
			set
			{
				if (fState != value)
				{
					CheckMaximumLength(StateInfo, value);
					SetNonPersistentPropertyValue(StateInfo, ref fState, value);
				}
			}
		}

		public ZPropertyInfo StateInfo
		{
			get { return GetZPropertyInfo(nameof(State)); }
		}

		protected abstract int State_MaxLength { get; }

		protected ZString fState;

		#endregion

		#region PostCode

		public ZString PostCode
		{
			get { return fPostCode; }
			set
			{
				if (fPostCode != value)
				{
					CheckMaximumLength(PostCodeInfo, value);
					SetNonPersistentPropertyValue(PostCodeInfo, ref fPostCode, value);
					ValidatePostCode();
				}
			}
		}

		public ZPropertyInfo PostCodeInfo
		{
			get { return GetZPropertyInfo(nameof(PostCode)); }
		}

		protected abstract int PostCode_MaxLength { get; }

		public void ValidatePostCode()
		{
			if (!IsValidationSuspended)
			{
				PostCodeInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(PostCodeInfo);
			}
		}

		protected ZString fPostCode;

		#endregion

		#region Country

		public ZString Country
		{
			get { return fCountry; }
			set
			{
				if (fCountry != value)
				{
					CheckMaximumLength(CountryInfo, value);
					SetNonPersistentPropertyValue(CountryInfo, ref fCountry, value);
					ValidateCountry();
				}
			}
		}

		public ZPropertyInfo CountryInfo
		{
			get { return GetZPropertyInfo(nameof(Country)); }
		}

		protected abstract int Country_MaxLength { get; }

		public void ValidateCountry()
		{
			if (!IsValidationSuspended)
			{
				CountryInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(CountryInfo);
				ListValidation.ErrorIfInvalidCode(CountryInfo, Lookups.CountryList);
			}
		}

		ZString fCountry;

		#endregion

		#endregion

		public void UpdateCusHAWBDetails()
		{
			UPECusHAWB.CS_OA_ConsigneeAddress = ZGuid.Empty;
			UPECusHAWB.CS_RL_NKOrigin = OriginPort;
			UPECusHAWB.CS_RL_NKDestination = DestinationPort;
			UPECusHAWB.CS_ConsigneeName = Name;
			UPECusHAWB.CS_ConsigneeStreet = Street;
			UPECusHAWB.CS_ConsigneeStreet2 = Street2;
			UPECusHAWB.CS_ConsigneeCity = City;
			UPECusHAWB.CS_ConsigneeState = State;
			UPECusHAWB.CS_ConsigneePostcode = PostCode;
			UPECusHAWB.CS_RN_NKConsigneeCountry = Country;
		}

		public override void ValidateAll()
		{
			if (!IsValidationSuspended)
			{
				base.ValidateAll();
				ValidateOriginPort();
				ValidateDestinationPort();
				ValidateName();
				ValidateStreet();
				ValidateCity();
				ValidatePostCode();
				ValidateCountry();
			}
		}

		#region NoteReference

		protected override string NoteReference
		{
			get
			{
				StringBuilder builder = new StringBuilder();
				builder.AppendFormat("{0}\r\n", Heading);
				builder.AppendFormat("{0}\r\n", Lines);
				builder.AppendFormat("{0}\r\n", OriginPortReference);
				builder.AppendFormat("{0}\r\n", DestinationPortReference);
				builder.AppendFormat("{0}\r\n", NameReference);
				builder.AppendFormat("{0}\r\n", Address1Reference);
				builder.AppendFormat("{0}\r\n", Address2Reference);
				builder.AppendFormat("{0}\r\n", CityReference);
				builder.AppendFormat("{0}\r\n", StateReference);
				builder.AppendFormat("{0}\r\n", PostCodeReference);
				builder.Append(CountryReference);
				return builder.ToString();
			}
		}

		#region Default Values

		protected abstract ZString DefaultAddressName { get; }
		protected abstract ZString DefaultName { get; }
		protected abstract ZString DefaultStreet { get; }
		protected abstract ZString DefaultStreet2 { get; }
		protected abstract ZString DefaultCity { get; }
		protected abstract ZString DefaultState { get; }
		protected abstract ZString DefaultPostCode { get; }
		protected abstract ZString DefaultCountry { get; }

		#endregion

		#region Segments

		string Heading
		{
			get { return string.Format("Fields               {0, -50}   {1, -50}", "Original", "Changed To"); }
		}

		string Lines
		{
			get { return string.Format("======               {0}   {0}", new string('=', 50)); }
		}

		string OriginPortReference
		{
			get { return string.Format("Origin UNLOCO:       {0, -50}   {1, -50}", UPECusHAWB.CS_RL_NKOrigin, OriginPort); }
		}

		string DestinationPortReference
		{
			get { return string.Format("Destination UNLOCO:  {0, -50}   {1, -50}", UPECusHAWB.CS_RL_NKDestination, DestinationPort); }
		}

		string NameReference
		{
			get { return string.Format(DefaultAddressName + " Name:      {0, -50}   {1, -50}", DefaultName, Name); }
		}

		string Address1Reference
		{
			get { return string.Format(DefaultAddressName + " Address:   {0, -50}   {1, -50}", DefaultStreet, Street); }
		}

		string Address2Reference
		{
			get { return string.Format("                     {0, -50}   {1, -50}", DefaultStreet2, Street2); }
		}

		string CityReference
		{
			get { return string.Format(DefaultAddressName + " City:      {0, -50}   {1, -50}", DefaultCity, City); }
		}

		string StateReference
		{
			get { return string.Format(DefaultAddressName + " State:     {0, -50}   {1, -50}", DefaultState, State); }
		}

		string PostCodeReference
		{
			get { return string.Format(DefaultAddressName + " Post Code: {0, -50}   {1, -50}", DefaultPostCode, PostCode); }
		}

		string CountryReference
		{
			get { return string.Format(DefaultAddressName + " Country:   {0, -50}   {1, -50}", DefaultCountry, Country); }
		}

		#endregion

		#endregion

		public new RTSTranshipmentDetailsLookups Lookups
		{
			get { return (RTSTranshipmentDetailsLookups)base.Lookups; }
		}

		protected override UPECusHAWBFlagDetailsLookups GetNewLookups()
		{
			return new RTSTranshipmentDetailsLookups(this);
		}

		protected void InitialiseValues()
		{
			fOriginPort = UPECusHAWB.CS_RL_NKOrigin;
			fDestinationPort = UPECusHAWB.CS_RL_NKDestination;
			fName = DefaultName;
			fStreet = DefaultStreet;
			fStreet2 = DefaultStreet2;
			fCity = DefaultCity;
			fState = DefaultState;
			fPostCode = DefaultPostCode;
			fCountry = DefaultCountry;
		}
	}
}
