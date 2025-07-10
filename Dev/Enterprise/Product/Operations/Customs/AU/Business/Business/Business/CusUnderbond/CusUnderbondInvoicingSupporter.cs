using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Customs.AU.Declaration.Business;

public class CusUnderbondInvoicingSupporter : JobInvoicingSupporter
{
	public CusUnderbondInvoicingSupporter(CusUnderbond parent)
		: base(parent)
	{
		Parent = parent;
	}

	protected readonly CusUnderbond Parent;

	public override RefUNLOCO Destination
	{
		get { return Parent.DischargePort; }
	}

	public override ZString TransportMode
	{
		get { return Core.Constants.TransportCodes.Air; }
	}

	public override ZDateTime ETA
	{
		get { return Parent.C4_ArrivalDate; }
	}

	public override JobInvoicingConsumerType ConsumerType
	{
		get { return JobInvoicingConsumerTypes.CusUnderbond; }
	}

	public override RefUNLOCO Origin
	{
		get { return Parent.LoadPort; }
	}

	public override bool IsImport
	{
		get { return Parent.C4_RL_NKDischargePort == GlbBranch.CurrentBranch.GB_RL_NKHomePort; }
	}

	public override bool IsExport
	{
		get { return Parent.C4_RL_NKLoadPort == GlbBranch.CurrentBranch.GB_RL_NKHomePort; }
	}

	public override bool IsDomestic
	{
		get { return Parent.C4_RL_NKLoadPort == GlbBranch.CurrentBranch.GB_RL_NKHomePort && Parent.C4_RL_NKDischargePort == GlbBranch.CurrentBranch.GB_RL_NKHomePort; }
	}

	public override ZString ConsolType
	{
		get { return Core.Constants.JobInvoicingDefaultDepartmentConsolType.NoConsol; }
	}

	protected override SecurityCheckpoint GetAuditSecurityCore()
	{
		return Env.Security.ACAOutturnBillsAuditBilling;
	}

	protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
	{
		return Env.Security.ACAOutturnBillsJobInvoicing;
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
