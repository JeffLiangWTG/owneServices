using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAOceanBillLookups : Customs.Business.CusSCAOceanBillLookups
	{
		public CusSCAOceanBillLookups(Customs.Business.BaseCusSCAOceanBill parent)
			: base(parent)
		{
		}

		protected new CusSCAOceanBill Parent => (CusSCAOceanBill)base.Parent;

		OrgHeaderCollection shippingLines;
		public override OrgHeaderCollection ShippingLines
		{
			get
			{
				if (shippingLines == null)
				{
					shippingLines = new ShippingProviderCollection(Factory);
				}
				return shippingLines;
			}
		}

		public CodeDescriptionPairList ApplicationCodeList
		{
			get
			{
				return Factory.GetCachedValue("AUSCAOceanLookups_ApplicationCodeList", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(AUCusApplicationCodeList.Codes.ForceCMRMessages, AUCusApplicationCodeList.Descriptions.ForceCMRMessages);
					return result;
				});
			}
		}

		public CodeDescriptionPairList CustomsShipmentStatusList
		{
			get
			{
				return Factory.GetCachedValue("CMR Extended CustomsShipmentStatusList", () => CMRConsolidatedCargoStatuses.SeaFilterStatuses);
			}
		}

		public CodeDescriptionPairList CustomsMessageStatusList
		{
			get
			{
				return Factory.GetCachedValue<CMRBaseStatuses>();
			}
		}

		#region LloydsIMOList
		public CodeDescriptionPairList LloydsIMOList
		{
			get
			{
				return Factory.GetCachedValue("CusSCAOceanBillLookups.LloydsIMOList" + Parent.CB_VesselName, delegate
				{
					var result = new CodeDescriptionPairList();

					var vessels = Factory.Load<RefVessel>(new ZQuery(RefVesselSchema.RV_Code, Parent.CB_VesselName));
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

		public override RefVesselCollection VesselNames
		{
			get
			{
				return Factory.GetCachedValue($"AUCusSCAOceanBillLookups.VesselNames_" + Parent.CB_VesselName + "_" + Parent.CB_LloydsIMO, () =>
				{
					var vesselNames = base.VesselNames;
					vesselNames.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(RefVesselCollection.FilterConstants.VesselName, "Property", Parent.CB_VesselName));
					vesselNames.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(RefVesselCollection.FilterConstants.LloydsNumber, "Property", Parent.CB_LloydsIMO));
					return vesselNames;
				});
			}
		}
	}
}
