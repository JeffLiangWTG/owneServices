namespace Enterprise.ProductRegistration.Common
{
	public static class Constants
	{
		// This should never happen because we will only send CW Next upgrade packages to customers whose databases are set for CW Next.
		// This is a defence-in-depth measure in case the above assumption is wrong.
		// E.g. if a customer gets their hands on a CW Next .edp and manually copies it to their CW1 server.
		// Because it should never happen, the error message is deliberately vague about how to solve it.
		public const string CargoWiseNextWrongProductUserErrorMessage = "This database is not set up for CargoWise Next. Please contact WiseTech Global.";
	}
}
