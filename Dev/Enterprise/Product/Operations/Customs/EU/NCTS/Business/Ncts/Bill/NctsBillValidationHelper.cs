namespace Enterprise.Customs.EU.NCTS.Business
{
	public static class NctsBillValidationHelper
	{
		public static string GetMessageErrorForRuleE1301(string attributeDescriptor)
		{
			return Res.GetString("DB328DE3-9146-4883-AA3E-C5A964D07BB1", "[E1301] In transition period, which is now, {0} must be empty", attributeDescriptor);
		}

		public static string GetMessageErrorR0506MustBeDifferent(string attributeDescriptor)
		{
			return Res.GetString("D9853879-5A36-4447-AD49-ED10DD70D2F9", "[R0506] {0} must be different for at least one of the house consignment.", attributeDescriptor);
		}
	}
}
