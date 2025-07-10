namespace Enterprise.Customs.DE.Business.Declaration
{
	public class CusEntryInstructionComparer : Customs.Business.CusEntryInstructionComparer
	{
		public CusEntryInstructionComparer()
		{
		}

		protected override int CompareCore(Customs.Business.CusEntryInstruction x, Customs.Business.CusEntryInstruction y)
		{
			var result = x.CEI_Style.CompareTo(y.CEI_Style);
			if (result == 0)
			{
				result = x.CEI_SubStyle.CompareTo(y.CEI_SubStyle);
			}
			return result;
		}
	}
}
