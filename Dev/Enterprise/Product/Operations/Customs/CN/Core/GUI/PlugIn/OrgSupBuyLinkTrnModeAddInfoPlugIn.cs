using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CN.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.CN.GUI
{
	public class OrgSupBuyLinkTrnModeAddInfoPlugIn : ZPlugIn
	{
		public OrgSupBuyLinkTrnModeAddInfoPlugIn(IBusiness hostBusinessEntity) : base(hostBusinessEntity)
		{
		}

		OrgSupBuyLinkTrnModeAddInfoBizObjCollection fAddInfoCollection;
		OrgSupBuyLinkTrnModeAddInfoBizObjCollection AddInfoCollection
		{
			get
			{
				if (fAddInfoCollection == null)
				{
					fAddInfoCollection = new OrgSupBuyLinkTrnModeAddInfoBizObjCollection(Factory);
					var newBizObj = CreateNewBizObjAndAddToDect();
					if (newBizObj != null)
					{
						fAddInfoCollection.Add(newBizObj);
					}
				}
				return fAddInfoCollection;
			}
		}

		Dictionary<ZGuid, OrgSupBuyLinkTrnModeAddInfoBizObj> fAddInfoDictionary;
		Dictionary<ZGuid, OrgSupBuyLinkTrnModeAddInfoBizObj> AddInfDictionary => fAddInfoDictionary ?? (fAddInfoDictionary = new Dictionary<ZGuid, OrgSupBuyLinkTrnModeAddInfoBizObj>());

		OrgSupBuyLinkTrnMode CurrentMode => Current as OrgSupBuyLinkTrnMode;

		public override string Name => Res.GetString("E3C5D476-D426-455B-88E4-0015C0056A99", "China Customs Defaults");

		protected override ZBool HasUserControl => true;

		protected override LicenceCheckpoint LicenceCheckPoint => Env.Licence.Broker;

		protected override Control GetNewUserControl()
		{
			return new OrgSupBuyLinkTrnModeControl();
		}

		protected override void OnCurrentChanged()
		{
			base.OnCurrentChanged();
			var addInfoCollection = AddInfoCollection;
			addInfoCollection.RemoveAll();

			var currentModePK = CurrentMode?.PK ?? ZGuid.Empty;
			if (!currentModePK.IsEmpty)
			{
				OrgSupBuyLinkTrnModeAddInfoBizObj bizObjToAdd;

				if (AddInfDictionary.ContainsKey(currentModePK))
				{
					bizObjToAdd = AddInfDictionary[currentModePK];
				}
				else
				{
					bizObjToAdd = CreateNewBizObjAndAddToDect();
				}

				addInfoCollection.Add(bizObjToAdd);
			}
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return AddInfoCollection;
		}

		OrgSupBuyLinkTrnModeAddInfoBizObj CreateNewBizObjAndAddToDect()
		{
			OrgSupBuyLinkTrnModeAddInfoBizObj result = null;

			var currentMode = CurrentMode;
			if (currentMode != null)
			{
				result = new OrgSupBuyLinkTrnModeAddInfoBizObj((OrgSupBuyLinkTrnModeAddInfo)currentMode.AddInfo);
				AddInfDictionary.Add(currentMode.PK, result);
			}

			return result;
		}
	}
}
