using CargoWise.Types;

namespace Enterprise.DocumentEngine
{
	static class DocumentPackLoader
	{
		public static void Load(PrintTaskDocumentPackLoader parentLoader, PrintTask task, DocumentPack pack, DocumentCommand currentCommand, DocumentCommand baseCommand, ZGuid sourcePivotPK = default(ZGuid))
		{
			if (pack.Count == 0 && currentCommand.ChildMenus.Count == 0)
			{
				parentLoader.ReasonsForEmptyPacks.AddRange(pack.ReasonsForEmptyPacks);
			}
			else
			{
				var id = new DocumentPackId(currentCommand.PK, currentCommand.Parent.DocumentSupporter.PK);
				if (!parentLoader.AddedPacks.Contains(id))
				{
					parentLoader.AddedPacks.Add(id);
					if ((currentCommand != baseCommand) && baseCommand.SU_IsDocPack && (parentLoader.AddedPacks.Count > 0))
					{
						if (task.Count > 0)
						{
							task[0].LastTemplateGeneratorLanguage = pack.LastTemplateGeneratorLanguage;

							foreach (IDeliverable deliverable in pack)
							{
								deliverable.SourcePivotPK = sourcePivotPK;
								task[0].Add(deliverable);
								var report = deliverable as Report;
								if (report != null)
								{
									report.SetParent(task[0]);
								}
							}

							foreach (IDeliverable deliverable in pack.OtherEDocsToAttach)
							{
								if (!task[0].OtherEDocsToAttach.Contains(deliverable))
								{
									task[0].OtherEDocsToAttach.Add(deliverable);
								}
							}
						}
						else
						{
							pack.SetDeliveryDetailsFromDocumentPrintSet(baseCommand);
							task.Add(pack);
						}
					}
					else if (pack.Count > 0 || (currentCommand.ChildMenus.Count > 0 && currentCommand.SU_IsDocPack))
					{
						task.Add(pack);
					}
				}
				else
				{
					pack.Dispose();
				}
			}

			ChildCommandsLoader.LoadChildCommands(currentCommand, parentLoader, sourcePivotPK, pack.Language);
		}
	}
}
