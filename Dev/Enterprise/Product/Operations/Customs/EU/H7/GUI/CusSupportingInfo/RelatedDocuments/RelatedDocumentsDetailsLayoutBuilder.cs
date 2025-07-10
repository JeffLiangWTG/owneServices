using Enterprise.Customs.Business;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	public class RelatedDocumentsDetailsLayoutBuilder : ColumnLayoutBuilder<CusSupportingInfo, AdditionalInformationDetailsControlBag>
	{
		public override AdditionalInformationDetailsControlBag CommonBag { get; } = AdditionalInformationDetailsControlBag.Instance;

		protected override int MaxColumns => 1;
	}
}
