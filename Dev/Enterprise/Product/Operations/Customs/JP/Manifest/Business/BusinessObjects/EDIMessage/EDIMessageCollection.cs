using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.JP.Common;

namespace Enterprise.Customs.JP.Manifest.Business
{
	public class EDIMessageCollection : EDIMessageCollectionNonDependent
	{
		public EDIMessageCollection(BusinessObjectFactory factory, AsycudaManifestHeader manifest)
			: base(factory, manifest)
		{
		}

		public new EDIMessage AddNew()
		{
			return (EDIMessage)base.AddNew();
		}

		public new EDIMessage this[int index]
		{
			get { return (EDIMessage)(base[index]); }
		}
	}
}
