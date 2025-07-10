using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public partial class JobCosting : IDataObject
	{
		public JobCosting()
		{
		}

		public JobCosting(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		public Branch Branch { get; set; }
		public Department Department { get; set; }
		public Staff SalesStaff { get; set; }
		public Staff OperationsStaff { get; set; }
		public Branch HomeBranch { get; set; }
		public Currency Currency { get; set; }
		[MaxLength(35)]
		public ZString? ClientContractNumber { get; set; }
		public ZDecimal? TotalRevenue { get; set; }
		public ZDecimal? AgentRevenue { get; set; }
		public ZDecimal? LocalClientRevenue { get; set; }
		public ZDecimal? OtherDebtorRevenue { get; set; }
		public ZDecimal? TotalCost { get; set; }
		public ZDecimal? TotalAccrual { get; set; }
		public ZDecimal? AccrualRecognized { get; set; }
		public ZDecimal? AccrualNotRecognized { get; set; }
		public ZDecimal? TotalWIP { get; set; }
		public ZDecimal? WIPRecognized { get; set; }
		public ZDecimal? WIPNotRecognized { get; set; }
		public ZDecimal? TotalJobProfit { get; set; }
		public List<ChargeLine> ChargeLineCollection { get; private set; }
		public List<CashAdvanceRequestHeader> CashAdvanceRequestHeaderCollection { get; private set; }
		public Branch TaxBranch { get; set; }
	}
}
