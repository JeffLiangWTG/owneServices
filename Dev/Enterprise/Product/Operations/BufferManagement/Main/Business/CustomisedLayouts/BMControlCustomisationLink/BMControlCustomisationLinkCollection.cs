using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class BMControlCustomisationLinkCollection : ActiveBusinessObjectCollection<BMControlCustomisationLink>, IBMControlCustomisationLinkCollection
	{
		public BMControlCustomisationLinkCollection(BusinessObject parent)
			: base(parent.Factory, parent, new ZQuery(), BMControlCustomisationLinkSchema.FML_ParentId)
		{
		}

		public BMControlCustomisationLinkCollection(BusinessObjectFactory factory, ZQuery query)
			: base(factory, query)
		{
		}

		#region IBMControlCustomisationLinkCollection Members

		IBMControlCustomisationLink IBMControlCustomisationLinkCollection.this[int index]
		{
			get { return base[index]; }
		}

		#endregion
	}
}
