namespace Enterprise.Customs.AE.Business;

public interface IMessageHeaderProvider
{
	string ReferenceNumber { get; }

	string MessageType { get; }

	string MessageVersion { get; }

	string MessageReleaseNumber { get; }

	string ControllingAgency { get; }
}
