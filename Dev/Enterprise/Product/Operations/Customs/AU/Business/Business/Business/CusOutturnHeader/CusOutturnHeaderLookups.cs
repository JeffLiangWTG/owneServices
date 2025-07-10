using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusOutturnHeaderLookups : Customs.Business.CusOutturnHeaderLookups
	{
		public CusOutturnHeaderLookups(CusOutturnHeader parent)
			: base(parent)
		{
		}

		protected new CusOutturnHeader Parent => (CusOutturnHeader)base.Parent;

		#region CTOAddressOrgs

		public SeaCTOAndDepotCollection CTOAddressOrgs
		{
			get
			{
				if (fCTOAddressOrgs == null)
				{
					fCTOAddressOrgs = new SeaCTOAndDepotCollection(Factory);
				}
				return fCTOAddressOrgs;
			}
		}
		SeaCTOAndDepotCollection fCTOAddressOrgs;

		#endregion

		#region OutturnStatusList

		public override CodeDescriptionPairList OutturnStatusList
		{
			get
			{
				if (fOutturnStatusList == null)
				{
					fOutturnStatusList = GetNewOutturnStatusList();
				}

				return fOutturnStatusList;
			}
		}
		CodeDescriptionPairList fOutturnStatusList;

		protected CodeDescriptionPairList GetNewOutturnStatusList()
		{
			return new CMRBaseStatuses();
		}

		#endregion

		#region LloydsIMOList
		public CodeDescriptionPairList LloydsIMOList
		{
			get
			{
				return Factory.GetCachedValue("CusOutturnHeaderLookups.LloydsIMOList" + Parent.C6_VesselName, delegate
				{
					var result = new CodeDescriptionPairList();

					var vessels = Factory.Load<RefVessel>(new ZQuery(RefVesselSchema.RV_Code, Parent.C6_VesselName));
					if (vessels != null)
					{
						var lloydsNumberList = vessels.Select(x => x.RV_LloydsNumber).ToList();
						lloydsNumberList.ForEach(x => result.AddPair(x, ""));
					}
					return result;
				});
			}
		}
		#endregion

		#region VesselNames
		public override RefVesselCollection VesselNames
		{
			get
			{
				return Factory.GetCachedValue("AUCusOutturnHeaderLookups.Vessels_" + Parent.C6_VesselName + "_" + Parent.C6_LloydsIMO, () =>
				{
					var result = base.VesselNames;
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(RefVesselCollection.FilterConstants.VesselName, "Property", Parent.C6_VesselName));
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(RefVesselCollection.FilterConstants.LloydsNumber, "Property", Parent.C6_LloydsIMO));
					return result;
				});
			}
		}
		#endregion
	}
}
