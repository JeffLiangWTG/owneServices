using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.DataTransfer
{
	public sealed class JobNumberToPKMappingProvider : IService
	{
		public JobNumberToPKMappingProvider(BusinessObjectFactory factory, Func<BusinessObjectFactory, IEnumerable<ZString>, bool, IEnumerable<Job>> loadJobs)
		{
			jobNumberToPKMapping = new Dictionary<ZString, ZGuid>();
			jobNumbersNotMapped = new List<ZString>();
			this.factory = factory;
			this.loadJobs = loadJobs;
		}

		public void BatchLoadJobs(IncompleteTransactionHeader xmlTransaction)
		{
			jobNumberToPKMapping.Clear();
			jobNumbersNotMapped.Clear();

			var jobNumbers = GetJobNumbers(xmlTransaction);
			BuildJobNumberToPKMapping(jobNumbers, fetchFromCacheOnly: false);
			jobNumbersNotMapped.AddRange(jobNumbers.Where(x => !jobNumberToPKMapping.ContainsKey(x)));
		}

		public IReadOnlyDictionary<ZString, ZGuid> JobNumberToPKMapping
		{
			get
			{
				BuildJobNumberToPKMapping(jobNumbersNotMapped, fetchFromCacheOnly: true);
				return jobNumberToPKMapping;
			}
		}

		List<ZString> GetJobNumbers(IncompleteTransactionHeader xmlTransaction)
		{
			var jobNumbers = new List<ZString>();
			foreach (var xmlConsolCost in xmlTransaction.ConsolCosts?.Cast<ConsolCost>() ?? Enumerable.Empty<ConsolCost>())
			{
				var xmlConsolCostCharges = xmlConsolCost.ConsolCostCharges;
				var jobNumbersFromJobNumberField = xmlConsolCostCharges.Cast<ConsolCostCharge>().Select(x => x.JobNumber);
				var jobNumbersFromInternalJobNumberField = xmlConsolCostCharges.Cast<ConsolCostCharge>().Where(x => !x.InternalJobNumber.IsEmpty).Select(x => x.InternalJobNumber);
				var allJobNumbers = jobNumbersFromJobNumberField.Union(jobNumbersFromInternalJobNumberField);

				jobNumbers.AddRange(allJobNumbers);
			}

			if (xmlTransaction.JobRelatedLines != null)
			{
				jobNumbers.AddRange(xmlTransaction.JobRelatedLines.Cast<IncompleteTransactionLine>().Select(x => x.JobNumber));
			}

			jobNumbers = jobNumbers.Distinct().Where(s => !s.IsEmpty).ToList();
			return jobNumbers;
		}

		void BuildJobNumberToPKMapping(IEnumerable<ZString> jobNumbers, bool fetchFromCacheOnly)
		{
			var jobs = loadJobs(factory, jobNumbers, fetchFromCacheOnly);
			foreach (var job in jobs)
			{
				var jobNumber = job.JH_JobNum;
				jobNumberToPKMapping.Add(jobNumber, job.PK);
				jobNumbersNotMapped.Remove(jobNumber);
			}
		}

		readonly Dictionary<ZString, ZGuid> jobNumberToPKMapping;
		readonly BusinessObjectFactory factory;
		readonly Func<BusinessObjectFactory, IEnumerable<ZString>, bool, IEnumerable<Job>> loadJobs;
		readonly List<ZString> jobNumbersNotMapped;

#if DEBUG
		public Dictionary<ZString, ZGuid> jobNumberToPKMapping_ForTestOnly => jobNumberToPKMapping;
		public List<ZString> jobNumbersNotMapped_ForTestOnly => jobNumbersNotMapped;
#endif
	}
}
