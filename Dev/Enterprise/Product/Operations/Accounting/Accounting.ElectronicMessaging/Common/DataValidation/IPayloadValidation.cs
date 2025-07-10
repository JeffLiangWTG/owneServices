using System.IO;
using CargoWise.ComponentModel;

namespace Enterprise.Accounting.ElectronicMessaging.Common.DataValidation
{
	public interface IPayloadValidation
	{
		void Validate(Stream stream, INotifications errorNotifications, INotifications warningNotifications);
	}
}
