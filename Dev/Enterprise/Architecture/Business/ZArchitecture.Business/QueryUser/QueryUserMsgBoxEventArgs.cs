using Res = Enterprise.ZArchitecture.Business.Res;

namespace Enterprise.ZArchitecture
{
	public class QueryUserMsgBoxEventArgs : QueryUserEventArgs
	{
		public QueryUserMsgBoxEventArgs(string caption, string message, bool defaultResponse)
		{
			this.Caption = caption;
			this.Message = message;
			this.Response = defaultResponse;
		}

		public QueryUserMsgBoxEventArgs(string message, bool defaultResponse) : this("", message, defaultResponse)
		{
		}

		public string Message;
		public string Caption;
		public bool Response;
	}

	public class QueryUserYesNoEventArgs : QueryUserMsgBoxEventArgs
	{
		public QueryUserYesNoEventArgs(string message, bool defaultResponse) : base(Res.GetString("199a56bc-01f6-4d67-b1e4-6b55d770c9d5", "Question"), message, defaultResponse)
		{
		}

		public QueryUserYesNoEventArgs(string caption, string message, bool defaultResponse) : base(caption, message, defaultResponse)
		{
		}
	}

	public class QueryUserOkCancelEventArgs : QueryUserMsgBoxEventArgs
	{
		public QueryUserOkCancelEventArgs(string message, bool defaultResponse) : base(Res.GetString("9ed3ba9c-b1b0-473d-a2eb-f4bf3ddef164", "Message"), message, defaultResponse)
		{
		}

		public QueryUserOkCancelEventArgs(string caption, string message, bool defaultResponse) : base(caption, message, defaultResponse)
		{
		}
	}

	public class QueryUserRetryCancelEventArgs : QueryUserMsgBoxEventArgs
	{
		public QueryUserRetryCancelEventArgs(string message, bool defaultResponse) : base(Res.GetString("9ed3ba9c-b1b0-473d-a2eb-f4bf3ddef164", "Message"), message, defaultResponse)
		{
		}

		public QueryUserRetryCancelEventArgs(string caption, string message, bool defaultResponse) : base(caption, message, defaultResponse)
		{
		}
	}

	public class QueryUserYesNoCancelEventArgs : QueryUserMsgBoxEventArgs
	{
		public QueryUserYesNoCancelEventArgs(string message, bool defaultResponse) : base(Res.GetString("9ed3ba9c-b1b0-473d-a2eb-f4bf3ddef164", "Message"), message, defaultResponse)
		{
		}

		public QueryUserYesNoCancelEventArgs(string caption, string message, bool defaultResponse) : base(caption, message, defaultResponse)
		{
		}

		public bool Cancel;
	}

	public class QueryUserYesNoYesAllNoAllEventArgs : QueryUserMsgBoxEventArgs
	{
		public QueryUserYesNoYesAllNoAllEventArgs() : base(Res.GetString("ad25e164-e9b1-4547-8580-4c0671ca8ab7", "Please decide on the following:"), Res.GetString("7dd2a48b-2cc7-4ac9-9fe9-d0e05ad50a85", "No Default Message"), true)
		{
		}
	}
}
