using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AQISControl : BaseAddInfoControl
	{
		public IAQIS AQIS
		{
			get
			{
				IAQIS result = null;

				if (CurrentAddInfo != null)
				{
					result = CurrentAddInfo.ParentAQISInfo;
				}

				return result;
			}
		}
	}
}
