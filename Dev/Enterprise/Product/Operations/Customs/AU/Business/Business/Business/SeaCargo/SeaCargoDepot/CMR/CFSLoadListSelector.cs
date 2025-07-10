using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.CFS.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CFSLoadListSelector : NonPersistentBusinessObject, IObsoleteValidation
	{
		public CFSLoadListSelector(ZString vesselName, ZString voyage)
		{
			fVesselName = vesselName;
			fVoyage = voyage;
		}

		public ZString VesselName
		{
			get { return fVesselName; }
		}
		readonly ZString fVesselName;

		public ZString Voyage
		{
			get { return fVoyage; }
		}
		readonly ZString fVoyage;

		public ZGuid ConsolPK
		{
			get { return fConsolPK; }
			set
			{
				SetNonPersistentPropertyValue(ConsolPKInfo, ref fConsolPK, value);
				if (!IsValidationSuspended)
				{
					ValidateConsolPK();
				}
			}
		}
		ZGuid fConsolPK;

		public ZPropertyInfo ConsolPKInfo
		{
			get { return GetZPropertyInfo(nameof(ConsolPK)); }
		}

		protected void ValidateConsolPK()
		{
			ConsolPKInfo.ClearAllNotifications();
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var consol = factory.Load<CFSLoadListConsol>(ConsolPK);
			if (consol == null)
			{
				ConsolPKInfo.AddError("Please Select a consol");
			}
			else
			{
				if (consol.JK_JX_JV_VoyageFlight.ToUpper() != Voyage.ToUpper())
				{
					ConsolPKInfo.AddError("Consol is on the incorrect Voyage.  Please select a consol on Voyage: " + Voyage);
				}
				if (consol.JK_JX_JV_NKVessel.ToUpper() != VesselName.ToUpper())
				{
					ConsolPKInfo.AddError("Consol is on the incorrect Vessel.  Please select a consol on Vessel: " + VesselName);
				}
			}
		}

		CFSLoadListConsolCollection fLoadListConsol_List;
		public CFSLoadListConsolCollection LoadListConsol_List
		{
			get
			{
				if (fLoadListConsol_List == null)
				{
					BusinessObjectFactory factory = new BusinessObjectFactory();
					fLoadListConsol_List = new CFSLoadListConsolCollection(factory);
				}

				#region Set Default Filters

				CFSConsolDefaultFilterProvider provider = new CFSConsolDefaultFilterProvider();

				provider.Voyage = Voyage;
				provider.Vessel = VesselName;

				provider.SetDefaultFilters(fLoadListConsol_List);

				#endregion

				return fLoadListConsol_List;
			}
		}
	}
}
