using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.ArchiveManager.Integration;
using Enterprise.DocumentEngine;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business.Actions
{
	public class DocumentCommandCache
	{
		[SuppressMessage("Reference", "CW1060:Do not use System.DateTime.Now Rule", Justification = "local date time only for caching")]
		public List<DocumentCommand> GetDocumentCommands(BusinessContext context)
		{
			if (DateTime.Now - lastClearedTime > CacheTimeOutInMinutes)
			{
				Clear();
			}

			if (!documentCommandCache.TryGetValue(context, out var result))
			{
				var docCommandFactory = new BusinessObjectFactory();
				docCommandFactory.RefreshEnabled = false;
				docCommandFactory.Saving += (factory) => throw new InvalidOperationException("This is a readonly Factory.");

				var additionalFilter = new ZQuery(StmMenuItemSchema.SU_IncludeDocInArchive, ArchiveConstants.IncludeDocInArchiveCodes.Yes);

				var documentCommands = new DocumentCommandCollection(docCommandFactory, additionalFilter);
				documentCommands.Load(new BusinessContext[] { context });

				result = new List<DocumentCommand>();

				foreach (var doc in documentCommands.Cast<DocumentCommand>())
				{
					result.Add(doc);
				}

				documentCommandCache.Add(context, result);
			}

			return result;
		}

		[SuppressMessage("Reference", "CW1060:Do not use System.DateTime.Now Rule", Justification = "local date time only for caching")]
		public void Clear()
		{
			documentCommandCache.Clear();
			lastClearedTime = DateTime.Now;
		}

		static readonly TimeSpan CacheTimeOutInMinutes = TimeSpan.FromMinutes(60);
		readonly Dictionary<BusinessContext, List<DocumentCommand>> documentCommandCache = new();

		[SuppressMessage("Reference", "CW1060:Do not use System.DateTime.Now Rule", Justification = "local date time only for caching")]
		DateTime lastClearedTime = DateTime.Now;
	}
}
