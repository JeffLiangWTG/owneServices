using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataConverters
{
	public class PartMatcher
	{
		public PartMatcher(ZString partNumber, OrgMatcher supplierMatch, OrgMatcher importerMatch, BusinessObjectFactory factory) : this(partNumber, supplierMatch, importerMatch, factory, typeof(OrgSupplierPart))
		{
		}

		public PartMatcher(ZString partNumber, OrgMatcher supplierMatch, OrgMatcher importerMatch, BusinessObjectFactory factory, Type typeOfPart)
		{
			if (!importerMatch.OriginalCode.IsEmpty && importerMatch.MatchedPK.IsEmpty)
			{
				AddReasonPartCannotBeImported("Importer Code: " + importerMatch.OriginalCode + " Could Not Be Matched");
			}
			if (!supplierMatch.OriginalCode.IsEmpty && supplierMatch.MatchedPK.IsEmpty)
			{
				AddReasonPartCannotBeImported("Supplier Code: " + supplierMatch.OriginalCode + " Could Not Be Matched");
			}
			if (ReasonPartCannotBeImported.IsEmpty)
			{
				ZQuery filter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, partNumber);
				OrgSupplierPart[] parts = (OrgSupplierPart[])factory.Load(typeOfPart, filter);

				foreach (OrgSupplierPart part in parts)
				{
					if (FoundAMatchOrGotARejection(part, supplierMatch, importerMatch))
					{
						break;
					}
				}
			}
		}

		bool FoundAMatchOrGotARejection(OrgSupplierPart part, OrgMatcher supplierMatch, OrgMatcher importerMatch)
		{
			bool cannotPossiblyBeAMatch = false;
			bool hasRightSupplier = false;
			bool alsoHasWrongSupplier = false;
			bool hasRightImporter = false;
			bool alsoHasWrongImporter = false;
			foreach (OrgPartRelation relation in part.RelatedOrganisations)
			{
				switch (relation.OU_Relationship)
				{
					case OrgPartRelation.RelationshipTypes.Supplier:
						if (supplierMatch.MatchedPK.IsEmpty)
						{
							cannotPossiblyBeAMatch = true;
						}
						else
						{
							if (relation.OU_OH == supplierMatch.MatchedPK)
							{
								hasRightSupplier = true;
							}
							else
							{
								alsoHasWrongSupplier = true;
							}
						}
						break;
					case OrgPartRelation.RelationshipTypes.Owner:
						if (importerMatch.MatchedPK.IsEmpty)
						{
							cannotPossiblyBeAMatch = true;
						}
						else
						{
							if (relation.OU_OH == importerMatch.MatchedPK)
							{
								hasRightImporter = true;
							}
							else
							{
								alsoHasWrongImporter = true;
							}
						}
						break;
				}
				if (cannotPossiblyBeAMatch)
				{
					break;
				}
			}

			if (!cannotPossiblyBeAMatch)
			{
				if ((hasRightSupplier || supplierMatch.MatchedPK.IsEmpty) && (hasRightImporter || importerMatch.MatchedPK.IsEmpty))
				{
					if (!alsoHasWrongSupplier && !alsoHasWrongImporter)
					{
						fMatchedPart = part;
					}
					else
					{
						if (alsoHasWrongSupplier)
						{
							AddReasonPartCannotBeImported("Part Matched but is linked to more Suppliers than: " + supplierMatch.MatchedCode);
						}
						if (alsoHasWrongImporter)
						{
							AddReasonPartCannotBeImported("Part Matched but is linked to more Importers than: " + importerMatch.MatchedCode);
						}
					}
				}
			}
			return (fMatchedPart != null || !ReasonPartCannotBeImported.IsEmpty);
		}

		public ZString ReasonPartCannotBeImported
		{
			get { return fReasonPartCannotBeImported; }
		}

		ZString fReasonPartCannotBeImported;

		void AddReasonPartCannotBeImported(ZString reason)
		{
			fReasonPartCannotBeImported += (fReasonPartCannotBeImported.IsEmpty ? "" : ", ") + reason;
		}

		OrgSupplierPart fMatchedPart;
		public OrgSupplierPart MatchedPart
		{
			get { return fMatchedPart; }
		}
	}
}
