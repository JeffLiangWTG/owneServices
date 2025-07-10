using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using ApplicationCodeTypeList = Enterprise.Customs.ManifestBase.ApplicationCodeTypeList;

namespace Enterprise.Customs.ASYCUDA.Module
{
	public class ASYCUDAManifestController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		internal protected ApplicationBusinessProvider Provider;

		internal protected ZString CountryCode;

		internal protected bool CreateVOC;

		#region Security
		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.AsycudaManifestReporting; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.AsycudaManifestReporting; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.AsycudaManifestReporting; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.AsycudaManifestReporting; }
		}
		#endregion

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.ASYCUDA.ASYCUDAManifest; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.ASYCUDA.Manifest; }
		}

		public override System.Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(AsycudaManifestHeader); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			var typeDecider = new AsycudaManifestHeaderTypeDecider();
			var countryCode = CountryCode;
			var applicationCode = CreateVOC ? ApplicationCodeTypeList.Codes.ShippingLine : ApplicationCodeTypeList.Codes.Consolidator;
			var randomManifestType = Provider?.ManifestTypes.FirstOrDefault(manifestType => manifestType.ApplicableManifestStyles.Contains(applicationCode))?.Code ?? ZString.Empty;
			var result = (AsycudaManifestHeader)Factory.New(typeDecider.GetGlobalManifestType(Factory, countryCode, randomManifestType, applicationCode));

			using (result.GetValidationSuspender())
			using (result.GetCheckBusinessObjectTypeSuspender())
			{
				if (CreateVOC)
				{
					result.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
				}

				if (!countryCode.IsEmpty &&
					countryCode != Core.Constants.CountryCodes.EuropeanUnion &&
					result.AMA_RN_NKCountry != countryCode)
				{
					result.AMA_RN_NKCountry = countryCode;
				}

				if (!string.IsNullOrEmpty(randomManifestType))
				{
					result.AMA_ManifestType = randomManifestType;
				}
			}

			return result;
		}

		public IZForm ShowEditForm(BusinessObject sourceEntity, bool skipRecentItemsToSet)
		{
			skipRecentItems = skipRecentItemsToSet;
			return ShowEditForm(sourceEntity);
		}

		bool? skipRecentItems;

		protected sealed override IZForm GetForm(IBusiness businessEntity)
		{
			var header = businessEntity as AsycudaManifestHeader;
			var result = GetFormCore(header);
			result.SkipRecentItems = skipRecentItems;
			return result;
		}

		protected virtual ManifestForm GetFormCore(AsycudaManifestHeader header)
		{
			return new ManifestForm(header);
		}

		protected override IZForm ShowFormForNewEntityCore(IBusiness businessEntity)
		{
			CollectionForDefaultsAndValidation = null;
			return base.ShowFormForNewEntityCore(businessEntity);
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new AsycudaConsolPlugin((ForwardingConsol)businessEntity);
		}

		public override ResourceStringData PluginTabPageCaption
		{
			get { return Enterprise.Customs.ASYCUDA.Module.Res.GetData("PlugInTabPage|ASYCUDAManifestController", "Manifest"); }
		}
	}
}
