using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.FR;

public class RemoveLegacyEntrySnapshot : DataTransformation, ITransformationIndexProvider
{
	public override string UserDescription => "Remove Legacy Entry Snapshot";

	public TransformationIndexProvider IndexProvider
	{
		get
		{
			var indexProvider = new TransformationIndexProvider(this);
			if (DbObjectCreator.TableExists(Db.Connection, CusEntrySnapshotSchema.Constants.TableName))
			{
				indexProvider.New(CusEntryHeaderSchema.Instance)
					.Key(CusEntryHeaderSchema.Constants.CH_DataModel)
					.Where($"[{CusEntryHeaderSchema.Constants.CH_DataModel}]='FR'")
					.GetInfo();
				indexProvider.New(CusEntrySnapshotSchema.Instance)
					.Key(CusEntrySnapshotSchema.Constants.CES_MessageType)
					.Where($"[{CusEntrySnapshotSchema.Constants.CES_MessageType}] IN ('DG', 'DI')")
					.GetInfo();
			}

			return indexProvider;
		}
	}

	protected override void OfflinePostUpgradeTransform()
	{
		if (DbObjectCreator.TableExists(Db.Connection, Db.Connection.CurrentDatabase, CusEntrySnapshotSchema.Constants.TableName, "dbo"))
		{
			Db.Connection.ExecuteNonQuery(@"DELETE snap FROM dbo.CusEntrySnapshot snap
JOIN dbo.CusEntryHeader entry ON snap.CES_CH_EntryHeader = entry.CH_PK
WHERE entry.CH_DataModel = 'FR' AND snap.CES_MessageType IN ('DG', 'DI');");
		}
	}
}
