using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImport5TMLine
	{
		ZInt EntryLineNo { get; }
		ZString HSCode { get; }
		ZString HSDescription { get; }
		ZDecimal NetWeightKG { get; }
		ZDecimal ValueForVAT { get; }
		ZDecimal VAT { get; }
	}
}
