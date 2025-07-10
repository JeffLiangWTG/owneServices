using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public sealed class ImportEntryInstructionDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new EntryInstructionDetailsLayoutBuilder();
			var deBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(deBag.StyleDropEdit, ControlWidthClass.Long);
			builder.Add(deBag.SubStyleDropEdit, ControlWidthClass.Long);
			builder.Add(deBag.DescriptionTextBox, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(deBag.AdditionalInfoTextBox, ControlWidthClass.Long);
			builder.Add(deBag.CPCDropEdit, ControlWidthClass.Long);
			builder.Add(deBag.AuthorisationNumberDropEdit, ControlWidthClass.Long);

			builder.SetCaption(deBag.StyleDropEdit, i => EntryInstructionDetailBasicUserControl.DefaultStyleCaption, i => i.CEI_StyleInfo);
			builder.SetCaption(deBag.SubStyleDropEdit, i => EntryInstructionDetailBasicUserControl.DefaultSubStyleCaption, i => i.CEI_SubStyleInfo);

			var authorisationNumberEnabledStyles = new ZString[] { ImportDeclarationTypeList.Codes.AAV, ImportDeclarationTypeList.Codes.VAV };
			builder.SetVisibility(deBag.AuthorisationNumberDropEdit, i => authorisationNumberEnabledStyles.Contains(i.CEI_Style) && (i.JobDeclaration?.IsImport ?? false), i => i.CEI_AuthorisationNumberInfo);

			return builder.Build();
		}
	}
}
