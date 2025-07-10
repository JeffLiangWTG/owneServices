using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI;

public sealed class AuthorizationGridColumnLayout : IGridColumnLayoutProvider
{
	IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ??= CreateLayout();
	IGridColumnLayout layout;

	IGridColumnLayout CreateLayout()
	{
		var commonColumnBag = AuthorizationGridColumnsBag.Instance;
		var builder = GridColumnLayoutBuilder.Create();

		builder.AddColumn(commonColumnBag.AGC_CodeDropEditColumn);
		builder.AddColumn(commonColumnBag.CustomsCodeTextBoxColumn);
		builder.AddColumn(commonColumnBag.EffectiveReferenceNumberCodeFindBoxColumn);
		builder.AddColumn(commonColumnBag.AGC_OH_OwnerOrganisationFindBoxColumn);

		return builder.Build();
	}
}
