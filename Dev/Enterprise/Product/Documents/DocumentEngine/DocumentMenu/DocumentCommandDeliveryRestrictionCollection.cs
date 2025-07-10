using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine
{
	public class DocumentCommandDeliveryRestrictionCollection : ActiveBusinessObjectCollection<DocumentCommandDeliveryRestriction>
	{
		public DocumentCommandDeliveryRestrictionCollection(DocumentCommand parent)
			: base(parent.Factory, parent, new ZQuery(), StmMenuDeliveryRestrictionSchema.SDR_SU)
		{
		}
	}
}
