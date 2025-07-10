using CargoWise.Types;
using Enterprise.Customs.FR.Business.OperationalActions;
using Enterprise.Customs.FR.Messaging.Interfaces.COD;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.COD
{
	public class DocAapurerWrapper : IDocAapurer
	{
		public DocAapurerWrapper(FrDeclarationCreditD48Applicator applicator)
		{
			this.applicator = applicator;
		}
		readonly FrDeclarationCreditD48Applicator applicator;

		public ZString DocumentCode => applicator.D48DocumentCode;

		public ZString DocumentReference => applicator.ReferenceNumber;
	}
}
