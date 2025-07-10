using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public class AUCClassColumnStyle : ZBaseFindBoxColumnStyle
	{
		public AUCClassColumnStyle(AUCClassColumnStyleInfo info)
			: base(() => new AUCClassGridFindBox(), info)
		{
		}
	}
}
