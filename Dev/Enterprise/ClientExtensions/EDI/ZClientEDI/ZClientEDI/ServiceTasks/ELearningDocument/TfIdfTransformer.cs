using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation;
using Enterprise.Integration;
using WTG.MachineLearning.NLP;
using WTG.MachineLearning.NLP.Preprocessing;
using WTG.Numerics.LinearAlgebra;

namespace Enterprise.Client.EDI.ServiceTasks.ELearningDocument
{
	public interface ITfIdfTransformer
	{
		(IEnumerable<double> tfidf, IEnumerable<int> termFrequency) Transform(string data, ILogger logger);
	}

	public class TfIdfTransformer : ITfIdfTransformer
	{
		readonly IncidentTokenizer incidentTokenizer;
		readonly TfIdfDocumentSimilarity tfIdfDocumentSimilarity;

		public TfIdfTransformer()
		{
			ISimilarIncidentRepository similarIncidentRepository = new SimilarIncidentRepository();
			var tokens = similarIncidentRepository.GetTokenPairs().ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString(CultureInfo.InvariantCulture));
			var terms = tokens.Values.Distinct().OrderBy(s => s).ToList();
			var dummyIDF = terms.ToDictionary(term => term, term => 1.0);
			ILinearAlgebra linearAlgebra = new BasicLinearAlgebra();
			tfIdfDocumentSimilarity = new TfIdfDocumentSimilarity(dummyIDF, linearAlgebra);
			incidentTokenizer = new IncidentTokenizer(similarIncidentRepository.GetTokenPairs().ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString(CultureInfo.InvariantCulture)), new PorterStemmer());
		}

		public (IEnumerable<double> tfidf, IEnumerable<int> termFrequency) Transform(string data, ILogger logger)
		{
			logger?.Log(LogType.Information, $"start performing transformation to bag of words for data length= {data.Length}");
			var tokenized = incidentTokenizer.TokenizeIncident(data).ToList();
			logger?.Log(LogType.Information, $"tokenized= {tokenized?.ToList()}");
			var rawTermFrequency = tfIdfDocumentSimilarity.DocumentToBagOfWords(tokenized).ToList();
			tfIdfDocumentSimilarity.GetAugmentedTermFrequency(rawTermFrequency.ToList()).ToList();
			return (tfIdfDocumentSimilarity.ComputeTfIdf(tokenized), rawTermFrequency);
		}
	}
}
