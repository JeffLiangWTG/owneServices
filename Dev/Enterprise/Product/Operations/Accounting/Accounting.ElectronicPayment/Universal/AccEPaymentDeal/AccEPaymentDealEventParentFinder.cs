using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static CargoWise.EventReference.Constants;
using static Enterprise.Accounting.ElectronicPayment.Universal.AccEPaymentDealMessageConstants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicPayment.Universal
{
	public class AccEPaymentDealEventParentFinder : EventParentFinder
	{
		public AccEPaymentDealEventParentFinder(BusinessObjectFactory factory, AccEPaymentDealDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent xmlEvent)
		{
			if (xmlEvent.DataContext?.DataTargetCollection == null
				|| !xmlEvent.DataContext.DataTargetCollection.Any(x => x.Type.HasValue && x.Type.Value == AccEPaymentDealSchema.Constants.TableName))
			{
				return null;
			}

			if (!(xmlEvent.DataContext is UniversalDataBuss.DataObjects.Universal._2012_11.DataContext))
			{
				logger.LogBoth(LogType.Error, Res.GetString("3762052b-5b44-421d-9634-aa887810b965", "Unsupported XML format encountered: {0}.", xmlEvent.DataContext.GetType().FullName));
				return null;
			}

			var providerCode = EventParameters.GetEventParameter(EventReferenceParameters.Codes.MessageType, xmlEvent.EventParameters);
			if (!providerCode.HasValue || !EPaymentProviderCodes.CodesList.ContainsCode(providerCode.Value))
			{
				logger.LogBoth(LogType.Error, Res.GetString("28FEE919-4701-44B5-9735-BDFBC37F5AD1", "Could not find a valid Provider Code in the Event's <MessageType> parameter. Value found: '{0}'", providerCode));
				return null;
			}

			var messageSubType = EventParameters.GetEventParameter(EventReferenceParameters.Codes.MessageSubType, xmlEvent.EventParameters);
			if (!messageSubType.HasValue || !new ZString[] { AutoEvents.StatusUpdatedCode, MessageSubTypes.CreateADeal }.Contains(messageSubType.Value))
			{
				logger.LogBoth(LogType.Error, Res.GetString("3661776A-8A75-4642-A04D-734D00A1752B", "Could not find a valid value in the Event's <MessageSubType> parameter. Value found: '{0}'", messageSubType));
				return null;
			}

			var dealReference = xmlEvent.GetMatchingDataTarget(manager.DataContextType)?.Key ?? ZString.Empty;
			if (string.IsNullOrEmpty(dealReference))
			{
				logger.LogBoth(LogType.Error, Res.GetString("01ED1FC5-FE63-4F9C-AC40-FE72008B4C62", "Could not find a valid Deal Reference in the <DataTarget> Key parameter. Value found: '{0}'", dealReference));
				return null;
			}

			if (xmlEvent.ContextCollection == null)
			{
				logger.LogBoth(LogType.Error, Res.GetString("601C5BB7-BE4C-4931-84AE-DF982ADEEE54", "No Context Collection found."));
				return null;
			}

			var context = xmlEvent?.ContextCollection?.FirstOrDefault(x => x.Type == XUEFieldNames.CompanyCode);
			GlbCompany company = null;
			if (context == null || !context.Value.HasValue)
			{
				logger.LogBoth(LogType.Error, Res.GetString("70359F13-2D15-4743-A874-BD3A490BBA6B", "{0} context is missing in Universal Event.", XUEFieldNames.CompanyCode));
			}
			else
			{
				company = factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, context.Value.Value);
			}

			var query = new ZQuery(AccEPaymentDealSchema.AED_InternalReference, dealReference);
			query.AddToFilter(AccEPaymentDealSchema.AED_GC_Company, company?.PK ?? ZGuid.Empty);
			query.AddToFilter(AccEPaymentDealSchema.AED_ProviderCode, providerCode);
			return factory.Load<AccEPaymentDeal>(query);
		}
	}
}
