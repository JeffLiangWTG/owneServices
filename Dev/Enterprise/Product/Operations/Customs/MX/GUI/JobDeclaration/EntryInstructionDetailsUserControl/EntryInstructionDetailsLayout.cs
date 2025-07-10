using Enterprise.Customs.GUI;
using Enterprise.Customs.MX.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.MX.GUI;

public sealed class EntryInstructionDetailsLayout : IPanelLayoutProvider
{
	PanelLayout IPanelLayoutProvider.Layout => layout ?? (layout = CreateEntryInstructionDetailsBasicUserControlLayout());
	PanelLayout layout;

	static PanelLayout CreateEntryInstructionDetailsBasicUserControlLayout()
	{
		var builder = new EntryInstructionsCoreDetailsLayoutBuilder<CusEntryInstruction>();
		var mxBag = EntryInstructionDetailsControlBag.Instance;

		builder.AddControlBag(mxBag);
		builder.AddColumn();

		builder.Add(mxBag.UCRNumberTextBox, widthClass: ControlWidthClass.Long);

		return builder.Build();
	}
}
