using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business;

public class ContinueEventArgs : System.ComponentModel.CancelEventArgs
{
	public ContinueEventArgs(ZString message, ZString caption)
	{
		this.Caption = caption;
		this.Message = message;
	}
	public readonly ZString Caption;
	public readonly ZString Message;
}
