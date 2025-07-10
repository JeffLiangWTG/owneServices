using System.Collections;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class CMRDepotUnderbondSynchroniser : BusinessObjectSynchroniser
	{
		public CMRDepotUnderbondSynchroniser(CusUnderbond destination, BusinessObject source)
			: base(destination, source)
		{
		}

		protected ArrayList OutturnSynchronisers
		{
			get
			{
				if (fOutturnSynchronisers == null)
				{
					fOutturnSynchronisers = new ArrayList();
				}
				return fOutturnSynchronisers;
			}
		}
		ArrayList fOutturnSynchronisers;

		protected override void OnSynchronise(SynchroniseEventArgs e)
		{
			base.OnSynchronise(e);
			foreach (CMRDepotOutturnSynchroniser synchroniser in OutturnSynchronisers)
			{
				synchroniser.Synchronise(e);
			}
		}

		protected CMRDepotOutturnSynchroniser GetOutturnSynchroniser(BusinessObject source, CusOutturn destination)
		{
			CMRDepotOutturnSynchroniser result = null;
			foreach (CMRDepotOutturnSynchroniser synchroniser in OutturnSynchronisers)
			{
				if (synchroniser.CFSSource == source && synchroniser.Outturn == destination)
				{
					result = synchroniser;
					break;
				}
			}
			return result;
		}
	}
}
