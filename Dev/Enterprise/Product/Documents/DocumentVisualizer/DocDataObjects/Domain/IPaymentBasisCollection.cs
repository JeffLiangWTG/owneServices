using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public interface IPaymentBasisCollection : IReadOnlyCollection<IPaymentBasis>
	{
		ZString Quantity { get; }
		ZString CalculationBasis { get; }
	}
}
