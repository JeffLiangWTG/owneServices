using System.Data;

using CargoWise.EntityFramework;
using Enterprise.Client.JAS.Business.Cognos;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.JAS.Business
{
	public class JASOrgCreditorGroup : OrgCreditorGroup
	{
		public JASOrgCreditorGroup(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override void Delete()
		{
			base.Delete();
			CognosAccountExtraInfoCollection.RemoveAll();
		}

		ManyToManyCreditorExtraInfoCollection CognosAccountExtraInfoCollection
		{
			get
			{
				if (fCognosAccountExtraInfoCollection == null)
				{
					fCognosAccountExtraInfoCollection = new ManyToManyCreditorExtraInfoCollection(this);
					fCognosAccountExtraInfoCollection.Load();
				}
				return fCognosAccountExtraInfoCollection;
			}
		}

		ManyToManyCreditorExtraInfoCollection fCognosAccountExtraInfoCollection;
	}
}
