using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class SupportingDocumentCollection : EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection
	{
		public SupportingDocumentCollection(BusinessObject parent)
			: base(parent)
		{
		}
		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var deltaSupporter = ((SupportingDocument)child).Parent as IDeltaSupporter;
			var deltaMode = deltaSupporter?.DeltaMode ?? ZString.Empty;
		}

		public new SupportingDocument this[int i] => (SupportingDocument)base[i];

		public new SupportingDocument AddNew() => (SupportingDocument)base.AddNew();

		public new SupportingDocument AddNew(System.Type bizOType) => (SupportingDocument)base.AddNew(bizOType);
	}
}
