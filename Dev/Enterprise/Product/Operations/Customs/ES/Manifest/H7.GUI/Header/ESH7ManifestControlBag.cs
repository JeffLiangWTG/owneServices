using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.Manifest.H7.GUI
{
	public class ESH7ManifestControlBag : ControlBag
	{
		public static ESH7ManifestControlBag Instance => manifestControlBag.Value;

		public ESH7ManifestControlBag()
		{
			CusAgentCodeFindBox = RegisterControl(nameof(ESH7ManifestFieldsUserControl.CusAgentCodeFindBox));
			CertificateDropEdit = RegisterControl(nameof(ESH7ManifestFieldsUserControl.CertificateDropEdit));
			TrainingCheckBox = RegisterControl(nameof(ESH7ManifestFieldsUserControl.TrainingCheckBox));
			LocationOfGoodsUserControl = RegisterControl(nameof(ESH7ManifestFieldsUserControl.LocationOfGoodsUserControl));
			TransportDocumentTypeDropEdit = RegisterControl(nameof(ESH7ManifestFieldsUserControl.TransportDocumentTypeDropEdit));
			TransportDocumentReferenceTextBox = RegisterControl(nameof(ESH7ManifestFieldsUserControl.TransportDocumentReferenceTextBox));
			G3MRNToRevokeDropEdit = RegisterControl(nameof(ESH7ManifestFieldsUserControl.G3MRNToRevokeDropEdit));
			EntryLineNumberTextBox = RegisterControl(nameof(ESH7ManifestFieldsUserControl.EntryLineNumberTextBox));
		}

		public ControlReference CusAgentCodeFindBox { get; }
		public ControlReference CertificateDropEdit { get; }
		public ControlReference TrainingCheckBox { get; }
		public ControlReference LocationOfGoodsUserControl { get; }
		public ControlReference TransportDocumentTypeDropEdit { get; }
		public ControlReference TransportDocumentReferenceTextBox { get; }
		public ControlReference G3MRNToRevokeDropEdit {  get; }
		public ControlReference EntryLineNumberTextBox { get; }

		protected override System.Windows.Forms.Control CreateTemplate() => new ESH7ManifestFieldsUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		readonly static Lazy<ESH7ManifestControlBag> manifestControlBag = new Lazy<ESH7ManifestControlBag>(() => new ESH7ManifestControlBag());
	}
}
