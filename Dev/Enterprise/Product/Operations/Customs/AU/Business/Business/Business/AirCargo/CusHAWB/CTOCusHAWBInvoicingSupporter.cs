using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Customs.AU.Declaration.Business;

public class CTOCusHAWBInvoicingSupporter : JobInvoicingSupporter
{
	public CTOCusHAWBInvoicingSupporter(CTOCusHAWB parent)
		: base(parent)
	{
		Parent = parent;
	}

	protected readonly CTOCusHAWB Parent;

	public override ZDecimal ActualChargeable
	{
		get { return Parent.CS_ChargableWeight; }
	}

	public override ZString ActualChargeableUnit
	{
		get { return Parent.CS_WeightUQ; }
	}

	public override ZDecimal ActualWeight
	{
		get { return Parent.CS_Weight; }
	}

	public override ZString ActualWeightUnit
	{
		get { return Parent.CS_WeightUQ; }
	}

	protected override SecurityCheckpoint GetAuditSecurityCore()
	{
		return Env.Security.AUCustomsAirCTOImportAuditBilling;
	}

	public override ZString ConsolType
	{
		get { return Core.Constants.JobInvoicingDefaultDepartmentConsolType.NoConsol; }
	}

	public override JobInvoicingConsumerType ConsumerType
	{
		get { return JobInvoicingConsumerTypes.CTOCusImportHAWB; }
	}

	public override ZDateTime ETA
	{
		get { return Parent.MAWB == null ? ZDateTime.Empty : Parent.MAWB.CM_ArrivalDate; }
	}

	public override ZString HouseBillNumber
	{
		get { return Parent.CS_HAWB; }
	}

	public override bool IsImport
	{
		get { return ImportExportHelper.IsImport(Parent.CS_RL_NKOrigin, Parent.CS_RL_NKDestination); }
	}

	public override bool IsExport
	{
		get { return ImportExportHelper.IsExport(Parent.CS_RL_NKOrigin, Parent.CS_RL_NKDestination); }
	}

	public override bool IsDomestic
	{
		get { return ImportExportHelper.IsDomestic(Parent.CS_RL_NKOrigin, Parent.CS_RL_NKDestination); }
	}

	protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
	{
		return Env.Security.AUCustomsAirCTOImportJobInvoicing;
	}

	public override ZString MasterBillNumber
	{
		get { return Parent.MAWB == null ? ZString.Empty : Parent.MAWB.CM_MAWB; }
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

	public override RefUNLOCO Origin
	{
		get
		{
			return Parent.Origin;
		}
	}

	public override RefUNLOCO Destination
	{
		get
		{
			return Parent.Destination;
		}
	}
}
