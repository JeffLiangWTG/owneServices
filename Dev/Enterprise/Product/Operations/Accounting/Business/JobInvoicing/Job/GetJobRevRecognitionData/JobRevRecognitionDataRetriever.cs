using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public interface IJobRevRecognitionDataRetriever
	{
		ZGuid JobPk { get; }
		IReadOnlyCollection<(ZString recognitionType, ZDateTime recognizedDate)> GetRevenueRecognitionData();
	}

	public class JobRevRecognitionDataRetriever : IJobRevRecognitionDataRetriever
	{
		public JobRevRecognitionDataRetriever(ZGuid jobPk)
		{
			this.jobPk = jobPk;
		}

		readonly ZGuid jobPk;

		ZGuid IJobRevRecognitionDataRetriever.JobPk => jobPk;

		IReadOnlyCollection<(ZString recognitionType, ZDateTime recognizedDate)> IJobRevRecognitionDataRetriever.GetRevenueRecognitionData()
		{
			var sql = "SELECT RecognitionType, RecognizedDate FROM dbo.GetRevRecognitionTypeAndDateByJob(@JobPK)";

			var sqlParams = new ZSqlParameterCollection { ZSqlParameter.New("@JobPK", ((IJobRevRecognitionDataRetriever)this).JobPk, JobHeaderSchema.PK) };

			var revRecognitionDataCollection = new DynamicBusinessObjectCollection(new ReadOnlyBusinessObjectFactory());
			revRecognitionDataCollection.Load(sql, sqlParams);

			return revRecognitionDataCollection.Select(x => ((ZString)x["RecognitionType"], (ZDateTime)x["RecognizedDate"])).ToList();
		}
	}
}
