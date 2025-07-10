using System;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.AU.Declaration.GUI;

public class AUCClassColumnStyleInfo : ZBaseFindBoxColumnStyleInfo
{
	public override Type ColumnStyleType
	{
		get { return typeof(AUCClassColumnStyle); }
	}
}
