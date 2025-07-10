using System;
using System.Diagnostics;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;

namespace Enterprise.Client.EDI.Billing
{
	[DebuggerDisplay("{TransactionType}, {ClientID}, {ClientNumber}, {TransactionTime}, {ABMCompanyCode}, {JurisdictionCode}, {ProcedureCode}, {Department}, {DocumentReference}, {TransactionCount}")]
	public class ABMCustomsData
	{
		public string TransactionType { get; set; }
		public string ClientID { get; set; }
		public string ClientNumber { get; set; }
		public DateTime PeriodStart { get; set; }
		public string ABMCompanyCode { get; set; }
		public string JurisdictionCode { get; set; }
		public string ProcedureCode { get; set; }
		public string Department { get; set; }
		public string DocumentReference { get; set; }
		public int TransactionCount { get; set; }

		public LicenceHeader LicHeader { get; set; }
		public ClientCompany Company { get; set; }

		public ZGuid ClientCompanyPk
		{
			get { return Company != null ? Company.PK : ZGuid.Empty; }
		}

		public string GroupingKey1
		{
			get { return JurisdictionCode; }
		}

		public string GroupingKey2
		{
			get { return TransactionType == ABMCustomsTransactionTypes.Codes.PortCommunity ? ProcedureCode : string.Empty; }
		}

		public string GroupingKey3
		{
			get { return Department; }
		}
	}
}
