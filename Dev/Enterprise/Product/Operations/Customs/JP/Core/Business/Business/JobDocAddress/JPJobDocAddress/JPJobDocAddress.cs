using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.JP.Business
{
	public class JPJobDocAddress : JobDocAddress
	{
		public JPJobDocAddress(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ResourceStringData("JP.JPJobDocAddress.E2_GovRegNumType", Caption = "Code")]
		[List(nameof(Lookups) + "." + nameof(JPJobDocAddressLookups.GovRegNumTypes))]
		[MaxLength(3)]
		public override ZString E2_GovRegNumType
		{
			get => base.E2_GovRegNumType;
			set => base.E2_GovRegNumType = value;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Used for params so the array is required.")]
		public ZString[] RequiredCusCodeTypes => Lookups.GovRegNumTypes.GetAllCodesZString();

		[ResourceStringData("JP.JPJobDocAddress.E2_CompanyName", Caption = "Company Name")]
		public override ZString E2_CompanyName { get => base.E2_CompanyName; set => base.E2_CompanyName = value; }

		[ResourceStringData("JP.JPJobDocAddress.E2_AdditionalAddressInformation", Caption = "Note")]
		public override ZString E2_AdditionalAddressInformation { get => base.E2_AdditionalAddressInformation; set => base.E2_AdditionalAddressInformation = value; }

		[ResourceStringData("JP.JPJobDocAddress.E2_Address1", Caption = "Address Line 1")]
		public override ZString E2_Address1 { get => base.E2_Address1; set => base.E2_Address1 = value; }

		[ResourceStringData("JP.JPJobDocAddress.E2_Address2", Caption = "Address Line 2")]
		public override ZString E2_Address2 { get => base.E2_Address2; set => base.E2_Address2 = value; }

		public override ZBool E2_AddressOverride
		{
			get => base.E2_AddressOverride;
			set
			{
				var isChanged = E2_AddressOverride != value;
				if (isChanged)
				{
					var address = Address;
					base.E2_AddressOverride = value;
					if (!value)
					{
						DocAddressNumbers.RemoveAndDeleteAll();
					}
					else
					{
						SetOverrideDocAddressNumbersFromOrg(address);
					}
				}
			}
		}

		void SetOverrideDocAddressNumbersFromOrg(OrgAddress address)
		{
			if (E2_AddressType == DocAddressTypes.Codes.AirCargoAgent)
			{
				LocationCode = GetAirCargoAgentOrgCusCode(address)?.OK_CustomsRegNo ?? ZString.Empty;
			}
		}

		#region Air Cargo Agent

		OrgCusCode GetAirCargoAgentOrgCusCode(OrgAddress address)
		{
			return address?.CustomsCodes?.GetOrgCusCodeObjectMatchingCountryAndCodes(CountryCodes.Japan, OrgCusCode.JapanCodeTypes.AAL);
		}

		[MaxLength(3)]
		public ZString LocationCode
		{
			get => (!E2_AddressOverride && HasRealAddress) ? (GetAirCargoAgentOrgCusCode(Address)?.OK_CustomsRegNo ?? ZString.Empty) : (AirCargoAgentAddressNumber?.E2N_Number ?? ZString.Empty);
			set
			{
				if (E2_AddressOverride)
				{
					var oldValue = LocationCode;
					if (oldValue != value)
					{
						CheckMaximumLength(LocationCodeInfo, value);
						if (value.IsEmpty)
						{
							TryDeleteAddressNumber(airCargoAgentAddressNumber);
						}
						else if (airCargoAgentAddressNumber == null)
						{
							airCargoAgentAddressNumber = DocAddressNumbers.FindOrCreate(OrgCusCode.JapanCodeTypes.AAL, Core.Constants.CountryCodes.Japan);
							RegisterEditableChildObject(airCargoAgentAddressNumber);
							airCargoAgentAddressNumber.E2N_Number = value;
						}
						else
						{
							airCargoAgentAddressNumber.E2N_Number = value;
						}
					}
					if (!IsValidationSuspended)
					{
						Validation.ValidateLocationCode();
					}
				}
				LocationCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo LocationCodeInfo => GetZPropertyInfo(nameof(LocationCode));

		public ZString LocationCodeType => (!E2_AddressOverride && HasRealAddress) ? (GetAirCargoAgentOrgCusCode(Address)?.OK_CodeType ?? ZString.Empty) : OrgCusCode.JapanCodeTypes.AAL;

		public ZPropertyInfo LocationCodeTypeInfo => GetZPropertyInfo(nameof(LocationCodeType));

		JobDocAddressNumber AirCargoAgentAddressNumber
		{
			get
			{
				if (airCargoAgentAddressNumber == null || airCargoAgentAddressNumber.IsDeleted)
				{
					airCargoAgentAddressNumber = DocAddressNumbers.Find(OrgCusCode.JapanCodeTypes.AAL, Core.Constants.CountryCodes.Japan);
				}
				return airCargoAgentAddressNumber;
			}
		}
		JobDocAddressNumber airCargoAgentAddressNumber;

		#endregion

		void TryDeleteAddressNumber(JobDocAddressNumber addressNumber)
		{
			if (addressNumber != null && !addressNumber.IsDeleted)
			{
				addressNumber.Delete();
			}
		}

		protected override JobDocAddressLookups GetNewLookups() => new JPJobDocAddressLookups(this);

		public new JPJobDocAddressLookups Lookups => (JPJobDocAddressLookups)base.Lookups;

		protected override JobDocAddressValidation GetNewValidation() => new JPJobDocAddressValidation(this);

		public new JPJobDocAddressValidation Validation => (JPJobDocAddressValidation)base.Validation;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			if (E2_GovRegNumType == GovRegNumTypeDefaultValueDEF)
			{
				E2_GovRegNumTypeInfo.ClearValue();
			}
		}

		const string GovRegNumTypeDefaultValueDEF = "DEF";
	}
}
