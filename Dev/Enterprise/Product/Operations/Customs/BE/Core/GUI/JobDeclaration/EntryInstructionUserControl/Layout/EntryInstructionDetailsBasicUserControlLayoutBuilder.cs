using Enterprise.Customs.EU.GUI;
using CusEntryInstruction = Enterprise.Customs.BE.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.BE.GUI;

public class EntryInstructionDetailsBasicUserControlLayoutBuilder : EntryInstructionsCoreDetailsLayoutBuilder<CusEntryInstruction>
{
	EntryInstructionBasicDetailsControlBag euBag => EU.GUI.EntryInstructionBasicDetailsControlBag.Instance;

	protected override void SetDefaultCaptions()
	{
		base.SetDefaultCaptions();
		SetCaption(euBag.LocationOfGoodsUserControl, x => Res.GetData("43436AB8-E4FE-4E23-B888-6142FD7A83C8", "Location of Goods", "[UCC 5/23] Location of Goods"));
	}
}
