using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	public class SubsetCusEntryInstructionCollection : SubsetBusinessObjectCollection<CusEntryInstruction>
	{
		public SubsetCusEntryInstructionCollection(CusEntryInstructionCollection<CusEntryInstruction> entryInsctructions, Predicate<CusEntryInstruction> predicate) : base(entryInsctructions)
		{
			this.predicate = predicate;
			Rebuild();
		}

		readonly Predicate<CusEntryInstruction> predicate;

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var instruction = element as CusEntryInstruction;
			return instruction != null && (predicate?.Invoke(instruction) ?? true);
		}

		protected override void RebuildOnConstruction()
		{
		}

		protected override bool AllowNewCore => false;
		protected override bool AllowRemoveCore => false;
	}
}
