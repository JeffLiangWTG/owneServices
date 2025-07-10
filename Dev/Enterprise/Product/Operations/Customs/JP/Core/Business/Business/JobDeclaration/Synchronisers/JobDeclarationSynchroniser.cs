using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.JP.Business;

public sealed class JobDeclarationSynchroniser(JobDeclaration declaration) : Customs.Business.JobDeclarationSynchroniser(declaration)
{
	public new JobDeclaration Destination => (JobDeclaration)base.Destination;

	protected override void HookSynchronisers()
	{
		base.HookSynchronisers();
		Synchronisers.Add(new FieldSynchroniser(Destination.JE_ReceiptModeInfo, GetConvertedReceiptMode, () => new ZPropertyInfo[] { Source.JS_HBLContainerPackModeOverrideInfo }));
		Synchronisers.Add(new FieldSynchroniser(Destination.JE_DeliveryModeInfo, GetConvertedDeliveryMode, () => new ZPropertyInfo[] { Source.JS_HBLContainerPackModeOverrideInfo }));
	}

	IZType GetConvertedReceiptMode()
	{
		var sourceValue = Source.JS_HBLContainerPackModeOverride.Split('/');
		return sourceValue.Length == 2 ? ConvertHBLContainerPackModeToReceiptMode(sourceValue[0]) : ZString.Empty;
	}

	IZType GetConvertedDeliveryMode()
	{
		var sourceValue = Source.JS_HBLContainerPackModeOverride.Split('/');
		return sourceValue.Length == 2 ? ConvertHBLContainerPackModeToDeliveryMode(sourceValue[1]) : ZString.Empty;
	}

	ZString ConvertHBLContainerPackModeToReceiptMode(ZString hBLContainerPackMode)
	{
		return hBLContainerPackMode.ToString() switch
		{
			Common.Constants.HBLDeliveryModes.CY => ReceiptModeList.Codes.Mode51,
			Common.Constants.HBLDeliveryModes.CFS => ReceiptModeList.Codes.Mode52,
			Common.Constants.HBLDeliveryModes.DOOR => ReceiptModeList.Codes.Mode53,
			Common.Constants.HBLDeliveryModes.PORT => ReceiptModeList.Codes.Mode54,
			_ => ZString.Empty
		};
	}

	ZString ConvertHBLContainerPackModeToDeliveryMode(ZString hBLContainerPackMode)
	{
		return hBLContainerPackMode.ToString() switch
		{
			Common.Constants.HBLDeliveryModes.CY => DeliveryModeList.Codes.Mode51,
			Common.Constants.HBLDeliveryModes.CFS => DeliveryModeList.Codes.Mode52,
			Common.Constants.HBLDeliveryModes.DOOR => DeliveryModeList.Codes.Mode53,
			Common.Constants.HBLDeliveryModes.PORT => DeliveryModeList.Codes.Mode54,
			_ => ZString.Empty
		};
	}
}
