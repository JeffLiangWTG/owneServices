using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.XmlIO.XmlWriting;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.GB.CDS.Messaging.MessageBuilders
{
	public class CDSQueryUniversalEventBuilder
	{
		public CDSQueryUniversalEventBuilder(IQueryDataProvider queryDataProvider)
		{
			QueryDataProvider = queryDataProvider;
		}

		public UniversalEvent BuildUniversalEvent(ZGuid sourcePK)
		{
			var universalEvent = new UniversalEvent
			{
				EventTime = ZDateTimeOffset.Now,
				EventType = Events.ServiceRequestedCode,
				EventReference = CreateEventReference(),
				DataContext = CreateDataContext(QueryDataProvider),
				ContextCollection = GetContextCollection(sourcePK)
			};

			return universalEvent;
		}

		public static string ConvertToXml(UniversalEvent universalEvent)
		{
			var xml = string.Empty;
			using (var stream = (SubStreamableStream)new MemoryStream())
			using (var reader = new StreamReader(stream))
			{
				new XmlWriter().WriteXML(universalEvent, stream, false, UniversalXmlInfo.Namespace_2011_11);
				stream.Flush();
				stream.Position = 0;
				xml = reader.ReadToEnd();
			}
			return xml;
		}

		List<Context> GetContextCollection(ZGuid sourcePK)
		{
			var contextCollection = QueryDataProvider.GetContextCollection(sourcePK).ToList();

			contextCollection.Add(new Context
			{
				Type = Key,
				Value = QueryDataProvider.CredentialKey
			});

			return contextCollection;
		}

		ZString CreateEventReference()
		{
			var parameters = new Dictionary<string, string>
			{
				{ CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, CDSQuery },
				{ CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Service, GBCustomsCDS }
			};
			return StmALog.GenerateEventReference(ZString.Empty, parameters);
		}

		IDataContextDataObject CreateDataContext(IQueryDataProvider dataWrapper)
		{
			var dataContext = DataContextFactory.New(UniversalXmlInfo.Namespace_2011_11);
			if (!dataWrapper.ContextReference.IsEmpty)
			{
				dataContext.AddDataSource(dataWrapper.ContextType, dataWrapper.ContextReference);
			}
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			return dataContext;
		}

		protected readonly IQueryDataProvider QueryDataProvider;

		const string GBCustomsCDS = "GBCustomsCDS";
		const string CDSQuery = "QUERY";
		const string Key = "Key";
	}
}
