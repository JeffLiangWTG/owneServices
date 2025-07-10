using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class ImportEntriesDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout IPanelLayoutProvider.Layout => PanelLayout;

		PanelLayout PanelLayout { get; }

		public ImportEntriesDetailsLayout()
		{
			PanelLayout = CreatePanelLayout();
		}

		PanelLayout CreatePanelLayout()
		{
			var layout = new PanelLayout();
			var bag = RefundDeclarationImportEntriesControlBag.Instance;
			layout.RegisterControlBag(bag);

			var ruler = layout.CreateRuler(151);
			layout.Include(ruler, bag.SequenceNumberCalcEdit);
			layout.Include(ruler, bag.ImportEntryNumberCodeFindBox);
			layout.Include(ruler, bag.ImportEntryLineNumCodeFindBox);
			layout.Include(ruler, bag.CustomsDisbursementBillCodeFindBox);
			layout.Include(ruler, bag.AmendSequenceNumber5WNDropEdit);

			return layout;
		}
	}
}
