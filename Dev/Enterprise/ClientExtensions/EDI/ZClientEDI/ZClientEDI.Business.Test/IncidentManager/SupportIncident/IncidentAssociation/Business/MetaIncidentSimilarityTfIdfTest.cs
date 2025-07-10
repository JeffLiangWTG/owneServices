using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation.Test
{
	class MetaIncidentSimilarityTfIdfTest : TestCaseWithFactory
	{
		public void Test_getAugmentedTermFrequency()
		{
			// Arrange
			var metaIncidentSimilarityTfIdf = new MetaIncidentSimilarityTfIdf()
			{
				TermFrequency = new[] { 1.7, 3.4, 2.9, -12.6, 4.2, 0.0 }
			};

			// Act
			var augmentedTermFrequency = metaIncidentSimilarityTfIdf.AugmentedTermFrequency.ToList();

			// Assert
			Assert(augmentedTermFrequency.Count == metaIncidentSimilarityTfIdf.TermFrequency.Count);
			Assert(Math.Abs(augmentedTermFrequency[0] - 1.7 / 4.2) < 1E-12);
			Assert(Math.Abs(augmentedTermFrequency[1] - 3.4 / 4.2) < 1E-12);
			Assert(Math.Abs(augmentedTermFrequency[2] - 2.9 / 4.2) < 1E-12);
			Assert(Math.Abs(augmentedTermFrequency[3] - -12.6 / 4.2) < 1E-12);
			Assert(Math.Abs(augmentedTermFrequency[4] - 1.0) < 1E-12);
			Assert(Math.Abs(augmentedTermFrequency[5]) < 1E-12);
		}

		public void Test_getTfIdfHex()
		{
			// Arrange
			var metaIncidentSimilarityTfIdf = new MetaIncidentSimilarityTfIdf()
			{
				TFIDF = new[] { 1.7, 3.4, 2.9, -12.6, 4.2, 0.0 }
			};

			// Act
			var tfIdfHex = metaIncidentSimilarityTfIdf.TFIDFHex;

			// Assert
			Assert(tfIdfHex == "0x505A33360681DFF660CA98DB0142B34369CD0367CF8080800303140000");
		}

		public void Test_getTfIdfHexNull()
		{
			// Arrange
			var metaIncidentSimilarityTfIdf = new MetaIncidentSimilarityTfIdf()
			{
				TFIDF = null
			};

			// Act
			var tfIdfHex = metaIncidentSimilarityTfIdf.TFIDFHex;

			// Assert
			Assert(tfIdfHex == null);
		}

		public void Test_compareTo()
		{
			// Arrange
			var metaIncidentSimilarityTfIdf1 = new MetaIncidentSimilarityTfIdf()
			{
				IncidentGuid = Guid.NewGuid(),
				PK = Guid.NewGuid()
			};

			var metaIncidentSimilarityTfIdf2 = new MetaIncidentSimilarityTfIdf()
			{
				IncidentGuid = Guid.NewGuid(),
				PK = Guid.NewGuid()
			};

			// Act
			// Assert
			if (String.Compare(metaIncidentSimilarityTfIdf1.IncidentGuid.ToString(), metaIncidentSimilarityTfIdf2.IncidentGuid.ToString(), StringComparison.Ordinal) < 0)
			{
				Assert(metaIncidentSimilarityTfIdf1 < metaIncidentSimilarityTfIdf2);
				Assert(metaIncidentSimilarityTfIdf2 > metaIncidentSimilarityTfIdf1);
			}
			else
			{
				Assert(metaIncidentSimilarityTfIdf1 > metaIncidentSimilarityTfIdf2);
				Assert(metaIncidentSimilarityTfIdf2 < metaIncidentSimilarityTfIdf1);
			}
		}

		public void Test_equals()
		{
			// Arrange
			var metaIncidentSimilarityTfIdf1 = new MetaIncidentSimilarityTfIdf()
			{
				PK = Guid.NewGuid(),
				IncidentGuid = Guid.NewGuid()
			};

			var metaIncidentSimilarityTfIdf2 = new MetaIncidentSimilarityTfIdf()
			{
				PK = Guid.NewGuid(),
				IncidentGuid = Guid.NewGuid()
			};

			var metaIncidentSimilarityTfIdf3 = new MetaIncidentSimilarityTfIdf()
			{
				PK = metaIncidentSimilarityTfIdf1.PK,
				IncidentGuid = metaIncidentSimilarityTfIdf1.IncidentGuid
			};

			var metaIncidentSimilarityTfIdf4 = new MetaIncidentSimilarityTfIdf()
			{
				PK = metaIncidentSimilarityTfIdf1.PK,
				IncidentGuid = Guid.NewGuid()
			};

			var metaIncidentSimilarityTfIdf5 = new MetaIncidentSimilarityTfIdf()
			{
				PK = Guid.NewGuid(),
				IncidentGuid = metaIncidentSimilarityTfIdf1.IncidentGuid
			};
			// Act
			// Assert
			Assert(!metaIncidentSimilarityTfIdf1.Equals(metaIncidentSimilarityTfIdf2));
			Assert(metaIncidentSimilarityTfIdf1.Equals(metaIncidentSimilarityTfIdf3));
			Assert(!metaIncidentSimilarityTfIdf1.Equals(metaIncidentSimilarityTfIdf4));
			Assert(!metaIncidentSimilarityTfIdf1.Equals(metaIncidentSimilarityTfIdf5));
		}
	}
}
