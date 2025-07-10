using System;
using MimeKit;

namespace Enterprise.RemotePrinting.Client;

public interface IReceiveMessageClient : IDisposable
{
	bool IsConnected { get; }
	int Count { get; }

	double Interval { get; }

	int ConnectMaxRetries { get; }
	int AuthenticateMaxRetries { get; }
	int CommandMaxRetries { get; }

	void Connect(string host);
	bool IsAuthenticated { get; }
	void Authenticate(string userName, string password);
	MimeMessage GetMessage(int index);
	void DeleteMessage(int index);
	void Disconnect(bool quit);
}
