using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.ES.Manifest.H7.Business.MessageWrappers.G3.Common;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Interface.G3;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class G3PresentGoodMasterConsignmentWrapper : G3MasterConsignmentWrapper, IG3PresentGoodMasterConsignment
	{
		public G3PresentGoodMasterConsignmentWrapper(IEnumerable<AsycudaBill> bills) : base(bills)
		{
		}

		public IReadOnlyCollection<IG3HouseConsignment> HouseConsignment => houseConsignment ??= bills.Select(o => new G3HouseConsignmentWrapper(o)).ToList().AsReadOnly();
		IReadOnlyCollection<IG3HouseConsignment> houseConsignment;
	}
}
