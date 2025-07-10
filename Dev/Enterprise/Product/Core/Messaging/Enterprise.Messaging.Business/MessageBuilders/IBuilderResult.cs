using CargoWise.EntityFramework;

namespace Enterprise.Messaging.Business.MessageBuilders
{
	public interface IBuilderResult
	{
		EDIMessage Message { get; }
		string[] Errors { get; }
		void AfterFullSuccess();
		AfterFullSuccessDelegate AfterFullSuccessDelegate { get; }
		BusinessObject Owner { get; }
	}

	public delegate void AfterFullSuccessDelegate(IBuilderResult builderResult);
}
