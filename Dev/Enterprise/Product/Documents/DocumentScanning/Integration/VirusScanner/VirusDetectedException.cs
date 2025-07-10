using System;

namespace Enterprise.DocumentScanning.Integration
{
	[Serializable]
	public class VirusDetectedException : Exception
	{
		public static string VirusDetectedExceptionMessage => Res.GetString("1AAFD4F8-93A1-4683-A5B1-9ADE096CA53E", "The file has been detected with virus and therefore cannot be saved or opened.");
		public virtual string VirusDetectedFriendlyMessage => Res.GetString("8F2221F6-EF73-413C-B277-3695F10E3939", "The file \"{0}\" has been detected with virus and therefore cannot be saved or opened.", Filename);

		public VirusDetectedException(string filename) : base(VirusDetectedExceptionMessage)
		{
			this.Filename = filename ?? "";
		}

#if NETFRAMEWORK
		protected VirusDetectedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public readonly string Filename;
	}
}
