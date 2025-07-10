using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.ASYCUDA.Module;

public class ASYCUDAManifestConsolController : JobConsolController, IBusinessObjectLoader
{
	public override ControllerID ID
	{
		get { return ControllerIDs.Customs.ASYCUDA.ASYCUDAManifestConsol; }
	}

	public override ModuleIdentifier ModuleID
	{
		get { return ModuleIDs.Customs.ASYCUDA.Manifest; }
	}

	protected override string GetIDForFormCache(IBusiness businessEntity)
	{
		return ControllerIDs.JobConsol.ToString();
	}

	public override System.Type TypeOfTopLevelBusinessObject
	{
		get { return typeof(ForwardingConsol); }
	}

	public override IZForm ShowNewForm()
	{
		ZController controller = ZControllerFactory.Create(ControllerIDs.JobConsol);
		IZForm newForm = controller.ShowNewForm();
		LastShownForm = controller.LastShownForm;
		return newForm;
	}

	public override IZForm ShowEditForm(BusinessObject sourceEntity)
	{
		try
		{
			var countryCode = ZString.Empty;
			var manifestType = ZString.Empty;
			if (sourceEntity is AsycudaManifestHeader header)
			{
				headerPK = header.PK;
				countryCode = header.AMA_RN_NKCountry;
				manifestType = header.AMA_ManifestType;
			}

			var form = base.ShowEditForm(ConvertSourceEntity(sourceEntity));
			SelectTabByCountryCode(form, countryCode, manifestType);
			return form;
		}
		finally
		{
			headerPK = null;
		}
	}
	ZGuid? headerPK;

	protected override ZGuid GetCurrentPK(IBusiness sourceEntity)
	{
		return headerPK ?? base.GetCurrentPK(sourceEntity);
	}

	public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
	{
		return base.ShowDeleteForm(ConvertSourceEntity(sourceEntity));
	}

	public override IZForm ShowViewForm(BusinessObject sourceEntity)
	{
		var countryCode = ZString.Empty;
		var manifestType = ZString.Empty;
		if (sourceEntity is AsycudaManifestHeader header)
		{
			countryCode = header.AMA_RN_NKCountry;
			manifestType = header.AMA_ManifestType;
		}

		var form = base.ShowViewForm(ConvertSourceEntity(sourceEntity));
		SelectTabByCountryCode(form, countryCode, manifestType);
		return form;
	}

	static void SelectTabByCountryCode(IZForm form, ZString countryCode, ZString manifestType)
	{
		if (countryCode.IsValid && form is ConsolForm consolForm)
		{
			var plugIn = consolForm.PlugIns.GetPlugIn(ControllerIDs.Customs.ASYCUDA.ASYCUDAManifest);
			if (plugIn.Enabled)
			{
				(plugIn.UserControl as AsycudaManifestMainControl)?.SelectTabByCountry(countryCode, manifestType);
			}
		}
	}

	BusinessObject ConvertSourceEntity(BusinessObject sourceEntity)
	{
		var manifestHeader = sourceEntity as AsycudaManifestHeader;
		var consol = manifestHeader?.Consol;
		return consol ?? sourceEntity;
	}

	protected override ConsolForm GetFormCore(ForwardingConsol businessEntity)
	{
		var result = new ConsolForm(businessEntity);
		result.PlugInIDToSelectOnLoaded = ControllerIDs.Customs.ASYCUDA.ASYCUDAManifest;
		return result;
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

	BusinessObject IBusinessObjectLoader.Load(BusinessObjectFactory factory, ZGuid pk)
	{
		AsycudaManifestHeader result = null;
		var moduleResultsPKCollection = ModuleResultsPKCollection;
		if (moduleResultsPKCollection != null)
		{
			PKData? foundData = null;
			foreach (var data in moduleResultsPKCollection)
			{
				if (data.PK == pk)
				{
					foundData = data;
					break;
				}
			}
			if (foundData.HasValue)
			{
				result = factory.Load<AsycudaManifestHeader>(pk);
			}
		}
		return result;
	}
}
