using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IT.NCTS.GUI;

sealed class ClickableContextForTest : ClickableContext
{
	public ClickableContextForTest(NctsHeader header) : base(header)
	{
	}

	#region ClickableContext

	public override ResourceString Caption => ResString.GetMultilingualString("5C87CD1F-1670-4643-A5F7-34CCF64AEED6", "Clickable Context For Test");
	public override string Name => "ClickableContextForTest";
	public override bool Visible => true;
	public override bool Enabled => true;
	public override void Execute(IClickableItem clickableItem) => Globals.Message.Show(Caption);

	#endregion
}
