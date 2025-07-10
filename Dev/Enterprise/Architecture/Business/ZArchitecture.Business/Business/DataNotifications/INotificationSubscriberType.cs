using CargoWise.ComponentModel;

namespace Enterprise.ZArchitecture
{
	[WTG.StaticAnalysis.Annotation.Immutable]
	public interface INotificationSubscriberType : INotificationType
	{
		string Name { get; }
		string Message { get; }
		string GetDisplayMessage(string additionalInfo);
	}
}
