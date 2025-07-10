using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class SecondCusBondDetailCollection : ActiveBusinessObjectCollection<SecondCusBondDetail>
	{
		readonly CusEntryInstruction instruction;

		public SecondCusBondDetailCollection(CusEntryInstruction instruction)
			: base(instruction.Factory, instruction, new ZQuery(CusBondDetailSchema.PW_ApplicationCode, SecondCusBondDetail.ApplicationCode), CusBondDetailSchema.PW_ParentID)
		{
			this.instruction = instruction;
		}

		protected override bool AllowNew => instruction.GetGuarantee(false)?.CanAddReleaseGuarantees ?? false;

		protected override void OnAdded(SecondCusBondDetail businessObject)
		{
			base.OnAdded(businessObject);
			if (instruction.GetGuarantee(false) is CusBondDetail guarantee)
			{
				businessObject.PW_CPH_Guarantee = guarantee.PW_CPH_Guarantee;
				businessObject.PW_Status = guarantee.PW_Status;
			}
		}

		protected override void EndNew(int index)
		{
			if (index >= 0 && index < Count)
			{
				SecondCusBondDetail businessObject = this[index];
				if (IsNonCommittedElement(businessObject))
				{
					instruction.MarkNeedAddGuaranteeTransactions();
					var hasChange = businessObject.HasChanges;
					base.EndNew(index);
					if (hasChange && MatchesFilter(businessObject, false) && !IsNonCommittedElement(businessObject))
					{
						if (!instruction.LockGuaranteeManagementMutex(false))
						{
							instruction.WarnConcurrency(Res.GetString("c06f7136-884e-4270-a37e-9774634a5848", "{0} will be deleted. Because {1}", businessObject.HumanReadableName, instruction.GuaranteeManagementMutexText));
							Delete(businessObject);
						}
					}
				}
			}
		}
	}
}
