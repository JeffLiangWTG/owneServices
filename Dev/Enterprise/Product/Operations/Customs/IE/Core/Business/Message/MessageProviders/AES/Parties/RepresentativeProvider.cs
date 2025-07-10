using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AES
{
	public class RepresentativeProvider : IRepresentative
	{
		public static RepresentativeProvider New(OrgAddress representativeAddress, ZString representativeStatus)
		{
			RepresentativeProvider result = null;
			if (representativeAddress?.Header is OrgHeader org)
			{
				result = new RepresentativeProvider(ContactProvider.New(org), representativeStatus, representativeAddress.GetEuIdentificationNumber(Core.Constants.CountryCodes.Ireland, ignoreCountryOfIssuanceIfNotMatched: true));
			}
			return result;
		}

		public static RepresentativeProvider New(JobDocAddress representativeAddress, ZString representativeStatus)
		{
			RepresentativeProvider result = null;
			if (representativeAddress?.Organisation is OrgHeader org)
			{
				result = new RepresentativeProvider(ContactProvider.New(org), representativeStatus, representativeAddress.Address.GetEuIdentificationNumber(Core.Constants.CountryCodes.Ireland, ignoreCountryOfIssuanceIfNotMatched: true));
			}
			return result;
		}

		RepresentativeProvider(IContact representative, string status, string id)
		{
			Contact = representative;
			Status = TranslateStatus(status);
			Id = id;
		}

		public string Status { get; }
		public string Id { get; }
		public IContact Contact { get; }

		static string TranslateStatus(string status)
		{
			var result = string.Empty;
			switch (status?.ToUpper())
			{
				case RepresentationTypeList.Codes._1Self:
					result = Constants.RepresentationTypeList.Self;
					break;
				case RepresentationTypeList.Codes._2Direct:
					result = Constants.RepresentationTypeList.Direct;
					break;
				case RepresentationTypeList.Codes._3Indirect:
					result = Constants.RepresentationTypeList.Indirect;
					break;
			}
			return result;
		}
	}
}
