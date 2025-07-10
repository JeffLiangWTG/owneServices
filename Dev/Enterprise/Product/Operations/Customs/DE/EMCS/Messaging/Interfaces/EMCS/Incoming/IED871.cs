using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.DE.EMCS.Messaging
{
	public interface IED871 : IEmcsDataProvider
	{
		IEMCSEvent ExciseMovement { get; }

		ZString GlobalExplanation { get; }

		IReadOnlyCollection<IED871BodyAnalysis> Lines { get; }
	}

	public interface IED871BodyAnalysis
	{
		ZString LineNumber { get; }

		ZString ExciseProductCode { get; }

		ZDecimal ActualQuantity { get; }

		ZString Explanation { get; }
	}
}
