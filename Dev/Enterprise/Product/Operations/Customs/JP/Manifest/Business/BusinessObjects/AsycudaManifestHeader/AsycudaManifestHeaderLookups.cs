using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using GlbStaffWrapper = Enterprise.Customs.JP.Common.GlbStaffWrapper;

namespace Enterprise.Customs.JP.Manifest.Business
{
	public class AsycudaManifestHeaderLookups : ASYCUDA.Business.AsycudaManifestHeaderLookups
	{
		public AsycudaManifestHeaderLookups(AsycudaManifestHeader parent)
			: base(parent)
		{
		}

		protected new AsycudaManifestHeader Parent => (AsycudaManifestHeader)base.Parent;

		protected override RefUNLOCOCollection GetDischargePortListCore()
		{
			var collection = LookupsHelper.GetPortCollectionCore(Factory, Parent.PortOfDischargeIATACodeInfo, Parent.IsAir);
			AddIataCodeFilterIfNeeded(collection, !Parent.IsExport);

			return collection;
		}

		protected override RefUNLOCOCollection GetLoadingPortListCore()
		{
			var collection = LookupsHelper.GetPortCollectionCore(Factory, Parent.PortOfLoadingIATACodeInfo, Parent.IsAir);
			AddIataCodeFilterIfNeeded(collection, Parent.IsExport);

			return collection;
		}

		void AddIataCodeFilterIfNeeded(RefUNLOCOCollection collection, bool shouldStartWithJP)
		{
			collection.FilterBusinessObjectDefaults.Add(FilterBusinessObjectDefault.Create(
					filterName: "Code",
					propertyName: "Property",
					value: new ZString("JP"),
					comparisonOperator: shouldStartWithJP ? ModuleTextFilter.ComparisonConstants.StartsWith : ModuleTextFilter.ComparisonConstants.NotStartsWith));

			if (!Parent.IsSea)
			{
				collection.FilterBusinessObjectDefaults.Add(FilterBusinessObjectDefault.Create(
					filterName: "IATA Code",
					propertyName: "Property",
					value: ZString.Empty,
					instance: 0,
					comparisonOperator: ModuleTextFilter.ComparisonConstants.StartsWith));

				collection.FilterBusinessObjectDefaults.Add(FilterBusinessObjectDefault.Create(
					filterName: "IATA Code",
					propertyName: "Property",
					instance: 1,
					comparisonOperator: ModuleTextFilter.ComparisonConstants.IsNotBlank));
			}
		}

		public override CodeDescriptionPairList Natures => Factory.GetCachedValue<JPJobMessageTypeList>();

		public override CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<JPMessageStatusList>();

		public IList<GlbExternalPasswordCUS> NaccsCredentialList
		{
			get
			{
				var staff = Parent.CustomsAgent;
				var transportMode = Parent.AMA_TransportMode;
				if (staff != null)
				{
					return GlbStaffWrapper.Get(staff)
						.PasswordCollection
						.Where(pw => pw.GP_Transport == UserCodeSpecificTransportModeList.Codes.BTH || pw.GP_Transport == Parent.AMA_TransportMode)
						.ToList();
				}
				else
				{
					return Enumerable.Empty<GlbExternalPasswordCUS>().ToList();
				}
			}
		}

		public ICollection PortOfFinalDepartureCollection => JPRefCusCodeListTypes.GetJapanBondedAreaCodes(Factory, Parent.AMA_TransportMode);

		public override CodeDescriptionPairList RegistrationStatusList => Factory.GetCachedValue<JPCustomsStatusList>();

		public CodeDescriptionPairList MasterBillStatusList => Factory.GetCachedValue<JPMasterBillStatusList>();

		public ICollection ViaLocationCodeList => JPRefCusCodeListTypes.GetJapanBondedAreaCodes(Factory, Parent.AMA_TransportMode, Parent.MasterBill.ABL_GoodsLocation);
	}
}
