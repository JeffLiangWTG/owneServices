using System;
using System.Collections.Generic;
using WTG.Numerics;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation
{
	public interface IMetaIncidentSimilarityTfIdf
	{
		IList<double> AugmentedTermFrequency { get; }
		Guid IncidentGuid { get; set; }
		DateTime IncidentLastModified { get; set; }
		Guid PK { get; set; }
		string Status { get; set; }
		IList<double> TermFrequency { get; set; }
		MemoryOptimizedSparseVector<double> TermFrequencySparseVector { get; }
		string TermFrequencyHex { get; }
		IList<double> TFIDF { get; set; }
		MemoryOptimizedSparseVector<double> TfIdfSparseVector { get; }
		string TFIDFHex { get; }
		int Version { get; set; }

		int CompareTo(MetaIncidentSimilarityTfIdf other);
		bool Equals(object other);
		int GetHashCode();
	}
}