using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.Business
{
	public class VanningAddress : JobDocAddress, IShortSequenceNumberLine
	{
		public VanningAddress(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			E2_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;
			E2_GovRegNumType = string.Empty;
			E2_GovRegNum = string.Empty;
			E2_ParentTableCode = CusEntryInstructionSchema.Constants.Prefix;
		}

		[ResourceStringData("JP.VanningAddress.E2_AddressSequence", Caption = "Sequence")]
		public override ZByte E2_AddressSequence
		{
			get => base.E2_AddressSequence;
			set => base.E2_AddressSequence = value;
		}

		[ResourceStringData("JP.VanningAddress.OrganisationPK", Caption = "Organization", ShortCaption = "Org.")]
		[List(nameof(Lookups) + "." + nameof(JobDocAddressLookups.OrgHeader_List))]
		public new ZGuid OrganisationPK
		{
			get => base.OrganisationPK;
			set => base.OrganisationPK = value;
		}

		[ResourceStringData("JP.VanningAddress.E2_OA_Address", Caption = "Address", ShortCaption = "Addr.")]
		public override ZGuid E2_OA_Address
		{
			get => base.E2_OA_Address;
			set => base.E2_OA_Address = value;
		}

		[ResourceStringData("JP.VanningAddress.E2_AddressOverride", Caption = "Override")]
		public override ZBool E2_AddressOverride
		{
			get => base.E2_AddressOverride;
			set
			{
				var regType = E2_GovRegNumType;
				var regNo = E2_GovRegNum;

				base.E2_AddressOverride = value;
				if (value)
				{
					E2_GovRegNum = regNo;
					E2_GovRegNumType = regType;
				}
			}
		}

		[ResourceStringData("JP.VanningAddress.E2_CompanyName", Caption = "Name")]
		[MaxLength(70)]
		public override ZString E2_CompanyName
		{
			get => base.E2_CompanyName;
			set => base.E2_CompanyName = value;
		}

		protected bool E2_CompanyName_ReadOnly => !E2_AddressOverride;

		[ResourceStringData("JP.VanningAddress.E2_City", Caption = "City")]
		[MaxLength(35)]
		public override ZString E2_City
		{
			get => base.E2_City;
			set => base.E2_City = value;
		}

		protected bool E2_City_ReadOnly => !E2_AddressOverride;

		[ResourceStringData("JP.VanningAddress.E2_Address1", Caption = "Street")]
		[MaxLength(35)]
		public override ZString E2_Address1
		{
			get => base.E2_Address1;
			set => base.E2_Address1 = value;
		}

		protected bool E2_Address1_ReadOnly => !E2_AddressOverride;

		[ResourceStringData("JP.VanningAddress.E2_AdditionalAddressInformation", Caption = "Additional Information")]
		[MaxLength(50)]
		public override ZString E2_AdditionalAddressInformation
		{
			get => base.E2_AdditionalAddressInformation;
			set => base.E2_AdditionalAddressInformation = value;
		}

		protected bool E2_AdditionalAddressInformation_ReadOnly => !E2_AddressOverride;

		[ResourceStringData("JP.VanningAddress.E2_GovRegNumType", Caption = "Code Type")]
		[List(nameof(Lookups) + "." + nameof(VanningAddressLookups.GovRegNumTypes))]
		[MaxLength(3)]
		public override ZString E2_GovRegNumType
		{
			get => base.E2_GovRegNumType;
			set => base.E2_GovRegNumType = value;
		}

		protected bool E2_GovRegNumType_ReadOnly => !E2_AddressOverride;

		[ResourceStringData("JP.VanningAddress.E2_GovRegNum", Caption = "Code")]
		[MaxLength(17)]
		[List(nameof(Lookups) + "." + nameof(VanningAddressLookups.JPCustomsControlledPremisesCodeList))]
		public override ZString E2_GovRegNum
		{
			get => base.E2_GovRegNum;
			set => base.E2_GovRegNum = value;
		}

		protected bool E2_GovRegNum_ReadOnly => !E2_AddressOverride;

		[ResourceStringData("JP.VanningAddress.E2_State", Caption = "Prefecture")]
		[MaxLength(15)]
		[List(nameof(Lookups) + "." + nameof(VanningAddressLookups.State_List))]
		public override ZString E2_State
		{
			get => base.E2_State;
			set => base.E2_State = value;
		}

		[ResourceStringData("JP.VanningAddress.E2_RN_NKCountryCode", Caption = "Country")]
		[List(nameof(Lookups) + "." + nameof(VanningAddressLookups.CountryList))]
		public override ZString E2_RN_NKCountryCode
		{
			get => base.E2_RN_NKCountryCode;
			set => base.E2_RN_NKCountryCode = value;
		}

		protected bool E2_RN_NKCountryCode_ReadOnly => !E2_AddressOverride;

		public new int StateCode_MaxLength
		{
			get { return 15; }
		}

		protected bool E2_State_ReadOnly => !E2_AddressOverride;

		public CusEntryInstruction Instruction => Factory.Load<CusEntryInstruction>(E2_ParentID);

		public override ZGuid E2_ParentID
		{
			get => base.E2_ParentID;
			set
			{
				var oldValue = E2_ParentID;
				base.E2_ParentID = value;
				if (!IsCopying && oldValue != E2_ParentID)
				{
					Instruction?.VanningLocationsSeqGenerator?.RecalculateWhenAdded(this);
				}
			}
		}

		ZShort ISequenceNumberLine<ZShort>.SequenceNumber
		{
			get => new ZShort(E2_AddressSequence);
			set => E2_AddressSequence = ZByte.ParseSafe(value.ToString(), ZByte.Zero);
		}

		ZGuid ISequenceNumberLine.FKToHeader => E2_ParentID;

		protected override JobDocAddressValidation GetNewValidation() => new VanningAddressValidation(this);

		protected override JobDocAddressLookups GetNewLookups() => new VanningAddressLookups(this);

		public new VanningAddressLookups Lookups => (VanningAddressLookups)base.Lookups;
	}
}
