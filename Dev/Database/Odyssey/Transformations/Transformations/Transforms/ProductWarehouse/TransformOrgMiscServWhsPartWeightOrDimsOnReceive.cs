using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse
{
	public class TransformOrgMiscServWhsPartWeightOrDimsOnReceive : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Transform WhsPreventReceiveOfPartsWithNoWeightOrDims to WhsCheckPartWeightOrDimsOnReceive.";

		#region ITransformationIndexProvider.IndexProvider

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				if (DbObjectCreator.ColumnExists(Db.Connection, OrgMiscServSchema.Constants.TableName, "OM_WhsPreventReceiveOfPartsWithNoWeightOrDims"))
				{
					indexProvider.New(OrgMiscServSchema.Instance).Key("OM_WhsPreventReceiveOfPartsWithNoWeightOrDims").Where("[OM_WhsPreventReceiveOfPartsWithNoWeightOrDims]=(1)").GetInfo();
				}
				return indexProvider;
			}
		}

		#endregion

		protected override void OfflinePreUpgradeTransform()
		{
			base.OfflinePreUpgradeTransform();

			if (DbObjectCreator.ColumnExists(Db.Connection, OrgMiscServSchema.Constants.TableName, "OM_WhsPreventReceiveOfPartsWithNoWeightOrDims"))
			{
				DbObjectCreator.CreateColumnIfNotExists(Db.Connection, OrgMiscServSchema.Constants.TableName, OrgMiscServSchema.Constants.OM_WhsCheckPartWeightOrDimsOnReceive, "CHAR(3)", "'NON'");

				var updateOrgMiscServWhsPartWeightOrDimsOnReceive = @"
UPDATE dbo.[OrgMiscServ]
SET OM_WhsCheckPartWeightOrDimsOnReceive = 'ALL',
OM_SystemLastEditUser = '~BP',
OM_SystemLastEditTimeUtc = GetUtcDate() 
WHERE OM_WhsPreventReceiveOfPartsWithNoWeightOrDims = 1;
";
				Db.Connection.ExecuteNonQuery(updateOrgMiscServWhsPartWeightOrDimsOnReceive);
			}
		}
	}
}
