using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class RollCollection : DependentCusAddInfoCollection<Roll, CLREGInfoProvider>
	{
		public RollCollection(CLREGInfoProvider master)
			: base(master, CusAddInfoTypeAttribute.Codes.AUROLL)
		{
		}

		public bool HasExporterRoll
		{
			get
			{
				foreach (Roll roll in this)
				{
					if (roll.ZA_Roll == CMRClientRolls.Codes.Exporter)
					{
						return true;
					}
				}
				return false;
			}
		}
	}
}
