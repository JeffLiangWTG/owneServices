using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.US
{
	public class UpdateCD_TaxRateDescForCusUSClassification : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Update CD_TaxRateDesc For CusUSClassification";

		protected override void OfflinePostUpgradeTransform()
		{
			var script = @"
BEGIN
    CREATE TABLE #TaxRateMapping (
        OldTaxRateDesc VARCHAR(20) COLLATE SQL_Latin1_General_CP1_CI_AS,
        NewTaxRateDesc VARCHAR(20),
        NewTaxRate DECIMAL(18, 9)
    );

    INSERT INTO #TaxRateMapping (OldTaxRateDesc, NewTaxRateDesc, NewTaxRate)
    VALUES
        ('15.3389c/L', '15.33902c/L', 0.1533902),
        ('28.26619c/WL', '28.26641c/WL', 0.2826641),
        ('41.47469c/WL', '41.47501c/WL', 0.4147501),
        ('83.21355c/WL', '83.2142c/WL', 0.832142),
        ('89.8178c/WL', '89.8185c/WL', 0.898185),
        ('87.1761c/WL', '87.17678c/WL', 0.8717678),
        ('$3.566322/PFL', '$3.5663227/PFL', 3.5663227),
        ('$3.566322/L', '$3.5663227/L', 3.5663227);

    UPDATE dbo.CusUSClassification
    SET 
        CD_TaxRateDesc = mapping.NewTaxRateDesc,
        CD_TaxRate = mapping.NewTaxRate,
        CD_SystemLastEditTimeUtc = GETUTCDATE(),
        CD_SystemLastEditUser = '~BP'
    FROM 
        #TaxRateMapping AS mapping
    WHERE 
        dbo.CusUSClassification.CD_TaxRateDesc COLLATE SQL_Latin1_General_CP1_CI_AS = mapping.OldTaxRateDesc;

	DROP TABLE #TaxRateMapping;
END
";

			Db.Connection.ExecuteNonQuery(script);
		}

		public TransformationIndexProvider IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(CusUSClassificationSchema.Instance)
					.Key(CusUSClassificationSchema.Constants.CD_TaxRateDesc)
					.Include(CusUSClassificationSchema.Constants.CD_SystemLastEditTimeUtc, CusUSClassificationSchema.Constants.CD_SystemLastEditUser)
					.Where($"[CD_TaxRateDesc] IN ('15.3389c/L', '28.26619c/WL', '41.47469c/WL', '83.21355c/WL', '89.8178c/WL', '87.1761c/WL', '$3.566322/PFL', '$3.566322/L')")
					.GetInfo();

				return indexProvider;
			}
		}
	}
}
