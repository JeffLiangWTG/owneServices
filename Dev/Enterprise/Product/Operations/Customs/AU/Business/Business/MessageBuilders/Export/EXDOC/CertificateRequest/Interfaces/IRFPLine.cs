using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	interface IRFPLine
	{
		ZInt RFPLineNumber { get; }
		ZDecimal NetLineQuantity { get; }
		ZString NetQuantityUnit { get; }
		ZDecimal PackQuantity { get; }
		ZString PackType { get; }
		IEnumerable<IRFPContainer> Containers { get; }
	}
}
