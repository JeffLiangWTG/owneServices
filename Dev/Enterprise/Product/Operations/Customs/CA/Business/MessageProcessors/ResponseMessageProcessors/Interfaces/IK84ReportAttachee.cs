namespace Enterprise.Customs.CA.Business
{
	using CargoWise.Types;

	public interface IK84ReportAttachee
	{
		ZDateTime StatementDate { get; set; }
		ZDateTime AccountingDate { get; set; }
		ZDateTime ConfirmedDate { get; set; }
		ZDateTime B2AcceptedDate { get; set; }
		ZString MessageType { get; }
	}
}
