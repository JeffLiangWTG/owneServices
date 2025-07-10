using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.CommissionManagement.Business
{
	public interface ICommissionPayable : IDocumentSupportable
	{
		BusinessObjectFactory Factory { get; }
		ZString BatchNumber { get; }
		IEnumerable<ViewCommissionLine> CommissionLinesForPayment { get; }
	}
}
