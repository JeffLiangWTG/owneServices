using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocCASSBillingLineCollection : DocumentWrapperCollection
	{
		public DocCASSBillingLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocCASSBillingLine this[int index]
		{
			get { return (DocCASSBillingLine)base[index]; }
		}
	}
}
