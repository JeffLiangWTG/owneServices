using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	[TestExcludeBusinessObjectsAllHaveTestCases]
	public class CARMSOAProgramAccountDocumentWrapper : NonPersistentBusinessObject
	{
		public CARMSOAProgramAccountDocumentWrapper(CusStatementHeader header, ZString bn15, BusinessObjectFactory factory) : base(factory)
		{
			this.header = header;
			this.Bn15 = bn15;
		}

		readonly CusStatementHeader header;
		public ZString Bn15 { get; private set; }

		public IEnumerable<CusStatementLine> Lines
		{
			get
			{
				var result = new List<CusStatementLine>();
				var lines = header.StatementLines;
				if (lines != null)
				{
					result.AddRange(lines.Where(x => x.B3_ImporterCustomsID == Bn15));
				}
				return result;
			}
		}

		#region Total

		public ZDecimal TotalDuties => Lines.Sum(x => x.B4_CARMDNChargeAmount_Duties);

		public ZDecimal TotalExcise => Lines.Sum(x => x.B4_CARMDNChargeAmount_ExciseTax);

		public ZDecimal TotalExciseDuties => Lines.Sum(x => x.B4_CARMDNChargeAmount_ExciseDuties);

		public ZDecimal TotalSIMA => Lines.Sum(x => x.B4_CARMDNChargeAmount_SIMA);

		public ZDecimal TotalGST => Lines.Sum(x => x.B4_CARMDNChargeAmount_GST);

		public ZDecimal TotalHST => Lines.Sum(x => x.B4_CARMDNChargeAmount_HST);

		public ZDecimal TotalPST => Lines.Sum(x => x.B4_CARMDNChargeAmount_PST);

		public ZDecimal TotalPayments => Lines.Sum(x => x.B4_CARMDNChargeAmount_Payments);

		public ZDecimal TotalOthers => Lines.Sum(x => x.B4_CARMDNChargeAmount_Others);

		public ZDecimal TotalTotalAmount => Lines.Sum(x => x.B4_CARMSOAChargeAmount_Totals);

		#endregion

		#region Summary By Day

		public BusinessObjectCollectionWrapper<CARMSOASummaryByDayDocumentWrapper> SummaryByDay
		{
			get
			{
				var list = new List<CARMSOASummaryByDayDocumentWrapper>();
				foreach (var line in Lines.OrderBy(x => x.B3_DueDate))
				{
					list.Add(new CARMSOASummaryByDayDocumentWrapper(line, line.Factory));
				}

				return new BusinessObjectCollectionWrapper<CARMSOASummaryByDayDocumentWrapper>(list);
			}
		}

		#endregion
	}
}
