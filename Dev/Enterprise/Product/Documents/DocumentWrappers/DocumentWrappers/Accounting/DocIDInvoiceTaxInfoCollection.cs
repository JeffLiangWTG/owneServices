using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocIDInvoiceTaxInfoCollection : DocumentWrapperCollection
	{
		#region Constructors && Type Overriding

		public DocIDInvoiceTaxInfoCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocIDInvoiceTaxInfo this[int index]
		{
			get { return (DocIDInvoiceTaxInfo)base[index]; }
		}

		#endregion
	}
}
