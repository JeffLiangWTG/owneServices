using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.CA.Messaging;

namespace Enterprise.Customs.CA.Business
{
	public interface ICSARevenueSummaryForm : ICAEDIFACTMessageAttachee
	{
		ZDateTime DocumentMessageDateTime { get; }
		ZDateTime RSFMonth { get; }
		ZDateTime PeriodStartDateTime { get; }
		ZDateTime PeriodEndDateTime { get; }
		ZString BusinessNumber { get; }
		ZString StatementNumber { get; }
		ZDecimal VFD { get; }
		IEnumerable<ICSARSFItem> Debits { get; }
		IEnumerable<ICSARSFItem> Credits { get; }
		IEnumerable<ICSARSFItem> InterimPayments { get; }
		IEnumerable<ICSARSFItem> CustomsAssessments { get; }
		ZDecimal TotalPayment { get; }
	}
}
