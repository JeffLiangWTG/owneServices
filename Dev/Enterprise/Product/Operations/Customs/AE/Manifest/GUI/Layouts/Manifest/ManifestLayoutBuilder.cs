using Enterprise.Customs.AE.Manifest.Business;
using Enterprise.Customs.ASYCUDA.GUI;

namespace Enterprise.Customs.AE.Manifest.GUI;

public sealed class ManifestLayoutBuilder : ManifestLayoutBuilder<AsycudaManifestHeader>
{
	protected override int MaxColumns => 3;

	protected override void SetDefaultCaptions()
	{
		SetCaption(CommonBag.ManifestNumberFromMasterBillTextBox, h => (h.AMA_AgentType == Core.Constants.AgentType.CoLoad) ? Res.GetData("98F0D90E-85BC-4B0A-8AEE-469ACFE19819", "Co-Load MBL") : Res.GetData("CF18FF9D-0BBD-485E-BDF0-D66C9791A036", "BOL"), h => h.AMA_AgentTypeInfo);
		SetCaption(CommonBag.CarrierAddressControl, h => (h.AMA_AgentType == Core.Constants.AgentType.CoLoad) ? Res.GetData("E01535B1-4142-4CC9-8C03-B843A400C50C", "Co-Loader") : Res.GetData("40FE5501-3540-40A9-8B63-C45BEAE42D55", "Carrier"), h => h.AMA_AgentTypeInfo);
		SetCaption(CommonBag.ShippingAgentAddressControl, h => (h.AMA_AgentType == Core.Constants.AgentType.CoLoad) ? Res.GetData("46D43DA3-E67D-42B6-9630-FFDF786E0885", "Sub Co-Loader") : Res.GetData("2BAFF5E0-1B12-42F4-9962-64AC1D3DCB73", "Default Frt. Forwarder", "Default Freight Forwarder", ""), h => h.AMA_AgentTypeInfo);
	}
}
