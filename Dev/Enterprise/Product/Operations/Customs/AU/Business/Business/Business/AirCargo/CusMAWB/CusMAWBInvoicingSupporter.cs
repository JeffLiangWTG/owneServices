using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Customs.AU.Declaration.Business;

public class CusMAWBInvoicingSupporter : JobInvoicingSupporter
{
	public CusMAWBInvoicingSupporter(CusMAWB parent)
		: base(parent)
	{
		Parent = parent;
	}

	protected readonly CusMAWB Parent;

	public override RefUNLOCO Destination
	{
		get { return Parent.DischargePort; }
	}

	public override ZString ConsolType
	{
		get { return Core.Constants.JobInvoicingDefaultDepartmentConsolType.NoConsol; }
	}

	public override ZString TransportMode
	{
		get { return Core.Constants.TransportCodes.Air; }
	}

	public override ZDateTime ETA
	{
		get { return Parent.CM_ArrivalDate; }
	}

	public override JobInvoicingConsumerType ConsumerType
	{
		get { return JobInvoicingConsumerTypes.CusMAWB; }
	}

	public override RefUNLOCO Origin
	{
		get { return Parent.LoadPort; }
	}

	public override ZString MasterBillNumber
	{
		get { return Parent.CM_MAWB; }
	}

	public override bool IsImport
	{
		get { return Parent.CM_RL_NKDischargePort == GlbBranch.CurrentBranch.GB_RL_NKHomePort; }
	}

	public override bool IsExport
	{
		get { return Parent.CM_RL_NKLoadPort == GlbBranch.CurrentBranch.GB_RL_NKHomePort; }
	}

	public override bool IsDomestic
	{
		get { return Parent.CM_RL_NKLoadPort == GlbBranch.CurrentBranch.GB_RL_NKHomePort && Parent.CM_RL_NKDischargePort == GlbBranch.CurrentBranch.GB_RL_NKHomePort; }
	}

	protected override SecurityCheckpoint GetAuditSecurityCore()
	{
		return Env.Security.ACAMasterImportAuditBilling;
	}

	protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
	{
		return Env.Security.ACAMasterImportJobInvoicing;
	}

	public override ZDateTime GetOperationsSignificantDate(string significantDateCode)
	{
		return ZDateTime.Empty;
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
