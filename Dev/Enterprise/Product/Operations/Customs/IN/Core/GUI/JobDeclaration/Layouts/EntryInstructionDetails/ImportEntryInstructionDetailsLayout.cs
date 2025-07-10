using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public class ImportEntryInstructionDetailsLayout : IPanelLayoutProvider
{
	#region IPanelLayoutProvider

	PanelLayout IPanelLayoutProvider.Layout => layout ?? (layout = CreateLayout());
	PanelLayout layout;

	#endregion

	PanelLayout CreateLayout()
	{
		var builder = new EntryInstructionsDetailsLayoutBuilder();
		var commonBag = builder.CommonBag;

		builder.AddColumn();

		return builder.Build();
	}
}
