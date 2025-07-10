using System.Data;

using CargoWise.EntityFramework;
using Enterprise.Client.JAS.Business.Cognos;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.JAS.Business
{
	public class JASOrgDebtorGroup : OrgDebtorGroup
	{
		public JASOrgDebtorGroup(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override void Delete()
		{
			base.Delete();
			CognosAccountExtraInfoCollection.RemoveAll();
		}

		ManyToManyDebtorExtraInfoCollection CognosAccountExtraInfoCollection
		{
			get
			{
				if (fCognosAccountExtraInfoCollection == null)
				{
					fCognosAccountExtraInfoCollection = new ManyToManyDebtorExtraInfoCollection(this);
					fCognosAccountExtraInfoCollection.Load();
				}
				return fCognosAccountExtraInfoCollection;
			}
		}

		ManyToManyDebtorExtraInfoCollection fCognosAccountExtraInfoCollection;
	}
}
