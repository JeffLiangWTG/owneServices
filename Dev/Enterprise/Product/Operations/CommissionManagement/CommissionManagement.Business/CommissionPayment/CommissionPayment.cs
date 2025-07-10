using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.CommissionManagement.Business
{
	public class CommissionPayment : NonPersistentBusinessObject, ICommissionPayable
	{
		public CommissionPayment(BusinessObjectFactory factory, IEnumerable<ViewCommissionLine> commissionLines)
			: base(factory)
		{
			this.commissionLines = commissionLines;
		}

		public ZString BatchNumber
		{
			get { return ZString.Empty; }
		}

		public IEnumerable<ViewCommissionLine> CommissionLinesForPayment
		{
			get { return commissionLines; }
		}
		readonly IEnumerable<ViewCommissionLine> commissionLines;

		public DocumentSupporter DocumentSupporter
		{
			get { return new CommissionPaymentDocumentSupporter(this); }
		}
	}
}
