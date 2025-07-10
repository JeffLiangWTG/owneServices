using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine
{
	public class FilterEvaluator
	{
		internal FilterEvaluator(params IBODocDataProvider[] boDataSources)
		{
			var uniqueDataSources = new List<IBODocDataProvider>();
			foreach (var boDataSource in boDataSources)
			{
				if (boDataSource != null && !uniqueDataSources.Contains(boDataSource))
				{
					uniqueDataSources.Add(boDataSource);
				}
			}

			if (uniqueDataSources.Count > 0)
			{
				filterTranslator = new TranslateMacroForDocumentFilter(new DataProviderList(uniqueDataSources.ToArray()));
			}
		}
		readonly TranslateMacroForDocumentFilter filterTranslator;

		internal string ReplaceMacros(ZString filterString)
		{
			if (filterTranslator == null)
			{
				return filterString;
			}
			else
			{
				return filterTranslator.ReplaceMacros(filterString);
			}
		}

		internal bool MatchesFilter(ZString filterString)
		{
			ZString translatedValue;
			return MatchesFilter(filterString, out translatedValue);
		}

		internal bool MatchesFilter(ZString filterString, out ZString translatedValue)
		{
			if (filterString.IsEmpty)
			{
				translatedValue = null;
				return true;
			}
			else if (filterTranslator == null)
			{
				translatedValue = null;
				return false;
			}

			translatedValue = ReplaceMacros(filterString);
			try
			{
				return ExpressionEvaluator.Evaluate(translatedValue, RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.Value);
			}
			catch (DocumentEngineException)
			{
				return false;
			}
		}
	}
}
