using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Universal
{
	public class AccEInvoicingCredentialDataContextManager : EventDataContextManager<GlbCompanyEInvoicingCertificateCredential>, IEventDataContextManager
	{
		public override DataContextType DataContextType => DataContextType.AccEInvoicingCredential;

		public override ZString DataContextKey => string.Empty;

		public override string DefaultOutputDirectory => null;

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return ZQuery.NoResultQuery;
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			return Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new AccEInvoicingCredentialEventParentFinder(factory, this, logger);
		}
	}
}
