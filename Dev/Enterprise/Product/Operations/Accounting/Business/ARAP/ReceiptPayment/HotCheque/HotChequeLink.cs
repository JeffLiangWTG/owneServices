using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.HotCheque;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	public class HotChequeLink : NonPersistentBusinessObject, IObsoleteValidation
	{
		public HotChequeLink(AccHotChequeCollection hotCheques)
			: base(hotCheques.Factory)
		{
			fHotCheques = hotCheques;
		}

		public AccHotChequeCollection HotCheques
		{
			get { return fHotCheques; }
		}

		readonly AccHotChequeCollection fHotCheques;
	}
}
