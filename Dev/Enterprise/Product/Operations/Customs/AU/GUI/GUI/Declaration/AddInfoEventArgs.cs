using System;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Customs.AU.Declaration.GUI;

public class AddInfoEventArgs : EventArgs
{
	public readonly AUAddInfo AddInfo;
	public AddInfoEventArgs(AUAddInfo addInfoToSave)
	{
		this.AddInfo = addInfoToSave;
	}
}
