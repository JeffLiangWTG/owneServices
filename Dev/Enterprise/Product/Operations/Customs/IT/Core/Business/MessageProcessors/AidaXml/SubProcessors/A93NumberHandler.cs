using System;
using CargoWise.Common;
using IXmlResponseMessage = CargoWise.Customs.IT.MessageDefinitions.IResponseMessage;

namespace Enterprise.Customs.IT.Business;

sealed class A93NumberHandler : IResponseMessageHandler
{
	public A93NumberHandler(IXmlCustomsLinkedObjectAdapter customsLinkedObjectAdapter, A93NumberHandleMode numberHandleMode)
	{
		this.customsLinkedObjectAdapter = Argument.NotNull(customsLinkedObjectAdapter, nameof(customsLinkedObjectAdapter));
		this.numberHandleMode = numberHandleMode;
	}

	public void Handle(IXmlResponseMessage responseMessage)
	{
		Argument.NotNull(responseMessage, nameof(responseMessage));

		var a93NumberManager = new A93NumbersManager(customsLinkedObjectAdapter);

		if (numberHandleMode.HasFlag(A93NumberHandleMode.Invalidate))
		{
			a93NumberManager.InvalidateNumbers();
		}

		var a93Numbers = responseMessage.A93Numbers;
		if (numberHandleMode.HasFlag(A93NumberHandleMode.Add) && a93Numbers.Count > 0)
		{
			a93NumberManager.AddNumbers(a93Numbers);
		}
	}

	readonly IXmlCustomsLinkedObjectAdapter customsLinkedObjectAdapter;
	readonly A93NumberHandleMode numberHandleMode;
}

[Flags]
enum A93NumberHandleMode
{
	Invalidate = 1,
	Add = 2,
	InvalidateAndAdd = Invalidate | Add
}
