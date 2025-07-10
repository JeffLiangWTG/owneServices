using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CN.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.CN.GUI
{
	public class CNOrgAdditionalCustomsDefaultsPlugIn : ZPlugIn
	{
		public CNOrgAdditionalCustomsDefaultsPlugIn(IBusiness hostBusinessEntity) : base(hostBusinessEntity)
		{
			Enabled = Env.CurrentCompany.Country.Code == Core.Constants.CountryCodes.China;
		}

		public override string Name => (NoResString)"China Customs Defaults";

		protected override ZBool HasUserControl => true;

		protected override LicenceCheckpoint LicenceCheckPoint => Env.Licence.Broker;

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return LinkCollection;
		}

		protected CNOrgSupplierBuyerLinkAddInfoCollection LinkCollection
		{
			get
			{
				if (linkCollection == null)
				{
					linkCollection = new CNOrgSupplierBuyerLinkAddInfoCollection(Factory);
					UpdateCollectionWithCurrent();
				}
				return linkCollection;
			}
		}
		CNOrgSupplierBuyerLinkAddInfoCollection linkCollection;

		void UpdateCollectionWithCurrent()
		{
			var addInfo = FindCurrentAddInfoAndUpdateCollection();
			if (addInfo != null)
			{
				if (LinkCollection.Count > 0)
				{
					LinkCollection.RemoveAll();
				}
				LinkCollection.Add(addInfo);
			}
		}

		CNOrgSupplierBuyerLinkAddInfo FindCurrentAddInfoAndUpdateCollection()
		{
			CNOrgSupplierBuyerLinkAddInfo addInfo = null;
			if (CurrentLink != null)
			{
				addInfo = (from CNOrgSupplierBuyerLinkAddInfo existingAddInfo in ExistinAddInfoDataLinkCollection
						   where existingAddInfo.ParentPK == CurrentLink.PK
						   select existingAddInfo).FirstOrDefault();
				if (addInfo == null)
				{
					addInfo = CurrentLink.GetAddInfo() is OrgSupplierBuyerLinkAddInfo addInfoBO ? new CNOrgSupplierBuyerLinkAddInfo(addInfoBO) : null;
					if (addInfo != null)
					{
						ExistinAddInfoDataLinkCollection.Add(addInfo);
					}
				}
			}
			return addInfo;
		}

		protected OrgSupplierBuyerLink CurrentLink
		{
			get { return (OrgSupplierBuyerLink)Current; }
		}

		protected CNOrgSupplierBuyerLinkAddInfoCollection ExistinAddInfoDataLinkCollection
		{
			get
			{
				if (existinAddInfoDataLinkCollection == null)
				{
					existinAddInfoDataLinkCollection = new CNOrgSupplierBuyerLinkAddInfoCollection(Factory);
					FindCurrentAddInfoAndUpdateCollection();
				}
				return existinAddInfoDataLinkCollection;
			}
		}
		CNOrgSupplierBuyerLinkAddInfoCollection existinAddInfoDataLinkCollection;

		protected override Control GetNewUserControl()
		{
			return new CNOrgAdditionalCustomsDefaultsControl();
		}

		protected override void OnCurrentChanged()
		{
			if (previousLink != null)
			{
				previousLink.OL_RN_NKImporterCountryInfo.ValueChanged -= new System.EventHandler(OL_RN_NKImporterCountryInfo_ValueChanged);
			}

			base.OnCurrentChanged();

			previousLink = CurrentLink;

			UpdateCollectionWithCurrent();

			if (CurrentLink != null)
			{
				CurrentLink.OL_RN_NKImporterCountryInfo.ValueChanged += new System.EventHandler(OL_RN_NKImporterCountryInfo_ValueChanged);
			}
		}

		OrgSupplierBuyerLink previousLink;

		void OL_RN_NKImporterCountryInfo_ValueChanged(object sender, System.EventArgs e)
		{
			UpdateCollectionWithCurrent();
		}
	}
}
