namespace Enterprise.Customs.BR.Business
{
	public partial class DetailWithoutLegalDocList
	{
		public static bool RequiresDetailWithoutLegalDocInJustification(string code)
		{
			switch (code)
			{
				case DetailWithoutLegalDocList.Codes._3020:
				case DetailWithoutLegalDocList.Codes._3021:
				case DetailWithoutLegalDocList.Codes._3026:
					return true;
				default:
					return false;
			}
		}
	}
}
