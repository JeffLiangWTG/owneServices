using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.DataExportBatch
{
	/// <summary>
	/// Collection of GenExportBatchSequence objects which are exported batches of certain types of transaction headers and lines.
	/// Used in Data Export Batch Numbers tab of the relevant form.
	/// </summary>
	public sealed class DataExportBatchDependentCollection : DependentBusinessObjectCollection<GenExportBatchSequence, BusinessObject>
	{
		public DataExportBatchDependentCollection(IDataExportBatchSource source)
			: base(((BusinessObject)source).Factory)
		{
			Source = source;
		}
		readonly IDataExportBatchSource Source;

		public override void Load()
		{
			ZStringBuilder builder = new ZStringBuilder(Lookups.DataExportBatchSubTypeList.GetAllCodes());

			string sQL = string.Format(CultureInfo.InvariantCulture, @"
SELECT
	XB_PK
FROM
	dbo.GenExportBatchSequence
WHERE
	XB_ParentID = @ParendId
	AND (XB_Type in ('{0}'))	

UNION ALL

SELECT
	XB_PK
FROM dbo.AccTransactionHeader
	INNER JOIN dbo.AccTransactionLines ON AL_AH = AH_PK
	INNER JOIN dbo.GenExportBatchSequence ON XB_ParentID = AL_PK
WHERE AH_PK = @ParendId
	AND (XB_Type IN ('{0}'))", builder.ToStringWithDelimiterBetweenAppends("','"));

			ZSqlParameterCollection parameters = new ZSqlParameterCollection();
			parameters.Add("@ParendId", Source.PK, GenExportBatchSequenceSchema.XB_ParentID);
			DynamicBusinessObjectCollection dynamicCollection = new DynamicBusinessObjectCollection(Factory);
			dynamicCollection.Load(sQL, parameters);

			var batchPks = from DynamicBusinessObject batch in dynamicCollection select new ZGuid(batch[GenExportBatchSequence.Schema.PK]);
			AddRange(Factory.Load<GenExportBatchSequence>(new ZQuery(GenExportBatchSequenceSchema.PK, batchPks)));
		}

		GenExportBatchSequenceLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = new GenExportBatchSequenceLookups(null);
				}
				return lookups;
			}
		}
		GenExportBatchSequenceLookups lookups;

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}
	}
}
