using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Universal
{
	public class AccEInvoicingCredentialEventParentFinder : EventParentFinder
	{
		public AccEInvoicingCredentialEventParentFinder(BusinessObjectFactory factory, AccEInvoicingCredentialDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent xmlEvent)
		{
			var companyCodeContext = xmlEvent.ContextCollection?.FirstOrDefault(x => x.Type == "CompanyCode");
			if (companyCodeContext == null || !companyCodeContext.Value.HasValue)
			{
				return null;
			}

			var passwordTypeContext = xmlEvent.ContextCollection?.FirstOrDefault(x => x.Type == "PasswordType");
			if (passwordTypeContext == null || !passwordTypeContext.Value.HasValue)
			{
				return null;
			}

			var company = factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, companyCodeContext.Value.Value);
			if (company == null)
			{
				return null;
			}

			var credentials = ObjectFactory.Get<IGlobalEInvoicingObjectFactory>().GetCountryEInvoicingObjectFactorySettings(company.GC_RN_NKCountryCode)?.Credentials;

			if (credentials == null || credentials.PasswordType != passwordTypeContext.Value.Value)
			{
				return null;
			}

			var behaviorProvider = credentials as IEInvoicingCredentialXUEBehaviorProvider;
			if (behaviorProvider == null)
			{
				return null;
			}

			var query = new ZQuery(GlbExternalPasswordSchema.GP_GC, company.PK)
				.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, passwordTypeContext.Value.Value);

			return factory.Load<GlbExternalPassword>(query);
		}
	}
}
