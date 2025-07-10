using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.CA.DIF.Business
{
	public class DIFHostWrapper : DISHostWrapperBase<DIFDocument>
	{
		public DIFHostWrapper(ICADIFHost difHost)
			: base(difHost)
		{
			this.DefaultValues = DISHost.ValueProvider;
		}

		public new ICADIFHost DISHost => (ICADIFHost)base.DISHost;
		internal readonly ICADIFDefaultValues DefaultValues;

		public override DISDocumentCollectionBase<DIFDocument> DISDocuments
		{
			get
			{
				if (disDocuments == null)
				{
					disDocuments = new DIFDocumentCollection(this);
					RegisterEditableChildObject(disDocuments);
				}
				return disDocuments;
			}
		}
		DIFDocumentCollection disDocuments;

		internal IeDoc GetEDoc(ZGuid pk)
		{
			return DISHost.EDocs.FirstOrDefault(eDoc => !eDoc.IsDeleted && eDoc.UniqueKey == pk);
		}
	}
}
