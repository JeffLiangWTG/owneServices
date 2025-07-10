using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class GuaranteeForEntryInstructionCollection : EU.Business.Declaration.GuaranteeForEntryInstructionCollection
	{
		public GuaranteeForEntryInstructionCollection(CusEntryInstruction cusEntryInstruction) : base(cusEntryInstruction)
		{
		}

		public new GuaranteeForEntryInstruction this[int index] => (GuaranteeForEntryInstruction)Elements[index];

		protected override BusinessObject AddNewCore() => AddNew(typeof(GuaranteeForEntryInstruction));

		public new GuaranteeForEntryInstruction AddNew() => (GuaranteeForEntryInstruction)base.AddNew();

		protected new GuaranteeForEntryInstruction AddNew(Type type) => (GuaranteeForEntryInstruction)base.AddNew(type);
	}
}
