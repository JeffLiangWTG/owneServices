using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	interface ICertificateLine
	{
		ZInt LineNumber { get; }
		ZDecimal NetQuantity { get; }
		ZString NetQuantityUnit { get; }
		ZString ProductCode { get; }
		ZString ProductDescription { get; }
		ZString AdditionalProductDescription { get; }
		ZString[] ExtraCertificates { get; }
		ZDecimal PackQuantity { get; }
		ZString PackType { get; }
		IEnumerable<IRFPNumber> RFPNumbers { get; }
	}
}
