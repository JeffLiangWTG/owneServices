using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.UniversalDataBuss.Management
{
	public class AcknowledgementUniversalEventCreator : IAcknowledgementUniversalEventCreator
	{
		public IXmlEventValueObject CreateUniversalEvent(Dictionary<string, string> acknowledgementContextCollection, string processingResultStatus, string dataImportLog, IEnumerable<IValidationRule> validationRules)
		{
			Argument.NotNull(processingResultStatus, nameof(processingResultStatus));
			Argument.NotNull(dataImportLog, nameof(dataImportLog));

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(new BusinessObjectFactory().Load<IGlbCompany>(Env.CurrentCompany.PK));

			var contextCollection = new List<Context>();
			if (!acknowledgementContextCollection.IsNullOrEmpty())
			{
				foreach (var keyValuePair in acknowledgementContextCollection)
				{
					var context = new Context();
					context.Type = new ContextType() { Type = keyValuePair.Key };
					context.Value = keyValuePair.Value;
					contextCollection.Add(context);
				}
			}

			var universalEvent = new UniversalEvent()
			{
				EventTime = ZDateTimeOffset.Now,
				EventType = AutoEvents.DataImport.Code,
				DataContext = dataContext,
				ContextCollection = contextCollection
			};

			if (validationRules != null)
			{
				if (universalEvent.ValidationRuleCollection == null)
				{
					universalEvent.ValidationRuleCollection = new List<ValidationRule>();
				}

				universalEvent.ValidationRuleCollection.AddRange(validationRules.Cast<ValidationRule>());
			}

			var dataImportLogContext = new Context();
			dataImportLogContext.Type = new ContextType() { Type = "DataImportLog" };
			dataImportLogContext.Value = dataImportLog;
			universalEvent.ContextCollection.Add(dataImportLogContext);

			var processingResultStatusContext = new Context();
			processingResultStatusContext.Type = new ContextType() { Type = "ProcessingResultStatus" };
			processingResultStatusContext.Value = processingResultStatus;
			universalEvent.ContextCollection.Add(processingResultStatusContext);

			return universalEvent;
		}
	}
}
