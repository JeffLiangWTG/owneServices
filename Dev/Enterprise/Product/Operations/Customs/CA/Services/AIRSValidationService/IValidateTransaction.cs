using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.Customs.CA.Services
{
	public interface IValidateTransaction
	{
		IValueObject Transaction { get; }
		int CommoditiesNumber { get; }

		void AddNewCommodity(IAIRSValidationQueriedLine line);
		void RemoveLastCommodity(string commodityGroupId);
		IEnumerable<ZString> GetAllCommodityGroupIds();
	}
}
