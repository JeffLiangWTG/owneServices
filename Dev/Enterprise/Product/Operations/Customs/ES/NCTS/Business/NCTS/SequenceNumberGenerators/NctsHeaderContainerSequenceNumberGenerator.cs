using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsHeaderContainerSequenceNumberGenerator : ShortSequenceNumberGenerator
	{
		public NctsHeaderContainerSequenceNumberGenerator(Func<IEnumerable<NctsDepartureHeaderContainer>> getLines) : base(getLines)
		{
		}

		protected override IEnumerable<IShortSequenceNumberLine> OrderedLinesForReCalculateAll => Lines.OrderBy(x => ((NctsDepartureHeaderContainer)x).BC_Mode).ThenBy(x => x.SequenceNumber);
	}
}
