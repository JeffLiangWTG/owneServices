using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.CH;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using CusEntryHeader = Enterprise.Customs.CH.Business.CusEntryHeader;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class ExportEntryHeaderCollection : ModuleEntryHeaderCollection
{
	public ExportEntryHeaderCollection(NctsDepartureMovementHeader movementHeader) : base(movementHeader.Factory, GlbCompany.CurrentCompany, null)
	{
		this.movementHeader = movementHeader;

		FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(ModuleEntryHeaderCollection.FilterConstants.EntryNumber, "ComparisonOperator", (ZString)ModuleTextFilter.ComparisonConstants.IsNotBlank, false));
		FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Shipment Type", "Property", (ZString)CHJobMessageTypeList.Codes.Export, FilterOrCategory.Red, 1, false));
		FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Shipment Type", "Property", (ZString)CHJobMessageTypeList.Codes.ExportDeclarationActivation, FilterOrCategory.Red, 2, false));

		AdditionalFilter = new ZDBOnlyQuery(typeof(CusEntryHeader));
	}

	readonly NctsDepartureMovementHeader movementHeader;

	protected override ZQuery CreateRelationshipFilter()
	{
		var entryHeaderQuery = new ZDBOnlyQuery(typeof(CusEntryHeader));

		if (movementHeader.RelatedExportEntryHeaders.Count > 0)
		{
			var alreadyAddedPKs = movementHeader.RelatedExportEntryHeaders.Select(x => x.XX_Relation2ID);
			entryHeaderQuery.AddToFilter(CusEntryHeaderSchema.PK, SQLComparisonOperator.NotEqual, alreadyAddedPKs);
		}

		var relatedExportGenPivotQuery = new ZDBOnlySubQuery(typeof(GenPivot), GenPivotSchema.XX_Relation2ID, true);
		relatedExportGenPivotQuery.AddToFilter(GenPivotSchema.XX_RelationType, GenPivotTypeDecider.Types.NctsRelatedExportGenPivot);
		entryHeaderQuery.AddSubQuery(relatedExportGenPivotQuery, JoinCondition.And);

		return base.CreateRelationshipFilter().AddToFilter(entryHeaderQuery);
	}
}
