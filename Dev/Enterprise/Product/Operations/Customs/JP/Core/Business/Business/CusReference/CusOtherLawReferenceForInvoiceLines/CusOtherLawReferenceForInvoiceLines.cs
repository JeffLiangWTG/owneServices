using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Common;

namespace Enterprise.Customs.JP.Business;

public class CusOtherLawReferenceForInvoiceLines(BusinessObjectFactory factory, DataRow row) : CusOtherLawReference(factory, row)
{
	protected override CusReferenceValidation GetNewValidation() => new CusOtherLawReferenceForInvoiceLinesValidation(this);

	new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

	public override ZString CFR_Reference
	{
		get => base.CFR_Reference;
		set
		{
			var oldValue = base.CFR_Reference;
			base.CFR_Reference = value;
			if (oldValue != value && Parent?.EntryInstruction is CusEntryInstruction entryInstruction && IsRoadTranportVehicleLaw && !entryInstruction.ApprovalCertificateInfos.Cast<ApprovalCertificateInfo>().Any(x => x.CSI_Code == ApprovalCertificateInfoCodes.MOTS))
			{
				var certificate = entryInstruction.ApprovalCertificateInfos.AddNew();
				certificate.CSI_Code = ApprovalCertificateInfoCodes.MOTS;
			}
		}
	}
}
