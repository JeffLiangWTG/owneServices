using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public class EntryInstructionsCoreDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, Customs.GUI.EntryInstructionBasicDetailsControlBag> where T : CusEntryInstruction
	{
		public override Customs.GUI.EntryInstructionBasicDetailsControlBag CommonBag => Customs.GUI.EntryInstructionBasicDetailsControlBag.Instance;

		public EntryInstructionBasicDetailsControlBag EUBag { get; } = EntryInstructionBasicDetailsControlBag.Instance;

		protected override int MaxColumns => 3;
	}
}
