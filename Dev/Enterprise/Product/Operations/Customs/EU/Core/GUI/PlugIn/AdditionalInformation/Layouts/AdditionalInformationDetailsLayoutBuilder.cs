using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public class AdditionalInformationDetailsLayoutBuilder : ColumnLayoutBuilder<AdditionalInfo, AdditionalInformationDetailsControlBag>
	{
		public override AdditionalInformationDetailsControlBag CommonBag { get; } = AdditionalInformationDetailsControlBag.Instance;

		protected override int MaxColumns => 1;
	}
}
