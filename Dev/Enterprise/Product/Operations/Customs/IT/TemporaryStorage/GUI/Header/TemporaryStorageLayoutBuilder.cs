using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.IT.TemporaryStorage.Business;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI;

sealed class TemporaryStorageLayoutBuilder : EU.TemporaryStorage.GUI.TemporaryStorageLayoutBuilder<TemporaryStorageHeader>
{
	protected override ResourceStringData GetArrivalTransportMeansCodeTextBoxCaption(EU.Business.CusTempStorage.TemporaryStorageHeader header)
	{
		return Res.GetData("E0B24E54-2DE4-4A60-BA78-60E380C5E8D6", englishCaptionOrFullDescription: "Transport ID");
	}
}
