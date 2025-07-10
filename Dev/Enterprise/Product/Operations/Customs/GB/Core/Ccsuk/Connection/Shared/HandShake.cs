namespace Enterprise.Customs.GB.Ccsuk.Connection
{
	public abstract class HandShake : Body
	{
		public static string HandShakeIdentifier = "***Hand Shake***";

		public abstract string Host { get; }

		public abstract string MessageType { get; }
	}
}
