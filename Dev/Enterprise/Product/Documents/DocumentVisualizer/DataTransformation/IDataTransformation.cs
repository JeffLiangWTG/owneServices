using System.Xml.Linq;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.DataTransformation
{
	interface IDataTransformation
	{
		string Description { get; }
		string DataStoreName { get; }

		XDocument Run(XDocument xml, INotificationsHandler notificationsHandler);
	}
}