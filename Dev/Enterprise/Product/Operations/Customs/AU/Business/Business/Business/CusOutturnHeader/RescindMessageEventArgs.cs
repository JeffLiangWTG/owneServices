using System;

namespace Enterprise.Customs.AU.Declaration.Business;

public class RescindMessageEventArgs : EventArgs
{
	readonly string messageText;

	public RescindMessageEventArgs(string messageText)
	{
		this.messageText = messageText;
	}

	public string MessageText
	{
		get { return messageText; }
	}
}
