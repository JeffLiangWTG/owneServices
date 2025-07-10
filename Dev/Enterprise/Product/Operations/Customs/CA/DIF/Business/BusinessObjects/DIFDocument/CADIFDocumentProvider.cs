using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.CA.DIF;

namespace Enterprise.Customs.CA.DIF.Business
{
	public class CADIFDocumentProvider : ICADIFDocumentProvider
	{
		public IEnumerable<IDIFDocument> GetDIFDocuments(ICADIFHost host) => new DIFHostWrapper(host).DISDocuments.Cast<IDIFDocument>();

		IDIFDocument ICADIFDocumentProvider.GetDIFDocument(BusinessObjectFactory factory, ZString referenceNumber, ZString applicationCode, ZGuid companyPK)
		{
			var query = new ZQuery(JobRequiredDocumentAddInfoSchema.EX_ReferenceNumber, referenceNumber);
			query.AddToFilter(JobRequiredDocumentAddInfoSchema.EX_ApplicationCode, applicationCode);
			query.AddToFilter(JobRequiredDocumentAddInfoSchema.EX_GC_Company, companyPK);

			var addinfo = factory.LoadTop1<JobRequiredDocumentAddInfo>(query);
			var host = addinfo?.DisHost as ICADIFHost;
			DIFDocument document = null;
			if (host != null)
			{
				document = new DIFDocument(new DIFHostWrapper(host));
				document.RequiredDocumentAddInfo = addinfo;
				document.Initialize();
			}
			return document;
		}
	}
}
