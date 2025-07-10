using CargoWise.Types;
using Enterprise.MasterFiles.Business.Customs;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class CusExitHeaderProcessTaskCollection : ProcessTaskCollection<CusExitHeaderProcessTask, CusExitHeader>
	{
		public CusExitHeaderProcessTaskCollection(CusExitHeader parent) : base(parent)
		{
		}

		public override ZString OriginCountry => Parent.CountryCode;
	}
}
