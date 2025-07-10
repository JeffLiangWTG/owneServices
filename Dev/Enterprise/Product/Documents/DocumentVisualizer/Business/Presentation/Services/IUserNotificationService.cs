using System.Collections.Generic;
using CargoWise.Integration;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public interface IUserNotificationService : IUserNotifications
	{
		void ShowNotifications(IEnumerable<INotification> notifications);
		string QueryUserResponse(string message, string caption, int minimumResponseLength, int maximumResponseLength);
		ICodeDescription QueryUserResponse(string message, string caption, ICodeDescriptionPairList optionsList);
	}
}