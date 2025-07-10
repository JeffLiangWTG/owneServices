using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.Declaration;
using CusEntryHeader = Enterprise.Customs.NZ.Business.Declaration.FormalEntry.CusEntryHeader;
using CusEntryLineCollection = Enterprise.Customs.Business.ICusEntryLineCollection<Enterprise.Customs.NZ.Business.Declaration.CusEntryLine>;

namespace Enterprise.DocumentWrappers.Customs.NZ.FormalEntry
{
	public class DocCusEntryLineCollectionPaddedForEntryPrint : DocCusEntryLineCollection
	{
		public DocCusEntryLineCollectionPaddedForEntryPrint(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocCusEntryLineCollectionPaddedForEntryPrint(CusEntryLineCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
			if (Count != 0 && collectionSource.Count != 0)
			{
				CusEntryHeader entryHeader = (CusEntryHeader)collectionSource[0].EntryHeader;
				if (entryHeader != null)
				{
					JobDeclaration declaration = entryHeader.Declaration;
					if (declaration != null)
					{
						int linesPrintedOnAFollowPage = 0;
						if (declaration.IsImport)
						{
							linesPrintedOnAFollowPage = 2;
						}
						else if (declaration.IsExport)
						{
							linesPrintedOnAFollowPage = 3;
						}

						if (linesPrintedOnAFollowPage != 0)
						{
							int linesAlreadyOnLastPage = (Count - 1) % linesPrintedOnAFollowPage;
							if (linesAlreadyOnLastPage != 0)
							{
								int linesToAdd = (linesPrintedOnAFollowPage - linesAlreadyOnLastPage);
								for (int counter = linesToAdd; counter > 0; counter--)
								{
									Add(DocCusEntryLine.New(Factory));
								}
							}
						}
					}
				}
			}
		}
	}
}
