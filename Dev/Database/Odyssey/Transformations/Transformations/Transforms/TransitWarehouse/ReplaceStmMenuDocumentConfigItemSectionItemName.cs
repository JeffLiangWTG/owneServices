using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse;

public class ReplaceStmMenuDocumentConfigItemSectionItemName : DataTransformation, ITransformationIndexProvider
{
	public override string UserDescription => "Update StmMenuDocumentConfigItem.S4_SectionItemName's value from Packages (Start Of Package) to Packages (Start of Package).";

	TransformationIndexProvider ITransformationIndexProvider.IndexProvider
	{
		get
		{
			var indexProvider = new TransformationIndexProvider(this);
			indexProvider.New(StmMenuDocumentConfigItemSchema.Instance)
				.Key(StmMenuDocumentConfigItemSchema.Constants.S4_SectionItemName)
				.Include(StmMenuDocumentConfigItemSchema.Constants.S4_SystemLastEditTimeUtc, StmMenuDocumentConfigItemSchema.Constants.S4_SystemLastEditUser)
				.GetInfo();
			return indexProvider;
		}
	}

	protected override void OfflinePostUpgradeTransform()
	{
		var sql = @"
UPDATE dbo.StmMenuDocumentConfigItem
SET
	S4_SectionItemName = 'Packages (Start of Package)',
	S4_SystemLastEditTimeUtc = GetUtcDate(),
	S4_SystemLastEditUser = '~BP'
WHERE
	S4_SectionItemName = 'Packages (Start Of Package)'";
		Db.Connection.ExecuteNonQuery(sql);
	}
}
