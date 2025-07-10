using System;

namespace Enterprise.Accounting.APAutomation.APReconciliation
{
	public interface IReconciliationLineIdentifier
	{
		public Guid LineIdentifier { get; set; } //This can be JobCharge or ConsolCost PK
		public APReconciliationLineTypes LineType { get; set; } //This can be the tabel code (JR or E6). 
	}
}
