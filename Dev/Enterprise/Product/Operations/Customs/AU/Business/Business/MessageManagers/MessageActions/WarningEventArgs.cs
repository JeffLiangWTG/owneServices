using System;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business;

public class WarningEventArgs : EventArgs
{
	public WarningEventArgs(ZString caption, ZString message)
	{
		this.Caption = caption;
		this.Message = message;
	}

	public readonly ZString Caption;
	public readonly ZString Message;
}
