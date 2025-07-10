using System.Linq;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformation.DataModification.AddInfoTransformationBase
{
	public abstract class CopyJobComInvoiceLineAddInfoToRealColumn : CopyAddInfoToRealColumn
	{
		public override SchemaStringColumn SourceAddInfoColumn => JobComInvoiceLineSchema.JI_AddInfo;
		public override string AdditionalSourceTableJoin
		{
			get
			{
				var countryCodesJoin = string.Join(", ", CountryCodes.Select(x => $"'{x}'"));
				return $@"INNER JOIN
(
	SELECT JZ_PK, JZ_ClusterKey
	FROM dbo.JobComInvoiceHeader
	INNER JOIN dbo.GlbBranch ON JZ_GB = GB_PK
	INNER JOIN dbo.GlbCompany ON GB_GC = GC_PK AND GC_RN_NKCountryCode IN ({countryCodesJoin})
	WHERE JZ_JE IS NULL AND JZ_GroupInvoice = 0
	UNION
	SELECT JZ_PK, JZ_ClusterKey
	FROM dbo.JobComInvoiceHeader
	INNER JOIN dbo.JobDeclaration ON JE_ClusterKey = JZ_ClusterKey AND JE_PK = JZ_JE
	INNER JOIN dbo.GlbCompany ON JE_GC = GC_PK AND GC_RN_NKCountryCode IN ({countryCodesJoin})
	WHERE JZ_GroupInvoice = 0
) InvoiceHeader ON JI_ClusterKey = JZ_ClusterKey AND JI_JZ = JZ_PK";
			}
		}

		public override string SourceTableHint => "WITH (FORCESEEK)";

		protected abstract string[] CountryCodes { get; }
	}
}
