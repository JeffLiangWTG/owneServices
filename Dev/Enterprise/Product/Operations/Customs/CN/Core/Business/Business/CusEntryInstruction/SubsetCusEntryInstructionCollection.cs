using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CN.Business
{
	public class SubsetCusEntryInstructionCollection : SubsetBusinessObjectCollection<CusEntryInstruction>, ICusEntryInstructionCollection<CusEntryInstruction>
	{
		public SubsetCusEntryInstructionCollection(JobDeclaration declaration, Predicate<CusEntryInstruction> predicate) : base(declaration.CustomsEntryInstructions)
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
		public IEnumerator<CusEntryInstruction> GetEnumerator()
		{
			return Elements.Cast<CusEntryInstruction>().GetEnumerator();
		}
	}
}
