
using CargoWise.Types;

namespace Enterprise.Client.OSP.Data_Import
{
	public class IFTMIN5MHeadingMessageDataRow : IFTMIN5MBaseDataRow
	{
		public IFTMIN5MHeadingMessageDataRow(ZString dataRow)
			: base(dataRow)
		{
		}

		public IFTMIN5MHeadingMessageDataRow(IFTMIN5MBaseDataRow dataRow)
			: base(dataRow)
		{
		}

		static class Constants
		{
			public static class MessageID
			{
				public const int Length = 7;
				public const int Position = 3;
			}

			public static class SenderID
			{
				public const int Length = 16;
				public const int Position = 10;
			}

			public static class SecondarySenderID
			{
				public const int Length = 10;
				public const int Position = 26;
			}
		}

		public ZString MessageID
		{
			get { return DataRow.SubstringSafe(Constants.MessageID.Position, Constants.MessageID.Length).Trim(); }
		}

		public ZString SenderID
		{
			get { return DataRow.SubstringSafe(Constants.SenderID.Position, Constants.SenderID.Length).Trim(); }
		}

		public ZString SecondarySenderID
		{
			get { return DataRow.SubstringSafe(Constants.SecondarySenderID.Position, Constants.SecondarySenderID.Length).Trim(); }
		}
	}
}
