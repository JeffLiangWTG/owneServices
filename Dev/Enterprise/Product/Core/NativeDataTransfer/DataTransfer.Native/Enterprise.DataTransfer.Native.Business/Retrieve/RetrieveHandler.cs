using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Common;
using Enterprise.DataTransfer.Native.Business.Requests;
using Enterprise.DataTransfer.Native.Business.Responses;
using Enterprise.DataTransfer.Native.Business.Xml.Serializers;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;
using Enterprise.DataTransfer.Native.Common.Logging;
using Enterprise.DataTransfer.Native.Utils;
using Enterprise.Integration;

namespace Enterprise.DataTransfer.Native.Business.Retrieve
{
	public class RetrieveHandler : IHandler
	{
		public RetrieveHandler()
		{
			DefinitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			Generator = new EntitySetXmlSerializer();
		}
		#region Dependency

		internal IDefinitionFinder DefinitionFinder;
		protected readonly EntitySetXmlSerializer Generator;

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		public Response Execute(Request request, IResponseFactory responseFactory)
		{
			var retrieveRequest = new RetrieveRequest(request);

			var logger = new MemoryLogger();
			var servicesSession = new AncillaryImportServices(logger);

			var xElementSetResults = Array.Empty<XElement>();
			var entitySetName = string.Empty;

			try
			{
				var validation = retrieveRequest.Validate();

				if (!validation.IsSuccess)
				{
					logger.Error(validation.Message);
				}
				else
				{
					var element = retrieveRequest.Body;
					entitySetName = GetEntityName(element);
					var definition = GetEntityDefinition(entitySetName);
					NativeXMLSupportValidator.CheckEntityIsSupported(definition.Name, definition.Root.EntityName);
					var criteriaGroups = GetCriteriaGroups(element);
					CheckCriteriaGroup(criteriaGroups);
					var entitySetResults = new HashSet<IEntity>();

					foreach (var criteriaGroup in criteriaGroups)
					{
						try
						{
							var retriever = RetrieverFactory.GetRetriever(criteriaGroup.Type, servicesSession);
							var entitySetResult = retriever.Retrieve(definition, criteriaGroup.Criterias).ToHashSet();
							entitySetResults.UnionWith(entitySetResult);
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							logger.LogOrReportException(ex, retrieveRequest.Body.ToString(), servicesSession);
						}
					}

					xElementSetResults = entitySetResults.Select(entity => Generator.Serialize(entity)).ToArray();
					xElementSetResults.ForEach(rootElement => rootElement.SetDefaultNameSpace(responseFactory.NameSpace));
					logger.Information(xElementSetResults.Length + " matches found.");
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				logger.LogOrReportException(ex, retrieveRequest.Body.ToString(), servicesSession);
			}

			return BuildResponse(responseFactory, logger.Buffer, xElementSetResults, entitySetName);
		}

		Response BuildResponse(IResponseFactory responseFactory, ILogBuffer logBuffer, XElement[] xElementSetResults, string entitySetName)
		{
			var response = responseFactory.GetNewResponse();
			response.Status = logBuffer.GetStatus();
			response.Informations = logBuffer.Logs().Select(l => l.ToString()).ToArray();
			response.DataItems = xElementSetResults;
			response.EntityInfo.Name = entitySetName;
			return response;
		}

		#region Implementation

		// TODO: Change to XDocument
		EntitySetDefinition GetEntityDefinition(string entitySetName)
		{
			return DefinitionFinder.FindByEntitySetName(entitySetName);
		}

		string GetEntityName(XElement xmlDoc)
		{
			var entityName = xmlDoc.Name.LocalName;
			CheckEntityName(entityName);
			return entityName;
		}

		internal IEnumerable<CriteriaGroup> GetCriteriaGroups(XElement element)
		{
			element.RemoveNameSpace(element.Name.Namespace);
			var criteriaGroupElements = element.Elements(TagName.CriteriaGroup);

			foreach (var criteriaGroupElement in criteriaGroupElements)
			{
				var typeElement = criteriaGroupElement.Attribute(TagName.Type);
				CheckCriteriaType(typeElement);
				var type = typeElement.Value;

				var criteriaElements = criteriaGroupElement.Elements(TagName.Criteria);
				var criterias = GetCriteria(criteriaElements);

				var criteriaGroup = new CriteriaGroup { Type = type, Criterias = criterias };
				yield return criteriaGroup;
			}
		}

		IEnumerable<EntityCriteria> GetCriteria(IEnumerable<XElement> criteriaItems)
		{
			foreach (var criteriaItem in criteriaItems)
			{
				yield return new EntityCriteria
				{
					EntityName = GetValueFromCriteria(criteriaItem, TagName.Entity),
					PropertyName = GetValueFromCriteria(criteriaItem, TagName.FieldName),
					Value = criteriaItem.Value
				};
			}
		}

		static string GetValueFromCriteria(XElement criteriaItem, string attribute)
		{
			try
			{
				return criteriaItem.Attribute(attribute).Value;
			}
			catch (NullReferenceException)
			{
				throw new NativeXMLUserVisibleException($"Criteria was missing attribute {attribute}");
			}
		}

		#region Check

		void CheckCriteriaType(XAttribute type)
		{
			if (type == null)
			{
				throw new NativeXMLUserVisibleException("Retrieve type is empty");
			}
			if (type.Value.IsEmpty())
			{
				throw new NativeXMLUserVisibleException("Retrieve type is empty");
			}
		}

		void CheckCriteriaGroup(IEnumerable<CriteriaGroup> criteriaGroups)
		{
			if (!criteriaGroups.Any())
			{
				throw new NativeXMLUserVisibleException("Could not find valid criteria group");
			}
		}

		void CheckEntityName(string entityName)
		{
			if (entityName.IsEmpty())
			{
				throw new NativeXMLUserVisibleException("EntitySet name not specified");
			}
		}

		#endregion

		#endregion
	}
}
