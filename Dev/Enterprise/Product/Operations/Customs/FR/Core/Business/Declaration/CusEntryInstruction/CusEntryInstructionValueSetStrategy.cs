using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class CusEntryInstructionValueSetStrategy : IValueSetStrategy
	{
		public CusEntryInstructionValueSetStrategy(CusEntryInstruction entryInstruction)
		{
			EntryInstruction = entryInstruction;
		}

		protected readonly CusEntryInstruction EntryInstruction;

		#region IValueSetStrategy Members

		public void ValueSet(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			ValueSetCore(valueThatHasChanged, oldValue);
		}

		#endregion

		protected virtual void ValueSetCore(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
		}
	}
}
