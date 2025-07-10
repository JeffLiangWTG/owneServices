using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class SupportingDocumentMetaData : IL.Business.SupportingDocumentMetaData
	{
		public SupportingDocumentMetaData(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new SupportingDocumentMetaDataLookups Lookups => (SupportingDocumentMetaDataLookups)base.Lookups;
		protected override CusCodeDataLookups GetNewLookups() => new SupportingDocumentMetaDataLookups(this);

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(SupportingDocument));
	}
}
