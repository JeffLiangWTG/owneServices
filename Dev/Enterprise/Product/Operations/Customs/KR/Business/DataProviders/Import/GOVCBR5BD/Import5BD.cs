using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class Import5BD : IImport5BDHeader
	{
		public string ImportDeclarationNumber { get; set; }
		public string RequestReason { get; set; }
		public string SecurityType { get; set; }
		public DateTime SecurityStartDate { get; set; }
		public DateTime SecurityEndDate { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public decimal SecurityAmount { get; set; }
		public string OtherSecurityType { get; set; }
		public string ReasonForEarlyRemoval { get; set; }
		ZString IImport5BDHeader.ImportDeclarationNumber => ImportDeclarationNumber;
		ZString IImport5BDHeader.RequestReason => RequestReason;
		ZString IImport5BDHeader.SecurityType => SecurityType;
		ZDate IImport5BDHeader.SecurityStartDate => new ZDate(SecurityStartDate);
		ZDate IImport5BDHeader.SecurityEndDate => new ZDate(SecurityEndDate);
		ZDecimal IImport5BDHeader.SecurityAmount => SecurityAmount;
		ZString IImport5BDHeader.OtherSecurityType => OtherSecurityType;
		ZString IImport5BDHeader.ReasonForEarlyRemoval => ReasonForEarlyRemoval;
	}
}
