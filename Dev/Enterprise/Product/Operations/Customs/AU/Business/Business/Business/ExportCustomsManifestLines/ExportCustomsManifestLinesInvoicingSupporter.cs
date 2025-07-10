using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Customs.AU.Declaration.Business;

public class ExportCustomsManifestLinesInvoicingSupporter : JobInvoicingSupporter
{
	public ExportCustomsManifestLinesInvoicingSupporter(ExportCustomsManifestLines parent)
		: base(parent)
	{
		Parent = parent;
	}

	protected readonly ExportCustomsManifestLines Parent;

	protected override SecurityCheckpoint GetAuditSecurityCore()
	{
		return Env.Security.AUCustomsAirCTOExportAuditBilling;
	}

	public override OrgHeader Consignee
	{
		get { return Parent.ConsigneeDocumentaryAddress.Organisation; }
	}

	public override OrgHeader Consignor
	{
		get { return Parent.ConsignorDocumentaryAddress.Organisation; }
	}

	public override ZString ConsolType
	{
		get { return Core.Constants.JobInvoicingDefaultDepartmentConsolType.NoConsol; }
	}

	public override JobInvoicingConsumerType ConsumerType
	{
		get { return JobInvoicingConsumerTypes.CTOCusExportHAWB; }
	}

	public override RefUNLOCO Destination
	{
		get { return Parent.Header == null ? null : Parent.Header.PortOfDestination; }
	}

	public override ZDateTime ETD
	{
		get { return Parent.Header == null ? ZDateTime.Empty : Parent.Header.ED_DepartureDate; }
	}

	public override ZString HouseBillNumber
	{
		get { return Parent.EL_AirWayBill; }
	}

	public override bool IsExport
	{
		get { return true; }
	}

	protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
	{
		return Env.Security.AUCustomsAirCTOExportJobInvoicing;
	}

	public override RefUNLOCO Origin
	{
		get { return Parent.Header == null ? null : Parent.Header.PortOfDeparture; }
	}

	public override ZString TransportMode
	{
		get { return Core.Constants.TransportModes.Air; }
	}

	public override ZString EditSecurityMessage
	{
		get { return EditSecurityMessageCore; }
	}

	protected virtual ZString EditSecurityMessageCore
	{
		get { return ZString.Empty; }
	}

	public override bool EditSecurityLock
	{
		get { return EditSecurityLockCore; }
	}

	protected virtual bool EditSecurityLockCore
	{
		get { return false; }
	}
}
