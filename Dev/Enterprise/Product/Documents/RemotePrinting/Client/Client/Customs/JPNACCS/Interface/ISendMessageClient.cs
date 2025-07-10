using System;
using MimeKit;

namespace Enterprise.RemotePrinting.Client;

public interface ISendMessageClient : IDisposable
{
	bool IsConnected { get; }

	double Interval { get; }

	int ConnectMaxRetries { get; }
	int CommandMaxRetries { get; }

	void Connect(string host);
	void Send(MimeMessage message);
	void Disconnect(bool quit);
}
