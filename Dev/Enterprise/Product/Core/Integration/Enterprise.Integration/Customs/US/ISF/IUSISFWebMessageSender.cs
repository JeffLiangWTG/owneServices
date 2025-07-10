using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class US
		{
			public static partial class ISF
			{
				public interface IUSISFWebMessageSender
				{
					string SendUpsertMessage(ZGuid headerPK);

					string SendDeleteMessage(ZGuid headerPK);
				}
			}
		}
	}
}
