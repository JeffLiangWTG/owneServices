using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class CusEntryInstructionCollection : CusEntryInstructionCollection<CusEntryInstruction>, ICusEntryInstructionCollection<CusEntryInstruction>
{
	public CusEntryInstructionCollection(JobDeclaration jobDeclaration) : base(jobDeclaration)
	{
		RefreshMaxCount();
	}

	JobDeclaration JobDeclaration => (JobDeclaration)Master;

	public void RefreshMaxCount()
	{
		var maxCount = JobDeclaration.AreMultipleEntryInstructionsAllowed ? -1 : 1;
		this.EnableMaxCountValidation(maxCount, warnAtHalfway: false, notificationType: NotificationType.Error, Res.GetString("57868AE2-57DB-43AE-A5C7-5B4F08ADBD75", "You cannot enter more than one Entry Instruction when Shipment Type is {0}", JobDeclaration.JE_MessageType));
	}
}
