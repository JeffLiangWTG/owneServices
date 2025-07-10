using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GB.Business.Messaging;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.GB.Chief.Messaging
{
	public abstract class GbChiefHeader : GbHeader
	{
		public GbChiefHeader(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
		}

		public override ZString TransportNationalityAtTheBorderBox21
		{
			get { return ConvertFromUnToChiefCountry(base.TransportNationalityAtTheBorderBox21); }
		}

		public override ZString CountryOfArrival
		{
			get { return ConvertFromUnToChiefCountry(base.CountryOfArrival); }
		}

		public override ZString CountryOfDeparture
		{
			get { return ConvertFromUnToChiefCountry(base.CountryOfDeparture); }
		}

		public override ZString CountryOfDestination
		{
			get { return ConvertFromUnToChiefCountry(base.CountryOfDestination); }
		}

		public override ZString CountryOfExport
		{
			get { return ConvertFromUnToChiefCountry(base.CountryOfExport); }
		}

		public ZString ConvertFromUnToChiefCountry(ZString unCountry)
		{
			if (!unCountry.IsEmpty)
			{
				var convertedCode = ZZRefCusMapCombined.MapCW1CodeToCustomsCode(EntryHeader.Factory,
					EntryHeader.Declaration.JE_ApplicationCode,
					RefCusMapTypeList.Codes.EUCTY, unCountry, ZDateTime.Today);
				if (!convertedCode.IsEmpty)
				{
					unCountry = convertedCode;
				}
			}
			return unCountry;
		}

		public ZBool IsSupplementaryDeclarationType
		{
			get { return EntryHeader.Declaration.IsSupplementaryDeclarationType; }
		}

		public abstract ZString HMRC_ASG_CODE(CusDecMessageTypeFunction originalOrReplacementOrDelete);
	}
}
