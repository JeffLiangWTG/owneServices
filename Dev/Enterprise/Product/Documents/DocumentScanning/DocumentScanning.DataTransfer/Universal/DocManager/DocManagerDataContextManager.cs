using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.DocumentScanning.DataTransfer.Universal
{
	public class DocManagerDataContextManager : EventDataContextManager<BusinessObject>, IDataContextManagerForManyTypes
	{
		public override DataContextType DataContextType => DataContextType.DocManager;

		public override ZString DataContextKey => ParentBO?.GetDataManagerContextKey() ?? ZString.Empty;

		public override string DefaultOutputDirectory => null;

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return null;
		}

		protected override BusinessObject[] LoadBusinessObjectsFromDataContextKey(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			if (IsDataContextMatchingKeyValid(matchingValues, logger, out ZString docManagerCode, out ZString jobCode))
			{
				try
				{
					BusinessObject bizo;
					var assemblyData = AssemblyDataLookup.GetAssemblyDataFromDocManagerCode(docManagerCode);
					var universalXmlEDocSupporter = assemblyData?.GetEDocsViaUniversalXmlSupport();

					if (universalXmlEDocSupporter != null)
					{
						bizo = universalXmlEDocSupporter.LoadBusinessObjectFromCode(factory, jobCode);
					}
					else
					{
						bizo = AssemblyDataLookup.GetBusinessObjectFromCode(new DbBackendDocumentFactory(factory), docManagerCode, jobCode, matchingValues.CompanyCode, true);
					}

					if (bizo != null)
					{
						bizo.SetDataManagerContextKey(matchingValues.Key);
						return new BusinessObject[] { bizo };
					}
					else if (universalXmlEDocSupporter != null)
					{
						logger.Log(Enterprise.Integration.LogType.Warning, $"Expecting a key in the format '{docManagerCode} {universalXmlEDocSupporter.ExpectedCodeFormat}' for {assemblyData.HumanReadableName}. Example: '{docManagerCode} {universalXmlEDocSupporter.ExampleCodeFormat}'");
					}
				}
				catch (NonUniqueAllocationCodeException ex)
				{
					logger.Log(Enterprise.Integration.LogType.Error, ex.Message);
				}
			}

			return System.Array.Empty<BusinessObject>();
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			return Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new DocManagerEventParentFinder(factory, this, logger);
		}

		bool IsDataContextMatchingKeyValid(IDataContextMatchingKey matchingValues, IXmlImportLogger logger, out ZString docManagerCode, out ZString jobCode)
		{
			docManagerCode = matchingValues.Key.SubstringSafe(0, 3);
			jobCode = matchingValues.Key.SubstringSafe(4);

			if (!jobCode.IsEmpty && AssemblyDataLookup.IsDocManagerCodeValid(docManagerCode))
			{
				return true;
			}

			logger.Log(Enterprise.Integration.LogType.Error, $"Invalid Context Key: '{matchingValues.Key}'. The key must be of the format 'XXX JobNumber' where XXX is a valid 3 letter Document Manager Code.");
			return false;
		}

		class DocManagerEventParentFinder : EventParentFinder
		{
			internal DocManagerEventParentFinder(BusinessObjectFactory factory, DocManagerDataContextManager manager, IXmlImportLogger logger)
				: base(factory, manager, logger)
			{
			}

			protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent xmlEvent)
			{
				if (xmlEvent.DataContext?.DataTargetCollection != null && xmlEvent.DataContext.DataTargetCollection.Any())
				{
					var docManagerContext = xmlEvent.DataContext.DataTargetCollection.FirstOrDefault(context => context.Type.HasValue && context.Type.Value == nameof(DataContextType.DocManager));
					if (docManagerContext != null)
					{
						return new BusinessObject[] { new DocManagerWrapper(factory) };
					}
				}

				return null;
			}
		}
	}
}
