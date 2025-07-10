using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class CusEntryInstructionCollection : EU.Business.Declaration.CusEntryInstructionCollection<CusEntryInstruction>
	{
		public CusEntryInstructionCollection(JobDeclaration parentBO) : base(parentBO)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var currentCEI = ((CusEntryInstruction)child);
			currentCEI.CEI_DateForDuty = ZDateTime.Today;

			if (Count > 0)
			{
				CusEntryInstruction previousCEI = this[Count - 1];

				SetDefaultFromPreviousCEI(previousCEI, currentCEI);
			}
		}

		void SetDefaultFromPreviousCEI(CusEntryInstruction previousCEI, CusEntryInstruction currentCEI)
		{
			currentCEI.CEI_SubStyle = previousCEI.CEI_SubStyle;
		}
	}
}
