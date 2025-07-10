using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Messaging;

using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class TraderWrapper : ITrader
	{
		public static TraderWrapper New(OrgAddress orgAddress, bool populateTIR, bool isAddressExtended)
			=> orgAddress != null ? new TraderWrapper(orgAddress, populateTIR, isAddressExtended) : null;

		public static TraderWrapper New(JobDocAddress jobDocAddress, bool populateTIR, bool isAddressExtended)
		{
			if (jobDocAddress != null && jobDocAddress.IsValidAddress)
			{
				return jobDocAddress.E2_AddressOverride
					? new TraderWrapper(jobDocAddress, isAddressExtended)
					: new TraderWrapper(jobDocAddress.Address, populateTIR, isAddressExtended);
			}

			return null;
		}

		TraderWrapper(JobDocAddress jobDocAddress, bool isAddressExtended)
		{
			this.jobDocAddress = jobDocAddress;
			this.isAddressExtended = isAddressExtended;
		}

		readonly JobDocAddress jobDocAddress;
		readonly bool isAddressExtended;

		TraderWrapper(OrgAddress orgAddress, bool populateTIR, bool isAddressExtended)
		{
			this.orgAddress = orgAddress;
			this.populateTIR = populateTIR;
			this.isAddressExtended = isAddressExtended;
		}
		readonly OrgAddress orgAddress;
		readonly bool populateTIR;

		public ZString Name
		{
			get
			{
				if (orgAddress != null)
				{
					return orgAddress.Header?.OH_FullName.Left(35) ?? ZString.Empty;
				}
				else
				{
					return jobDocAddress.E2_CompanyName.Left(35);
				}
			}
		}

		public ZString CompanyName
		{
			get
			{
				if (orgAddress != null)
				{
					return orgAddress.CompanyName.Left(35);
				}
				else
				{
					return jobDocAddress.E2_CompanyName.Left(35);
				}
			}
		}

		public ZString StreetAndNumber
		{
			get
			{
				if (isAddressExtended)
				{
					if (orgAddress != null)
					{
						return orgAddress.Address1AndAddress2.Left(70);
					}
					return jobDocAddress.E2_Address1AndE2_Address2.Left(70);
				}
				else
				{
					return orgAddress?.OA_Address1.Left(35) ?? jobDocAddress.E2_Address1.Left(35);
				}
			}
		}

		public ZString PostalCode
		{
			get
			{
				if (orgAddress != null)
				{
					return orgAddress?.OA_PostCode.Left(9) ?? ZString.Empty;
				}
				else
				{
					return jobDocAddress.E2_Postcode.Left(9);
				}
			}
		}

		public ZString City
		{
			get
			{
				if (orgAddress != null)
				{
					return orgAddress?.OA_City.Left(35) ?? ZString.Empty;
				}
				else
				{
					return jobDocAddress.E2_City.Left(35);
				}
			}
		}

		public ZString CountryCode
		{
			get
			{
				if (orgAddress != null)
				{
					return orgAddress?.OA_RN_NKCountryCode ?? ZString.Empty;
				}
				else
				{
					return jobDocAddress.E2_RN_NKCountryCode;
				}
			}
		}

		public ZString NameAndAddressLanguage
		{
			get
			{
				if (orgAddress != null)
				{
					return orgAddress?.Language ?? ZString.Empty;
				}
				else
				{
					return jobDocAddress.Language;
				}
			}
		}

		public ZString TIN => CachedValueHelper.GetValue(ref tin, () => orgAddress?.GetEORI().Left(17) ?? ZString.Empty);
		CachedValue<ZString> tin;

		public ZString HolderIDTIR => CachedValueHelper.GetValue(ref holderIDTIR, () => populateTIR && orgAddress?.Header != null ? orgAddress.Header.GetRegoCodeOfThisOrg(OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers).Left(17) : ZString.Empty);
		CachedValue<ZString> holderIDTIR;

		public ZString RepresentativeCapacity => ZString.Empty;

		public ZString RepresentativeCapacityLanguage => ZString.Empty;
	}
}
