using System;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.DataTransfer.Native.WebServiceDelivery.LocalService;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DataTransfer.Native.WebServiceDelivery
{
	/// <summary>
	/// Convert IEDICommunicationMode to Request Message
	/// </summary>
	public class EDICommunicationsModeConverter : IEDICommunicationsModeConverter
	{
		public IRequestMessage Convert(IEDICommunicationsMode mode)
		{
			var updateRequest = new RequestMessageData();
			updateRequest.MessageID = Guid.NewGuid().ToString();
			updateRequest.RecipientID = mode.EK_Destination;
			updateRequest.RecipientType = "ENTNDS";
			//Might be change due to security type for Web Service
			updateRequest.SenderUsername = mode.EK_LoginName;
			updateRequest.SenderID = GenerateSenderID();

			return updateRequest;
		}

		public string GenerateSenderID()
		{
			return Env.CurrentCompany.GetLicenceCode();
		}
	}

	public interface IEDICommunicationsModeConverter
	{
		IRequestMessage Convert(IEDICommunicationsMode mode);
	}
}
