using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business
{
	class IPSInactiveRatingHeaderArchiveStageDescriptor : IPSArchiveDescriptor
	{
		public override string Name
			=> Res.GetString("66B33922-0138-4DC9-BDFE-FBF2F62744B8", "Inactive Rating Header Archive");

		public override SchemaColumn MainArchivePKColumn
			=> RatingHeaderSchema.PK;

		public override SchemaColumn MainArchiveNKColumn
			=> RatingHeaderSchema.TH_QuoteNumber;

		public override SchemaDateTimeColumn MainDateFilterColumn
			=> RatingHeaderSchema.TH_SystemCreateTimeUtc;

		protected internal override SchemaBoolColumn IsCancelledSchemaColumn
			=> RatingHeaderSchema.TH_IsCancelled;

		public override ZQuery GetMainArchiveableFilter(IArchiveConfiguration config)
		{
			var query = base.GetMainArchiveableFilter(config);
			var sql = @"NOT EXISTS
(
	SELECT TA_TH FROM dbo.RateAttachment
	WHERE TA_TH = TH_PK
)";
			_ = query.AddFilterAndZSQLParameterCollection(sql, null);

			return query;
		}

		public override IEnumerable<IArchivePreparationAction> GetPreparationAction(IArchiveLogger logger, IArchiveSet archiveSet, IArchiveSystemCache cache, IArchiveConfiguration config)
		{
			var imageAction = ObjectFactory.Get<IArchiveImageGenerationAction>();
			imageAction.Setup(logger, archiveSet, BusinessObjectProviderDictionary, cache);
			yield return imageAction;
		}
	}
}
