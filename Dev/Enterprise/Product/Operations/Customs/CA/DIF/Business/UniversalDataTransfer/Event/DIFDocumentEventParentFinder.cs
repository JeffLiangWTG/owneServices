using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.CA.DIF.Business.UniversalDataTransfer
{
	public class DIFDocumentEventParentFinder : EventParentFinder
	{
		public DIFDocumentEventParentFinder(BusinessObjectFactory factory, IEventDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent xmlEvent)
		{
			JobRequiredDocumentAddInfo[] result = null;
			var urn = xmlEvent?.DataContext?.DataTargetCollection?.FirstOrDefault()?.Key;
			if (urn.HasValue)
			{
				var query = new ZDBOnlyQuery(typeof(JobRequiredDocumentAddInfo));
				_ = query.AddToFilter(JobRequiredDocumentAddInfoSchema.EX_ReferenceNumber, urn);
				_ = query.AddToFilter(JobRequiredDocumentAddInfoSchema.EX_ApplicationCode, Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF);
				_ = query.AddToFilter(JobRequiredDocumentAddInfoSchema.EX_GC_Company, GlbCompany.CurrentCompany.PK);

				var jobRequiredDocumentAddInfo = factory.LoadTop1<JobRequiredDocumentAddInfo>(query);
				if (jobRequiredDocumentAddInfo != null)
				{
					result = new JobRequiredDocumentAddInfo[] { jobRequiredDocumentAddInfo };
				}
			}
			return result;
		}
	}
}
