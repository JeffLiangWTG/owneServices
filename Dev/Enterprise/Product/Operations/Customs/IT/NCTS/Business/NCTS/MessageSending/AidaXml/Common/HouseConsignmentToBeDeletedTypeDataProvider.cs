using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class HouseConsignmentToBeDeletedTypeDataProvider : HouseConsignmentToBeDeletedTypeDataProviderAbstractClass
{
	public HouseConsignmentToBeDeletedTypeDataProvider(int houseConsignmentNumberToBeDeleted, IEnumerable<int> articleNumberToBeDeleted)
	{
		HouseConsignmentNumberToBeDeleted = houseConsignmentNumberToBeDeleted;
		ArticleNumberToBeDeleted = articleNumberToBeDeleted?.ToArray();
	}

	public override int HouseConsignmentNumberToBeDeleted { get; }

	public override IReadOnlyCollection<int> ArticleNumberToBeDeleted { get; }
}
