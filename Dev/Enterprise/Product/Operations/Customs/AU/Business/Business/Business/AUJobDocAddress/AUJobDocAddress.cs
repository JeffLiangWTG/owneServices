using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUJobDocAddress : JobDocAddress
	{
		public AUJobDocAddress(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			E2_AddressOverrideInfo.ValueChanged += E2_AddressOverrideInfo_ValueChanged;
			GetDefaultCountryCodeIfEmpty = () => string.Empty;
		}

		#region Schema

		public new class Schema : JobDocAddress.Schema
		{
			public const string ApprovalNumber = "ApprovalNumber";
			public const string EXDOCEstablishmentNumber = "EXDOCEstablishmentNumber";
			public const int EXDOCEstablishmentNumberMaxLength = 6;
		}

		#endregion

		public override bool SupportsDocAddressNumbers => true;

		[ResourceStringData("00E4B99F-AC52-41A6-A188-893B7EDDF7BE", Caption = "Approval Number")]
		[ReadOnlyMember(nameof(RegNumReadOnly))]
		[MaxLength(JobDocAddressNumber.Schema.E2N_NumberMaxLength)]
		[BusinessObjectTestExclude]
		public ZString ApprovalNumber
		{
			get => GetRegNum(OrgCusCode.CodeTypes.EUTracesID);
			set
			{
				SetRegNum(OrgCusCode.CodeTypes.EUTracesID, value);
				ApprovalNumberInfo.RefreshBinding();
			}
		}
		bool RegNumReadOnly => !E2_AddressOverride && OrganisationPK.IsValid;

		public ZPropertyInfo ApprovalNumberInfo
		{
			get
			{
				var approvalNumber = DocAddressNumbers.FindFirstByNumberType(OrgCusCode.CodeTypes.EUTracesID);
				return (E2_AddressOverride && approvalNumber != null) ? GetWrappedZPropertyInfo(Schema.ApprovalNumber, x => approvalNumber.E2N_NumberInfo) : GetZPropertyInfo(Schema.ApprovalNumber);
			}
		}

		[ResourceStringData("Enterprise.Customs.AU.Declaration.Business.AUJobDocAddress|E2_GovRegNum", Caption = "ID")]
		[MaxLength(Schema.EXDOCEstablishmentNumberMaxLength)]
		public ZString EXDOCEstablishmentNumber
		{
			get => E2_GovRegNum;
			set
			{
				if (E2_GovRegNum != value)
				{
					CheckMaximumLength(EXDOCEstablishmentNumberInfo, value);
					if (value.IsEmpty)
					{
						E2_AddressOverride = false;
					}
					else
					{
						var filter = new ZQuery(OrgCusCodeSchema.OK_CustomsRegNo, value);
						filter.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.Australia);
						filter.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber);
						var orgCusCodes = Factory.Load<OrgCusCode>(filter);

						if (orgCusCodes.Length == 1 && orgCusCodes[0].PremisesAddress is { } premisesAddress)
						{
							E2_AddressOverride = false;
							E2_OA_Address = premisesAddress.PK;
						}
						else
						{
							E2_OA_Address = ZGuid.Empty;
							E2_AddressOverride = true;
							E2_GovRegNum = value;
							E2_GovRegNumType = OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber;
							E2_AdditionalAddressInformation = OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber;
						}
					}

					EXDOCEstablishmentNumberInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo EXDOCEstablishmentNumberInfo => GetZPropertyInfo(Schema.EXDOCEstablishmentNumber);

		void E2_AddressOverrideInfo_ValueChanged(object sender, System.EventArgs e)
		{
			if (E2_AddressOverride)
			{
				DefaultRegNum(OrgCusCode.CodeTypes.EUTracesID);
			}
			else
			{
				ApprovalNumber = ZString.Empty;
			}
		}

		void DefaultRegNum(ZString codeType)
		{
			var orgHeader = Factory.Load<OrgHeader>(OrganisationPK);
			var regNo = orgHeader?.CustomsCodes.GetCustomsRegNo(codeType) ?? ZString.Empty;
			if (!regNo.IsEmpty)
			{
				SetRegNum(codeType, regNo);
			}
		}

		void SetRegNum(ZString codeType, ZString regNo)
		{
			var collection = DocAddressNumbers;
			var existingNumber = collection.FindFirstByNumberType(codeType);
			if (regNo.IsEmpty)
			{
				existingNumber?.Delete();
			}
			else
			{
				var jobDocAddressNumToStore = existingNumber ?? collection.AddNew(codeType, Core.Constants.CountryCodes.Australia);
				jobDocAddressNumToStore.E2N_Number = regNo;
			}
		}

		ZString GetRegNum(ZString codeType)
		{
			ZString result;
			if (E2_AddressOverride)
			{
				var addressNumber = DocAddressNumbers.FindFirstByNumberType(codeType);
				result = addressNumber?.E2N_Number ?? ZString.Empty;
			}
			else
			{
				result = Organisation?.CustomsCodes.GetCustomsRegNo(codeType) ?? ZString.Empty;
			}

			return result;
		}
	}
}
