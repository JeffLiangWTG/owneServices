using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	interface IRFPNumber
	{
		ZString RFPNumber { get; }
		IEnumerable<IRFPLine> RFPLines { get; }
	}
}
