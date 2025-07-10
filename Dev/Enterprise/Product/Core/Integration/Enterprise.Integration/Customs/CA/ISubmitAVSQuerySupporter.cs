using CargoWise.EntityFramework;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class CA
		{
			public interface ISubmitAVSQuerySupporter
			{
				bool SupportSubmitAVSQuery { get; }
				string NotSupportSubmitAVSQueryReason { get; }
				IProcessor CreateSubmitAVSQueryProcessor();
			}
		}
	}
}