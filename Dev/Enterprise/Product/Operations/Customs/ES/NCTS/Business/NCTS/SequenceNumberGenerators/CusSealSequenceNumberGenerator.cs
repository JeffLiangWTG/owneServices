using System;
using System.Collections.Generic;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class CusSealSequenceNumberGenerator : ShortSequenceNumberGenerator
	{
		public CusSealSequenceNumberGenerator(Func<IEnumerable<EU.NCTS.Business.CusSeal>> getLines) : base(getLines)
		{
		}

		protected override int SequenceStartingNumberCore => 3;
	}
}
