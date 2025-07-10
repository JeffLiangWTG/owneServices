using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation
{
	public class IncidentSimilarityTfIdf : AutoIncidentSimilarityTfIdf
	{
		public static class Status
		{
			public const string Uninitialized = "UNZ";
			public const string TermFrequencyComputed = "TCD"; // Term frequency is "raw"
			public const string TfIdfComputed = "ICD"; // Term frequency is "augmented"
			public const string MatrixComputed = "MCD";
		}

		public SupportIncident SupportIncident => Factory.Load<SupportIncident>(ISV_IM_Incident);

		public IEnumerable<double> RawTermFrequency
		{
			get
			{
				if (ISV_TF.IsEmpty)
				{
					return null;
				}

				return SimilarIncidentRepository.BytesToDoubles((byte[])ISV_TF);
			}
			set
			{
				if (value == null || !value.Any())
				{
					ISV_TF = ZBlob.Empty;
				}
				else
				{
					ISV_TF = SimilarIncidentRepository.DoublesToBytes(value.ToList()).ToArray();
				}
			}
		}

		public IEnumerable<double> AugmentedTermFrequency
		{
			get
			{
				if (RawTermFrequency == null)
				{
					return null;
				}
				var raw_tf = RawTermFrequency.ToList();
				var max = raw_tf.Max();
				return raw_tf.Select(d => d / max);
			}
		}

		public IEnumerable<double> TFIDF
		{
			get
			{
				if (ISV_TFIDF.IsEmpty)
				{
					return null;
				}

				return SimilarIncidentRepository.BytesToDoubles((byte[])ISV_TFIDF);
			}
			set
			{
				if (value == null || !value.Any())
				{
					ISV_TFIDF = ZBlob.Empty;
				}
				else
				{
					ISV_TFIDF = SimilarIncidentRepository.DoublesToBytes(value.ToList()).ToArray();
				}
			}
		}

		public IncidentSimilarityTfIdf(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
