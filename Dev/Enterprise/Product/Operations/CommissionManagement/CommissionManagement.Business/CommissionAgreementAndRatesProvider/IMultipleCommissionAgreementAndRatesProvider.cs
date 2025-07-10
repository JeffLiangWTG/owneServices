using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.CommissionManagement.Business
{
	public interface IMultipleCommissionAgreementAndRatesProvider
	{
		Dictionary<ZString, ICommissionAgreementAndRates> ByStream { get; }
	}
}
