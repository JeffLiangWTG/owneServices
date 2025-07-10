
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Rating.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocIndexEntryCollection : DocumentWrapperCollection
	{
		public DocIndexEntryCollection(BusinessObjectFactory factory)
			: base(factory) { }

		public DocIndexEntryCollection(PricingPageCollection formatTableCollection, BusinessObjectFactory factory)
			: base(factory)
		{
			foreach (PricingPage page in formatTableCollection)
			{
				this.Add(DocIndexEntry.New(page, factory));
			}
		}

		public new DocIndexEntry this[int index]
		{
			get { return (DocIndexEntry)base[index]; }
		}
	}
}
