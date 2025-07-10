using System;
using CargoWise.Types;

namespace Enterprise.Accounting.APAutomation.APReconciliation
{
	public class APReconciliationLine : IReconciliationLineIdentifier
	{
		Guid IReconciliationLineIdentifier.LineIdentifier
		{
			get => this.LineIdentifier.ToGuid();
			set => this.LineIdentifier = value;
		}

		public ZGuid LineIdentifier { get; set; } //This can be JobCharge or ConsolCost PK
		public ZString Description { get; set; }
		public APReconciliationLineTypes LineType { get; set; } //This can be the tabel code (JR or E6). 
		public decimal OSExTaxAmount { get; set; }
		public string OSCurrency { get; set; }
		public decimal LocalExTaxAmount { get; set; }
		public string LocalCurrency { get; set; }
		public decimal ExchangeRate { get; set; }
		public ZGuid CreditorPk { get; set; }
		public ZGuid ParentId { get; set; }
	}
}
