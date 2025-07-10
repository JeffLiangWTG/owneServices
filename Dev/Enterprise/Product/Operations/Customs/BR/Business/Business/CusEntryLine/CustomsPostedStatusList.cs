namespace Enterprise.Customs.BR.Business
{
	public class CustomsPostedStatusList : Customs.Business.EntryLineStatusList
	{
		public new class Codes : Customs.Business.EntryLineStatusList.Codes
		{
			public const string Accepted = "ACC";
			public const string UpdatePending = "UPD";
		}

		public new class Descriptions : Customs.Business.EntryLineStatusList.Descriptions
		{
			public static string Accepted
			{
				get { return Res.GetString("98f3e069-e212-48ec-b317-85780edcaa68", "Accepted"); }
			}
			public static string UpdatePending
			{
				get { return Res.GetString("9dc11510-c07a-47bf-a7fb-3e4da1251ffc", "Update Pending"); }
			}
		}

		public CustomsPostedStatusList() : base()
		{
			AddPair(Codes.Accepted, Descriptions.Accepted);
			AddPair(Codes.UpdatePending, Descriptions.UpdatePending);
		}
	}
}
