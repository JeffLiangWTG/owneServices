using CargoWise.Common;
using Enterprise.ZArchitecture.Environment.DialogDefault;

namespace Enterprise.ZArchitecture
{
	public class DefaultableQueryUserEventArgs : QueryUserMsgBoxEventArgs
	{
		public DialogDefaultContext Context { get; }

		public DefaultableQueryUserEventArgs(DialogDefaultContext dialogDefaultContext, string message, bool defaultResponse)
			: base(Argument.NotNull(dialogDefaultContext, nameof(dialogDefaultContext)).Caption, message, defaultResponse)
		{
			Context = dialogDefaultContext;
		}
	}
}
