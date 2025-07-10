using CargoWiseOne.ResourceStrings;

class CW1174
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1178:Do Not Invoke Old Res.GetString Methods", Justification = "Used for Analyzer test")]
	public void BadCode()
	{
		// CW1174 Res.GetString invocations must be located within a namespace declaration
		_ = Res.GetString("abcd-1234-efgh-5678", "Customs Main Page");
	}
}
