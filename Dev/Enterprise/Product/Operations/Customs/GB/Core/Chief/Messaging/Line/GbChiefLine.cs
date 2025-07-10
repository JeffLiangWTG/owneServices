using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Messaging;

namespace Enterprise.Customs.GB.Chief.Messaging
{
	public abstract class GbChiefLine : GbLine
	{
		protected GbChiefLine(GbChiefHeader header, CusEntryLine actualEntryLine)
			: base(header, actualEntryLine)
		{
			this.header = header;
		}

		protected override ZString GetCountryOfOriginBox34()
		{
			return header.ConvertFromUnToChiefCountry(base.GetCountryOfOriginBox34());
		}
		protected GbChiefHeader header;
	}
}
