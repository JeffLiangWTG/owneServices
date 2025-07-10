using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.MasterFiles;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.GVMS
{
	public class AsycudaManifestHeaderLookups : ASYCUDA.Business.AsycudaManifestHeaderLookups
	{
		public AsycudaManifestHeaderLookups(ASYCUDA.Business.AsycudaManifestHeader parent) : base(parent)
		{
		}
		public new AsycudaManifestHeader Parent => (AsycudaManifestHeader)base.Parent;

		public override CodeDescriptionPairList Natures => Factory.GetCachedValue<GVMSManifestNature>();

		public CodeDescriptionPairList GVMSRoutesList => Universal.RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GbGVMSRoutes, ZDateTime.Now);

		protected override ICollection GetCustomsLoadingPortListCore()
		{
			switch (Parent.AMA_Nature)
			{
				case GVMSManifestNature.Codes.Import:
					return Factory.GetCachedValue<RefUNLOCOCollection>("GVMS-Ports-GVMS", () => new UKUnlocoGVMSLocations(Factory));
				case GVMSManifestNature.Codes.Export:
					return Factory.GetCachedValue<RefUNLOCOCollection>("GVMS-Ports-UK", () => new UKUnlocoUKLocations(Factory));
				case GVMSManifestNature.Codes.GBtoNI:
					return Factory.GetCachedValue<RefUNLOCOCollection>("GVMS-Ports-GB", () => new UKUnlocoGBLocations(Factory));
				case GVMSManifestNature.Codes.NItoGB:
					return Factory.GetCachedValue<RefUNLOCOCollection>("GVMS-Ports-NI", () => new UKUnlocoNILocations(Factory));
				default:
					return base.GetLoadingPortListCore();
			}
		}

		protected override RefUNLOCOCollection GetLoadingPortListCore()
		{
			return (RefUNLOCOCollection)GetCustomsLoadingPortListCore();
		}

		protected override ICollection GetCustomsDischargePortListCore()
		{
			switch (Parent.AMA_Nature)
			{
				case GVMSManifestNature.Codes.Import:
					return Factory.GetCachedValue<RefUNLOCOCollection>("GVMS-Ports-UK", () => new UKUnlocoUKLocations(Factory));
				case GVMSManifestNature.Codes.Export:
					return Factory.GetCachedValue<RefUNLOCOCollection>("GVMS-Ports-GVMS", () => new UKUnlocoGVMSLocations(Factory));
				case GVMSManifestNature.Codes.GBtoNI:
					return Factory.GetCachedValue<RefUNLOCOCollection>("GVMS-Ports-NI", () => new UKUnlocoNILocations(Factory));
				case GVMSManifestNature.Codes.NItoGB:
					return Factory.GetCachedValue<RefUNLOCOCollection>("GVMS-Ports-GB", () => new UKUnlocoGBLocations(Factory));
				default:
					return base.GetDischargePortListCore();
			}
		}

		protected override RefUNLOCOCollection GetDischargePortListCore()
		{
			return (RefUNLOCOCollection)GetCustomsDischargePortListCore();
		}

		public CodeDescriptionPairList CarrierCodeList
		{
			get
			{
				return Factory.GetCachedValue($"GVMS_CarrierCodes_{Parent.AMA_TransportMode}", () =>
				{
					var collection = new ZZRefCarrierCombinedCollection(Factory, Core.Constants.CountryCodes.UnitedKingdom);

					var list = new CodeDescriptionPairList();
					foreach (var carrier in collection)
					{
						list.AddPair(carrier.ZZ4_Code, carrier.ZZ4_Description);
					}

					return list;
				});
			}
		}

		public CodeDescriptionPairList EmptyVehicleList => Factory.GetCachedValue<GVMSEmptyVehicle>();

		public CodeDescriptionPairList ProfileList
		{
			get
			{
				var branch = Parent.Branch;
				var pwdList = new GlbExternalPasswordCollection_GB(branch.Company);
				string eori = branch.OrgProxy.GetEuIdentificationNumber();

				var list = new CodeDescriptionPairList();
				pwdList.Load();
				foreach (GlbExternalPassword_GB pwd in pwdList)
				{
					if (pwd.Status == PasswordStatusList.Codes.Valid && pwd.EORI == eori)
					{
						list.AddPair(pwd.Badge);
					}
				}
				return list;
			}
		}

		public CodeDescriptionPairList HaulierTypeList => Factory.GetCachedValue<GVMSHaulierType>();

		public override CodeDescriptionPairList TransportModeList => Factory.GetCachedValue("GVMS_TransportModeList", () =>
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Core.Constants.TransportModes.Sea, Core.Constants.TransportModeDescriptions.Sea);
			result.AddPair(Core.Constants.TransportModes.Road, Core.Constants.TransportModeDescriptions.Road);
			result.AddPair(Core.Constants.TransportModes.RollOnRollOff, Core.Constants.TransportModeDescriptions.RollOnRollOff);
			return result;
		});
	}
}
