using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.IO;
using WTG.Numerics;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation
{
	public class MetaIncidentSimilarityTfIdf : IComparable<MetaIncidentSimilarityTfIdf>, IMetaIncidentSimilarityTfIdf
	{
		public Guid PK { get; set; }

		public Guid IncidentGuid { get; set; }
		public int Version { get; set; }

		MemoryOptimizedSparseVector<double> termFrequency;
		public IList<double> TermFrequency
		{
			get => termFrequency?.FastToList();
			set
			{
				if (value == null)
				{
					termFrequency = null;
					return;
				}

				termFrequency = new MemoryOptimizedSparseVector<double>(value.Count);

				for (var i = 0; i < value.Count; i++)
				{
					if (Math.Abs(value[i]) > 1E-12)
					{
						termFrequency[i] = value[i];
					}
				}
			}
		}
		public MemoryOptimizedSparseVector<double> TermFrequencySparseVector => termFrequency;

		public IList<double> AugmentedTermFrequency
		{
			get
			{
				if (termFrequency == null)
				{
					return null;
				}

				var max = TermFrequency.Max();

				var sparseVector = new MemoryOptimizedSparseVector<double>(TermFrequency.Count);

				for (var i = 0; i < TermFrequency.Count; i++)
				{
					if (Math.Abs(TermFrequency[i]) > 1E-12)
					{
						sparseVector[i] = TermFrequency[i] / max;
					}
				}

				return sparseVector;
			}
		}
		public string TermFrequencyHex => TermFrequency == null ? null : $"0x{BitConverter.ToString(Compressor.Compress(SimilarIncidentRepository.DoublesToBytes(TermFrequency).ToArray())).Replace("-", "")}";

		MemoryOptimizedSparseVector<double> tfIDF;
		public IList<double> TFIDF
		{
			get => tfIDF?.FastToList();
			set
			{
				if (value == null)
				{
					tfIDF = null;
					return;
				}

				tfIDF = new MemoryOptimizedSparseVector<double>(value.Count);

				for (var i = 0; i < value.Count; i++)
				{
					if (Math.Abs(value[i]) > 1E-12)
					{
						tfIDF[i] = value[i];
					}
				}
			}
		}
		public MemoryOptimizedSparseVector<double> TfIdfSparseVector => tfIDF;

		public string TFIDFHex => TFIDF == null ? null : $"0x{BitConverter.ToString(Compressor.Compress(SimilarIncidentRepository.DoublesToBytes(TFIDF).ToArray())).Replace("-", "")}";
		public string Status { get; set; }
		public DateTime IncidentLastModified { get; set; }

		public int CompareTo(MetaIncidentSimilarityTfIdf other)
		{
			return String.Compare(
				(IncidentGuid.ToString() + PK.ToString()),
				other.IncidentGuid.ToString() + other.PK.ToString(),
				StringComparison.Ordinal);
		}

		public static bool operator >(MetaIncidentSimilarityTfIdf operand1, MetaIncidentSimilarityTfIdf operand2)
		{
			return operand1.CompareTo(operand2) > 0;
		}

		public static bool operator <(MetaIncidentSimilarityTfIdf operand1, MetaIncidentSimilarityTfIdf operand2)
		{
			return operand1.CompareTo(operand2) < 0;
		}

		public static bool operator >=(MetaIncidentSimilarityTfIdf operand1, MetaIncidentSimilarityTfIdf operand2)
		{
			return operand1.CompareTo(operand2) >= 0;
		}
		public static bool operator <=(MetaIncidentSimilarityTfIdf operand1, MetaIncidentSimilarityTfIdf operand2)
		{
			return operand1.CompareTo(operand2) <= 0;
		}

		public static bool operator ==(MetaIncidentSimilarityTfIdf operand1, MetaIncidentSimilarityTfIdf operand2)
		{
			return operand1.CompareTo(operand2) == 0;
		}

		public static bool operator !=(MetaIncidentSimilarityTfIdf operand1, MetaIncidentSimilarityTfIdf operand2)
		{
			return operand1.CompareTo(operand2) != 0;
		}

		public override bool Equals(object other)
		{
			if ((other == null) || GetType() != other.GetType())
			{
				return false;
			}
			var idf = other as MetaIncidentSimilarityTfIdf;
			return CompareTo(idf) == 0;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Required for Equals override")]
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
