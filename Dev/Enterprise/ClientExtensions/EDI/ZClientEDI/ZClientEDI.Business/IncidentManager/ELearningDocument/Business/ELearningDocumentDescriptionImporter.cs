using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace ZClientEDI.Business.IncidentManager.ELearningDocument.Business
{
	public class ELearningDocumentDescriptionImporter
	{
		readonly BusinessObjectFactory Factory;
		public ELearningDocumentDescriptionImporter(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		public bool RecordExists(Guid myAccountDocumentId)
		{
			var query = new ZQuery(ELearningDocumentDescriptionSchema.ELD_MyAccountDocumentId, myAccountDocumentId);
			return Factory.ExistsInDatabase(AutoELearningDocumentDescription.Schema.TableName, query);
		}

		public ELearningDocumentDescription Load(Guid myAccountDocumentId)
		{
			var query = new ZQuery(ELearningDocumentDescriptionSchema.ELD_MyAccountDocumentId, myAccountDocumentId);
			return Factory.LoadTop1<ELearningDocumentDescription>(query);
		}

		public ELearningDocumentDescription Create()
		{
			return Factory.New<ELearningDocumentDescription>();
		}

		public ZDateTime LastUpdateDate()
		{
			var query = new ZQuery();
			var lastRecord = (Factory.Load<ELearningDocumentDescription>(query)).OrderByDescending(c => c.DocumentLastModified).FirstOrDefault();
			return lastRecord == default(ELearningDocumentDescription) ? new DateTime(2000, 1, 1) : lastRecord.DocumentLastModified;
		}

		ZDateTime LastUpdatedTfIdf()
		{
			var query = new ZQuery();
			query.OrderBy = ELearningDocumentTfIdfSchema.Constants.EDT_DocumentLastModified + OrderByClause.Descending;
			var lastRecord = Factory.LoadTop1<ELearningDocumentTfIdf>(query);
			return lastRecord == default(ELearningDocumentTfIdf) ? new DateTime(2000, 1, 1) : lastRecord.DocumentLastModified;
		}

		public IEnumerable<(string url, ZGuid id)> GetDescriptions()
		{
			var lastUpdate = LastUpdateDate();
			var lasTfIdfUpdate = LastUpdatedTfIdf();
			if (lastUpdate.CompareTo(lasTfIdfUpdate) > 0)
			{
				lastUpdate = lasTfIdfUpdate;
			}
			var query = new ZQuery();
			query.AddToFilter(JoinCondition.And, ELearningDocumentDescriptionSchema.ELD_DocumentLastModified, SQLComparisonOperator.GreaterThanOrEqualTo, lastUpdate);
			return Factory.Load<ELearningDocumentDescription>(query).Select(c => (c.Url, c.PK));
		}
	}
}
