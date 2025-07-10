namespace Enterprise.Customs.EU.Business.Declaration
{
	public static class DeclarationValidationConstants
	{
		public static string IncotermPlaceCodeMismatch => Res.GetString("9E84DBB5-5BBA-4FCF-9A20-B0486F954DF9", "Incoterm place code values do not match between Declaration and Invoice Header.");

		public static string RequiredAgreedPlaceCode => Res.GetString("3B029213-A968-44B5-894C-523B9DBA1C15", "Incoterm Place Code or Country Code is required");
	}
}
