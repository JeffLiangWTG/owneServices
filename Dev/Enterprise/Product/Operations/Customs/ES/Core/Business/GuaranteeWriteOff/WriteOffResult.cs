using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business
{
	public class WriteOffResult : NonPersistentBusinessObject
	{
		public WriteOffResult(ZString jobNumber, ZString mrn, ZString guaranteeStatus, ZString declarationStatus)
		{
			JobNumber = jobNumber;
			Mrn = mrn;
			GuaranteeStatus = guaranteeStatus;
			DeclarationStatus = declarationStatus;
		}

		public ZString JobNumber { get; }

		public ZString Mrn { get; }

		public ZString GuaranteeStatus { get; }

		public ZString DeclarationStatus { get; }
	}
}
