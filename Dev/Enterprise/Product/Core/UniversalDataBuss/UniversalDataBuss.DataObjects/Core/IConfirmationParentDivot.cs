using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public interface IConfirmationParentDivot : IDataObject
	{
		ZInt? Quantity { get; set; }

		List<Confirmation> ConfirmationCollection { get; set; }
	}
}
