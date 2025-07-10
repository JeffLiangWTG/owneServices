using System;
using System.Collections.Generic;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.ResourceStrings.Cache.Testing
{
	public class ResourceStringContentTestTracker : BaseTestListener
	{
		public override void BeforeEachTest(DateTime startTime)
		{
			ResetLastFailures();
		}

		public static void ResetLastFailures()
		{
			if (!TestingState.IsRunningOnDAT)
			{
				lastFailures = null;
			}
			translationErrorFormatter = null;
		}

		public static void LogFailure(string language, string key, string message, string source, string translation)
		{
			if (!TestingState.IsRunningOnDAT)
			{
				if (lastFailures == null)
				{
					lastFailures = new HashSet<Tuple<string, string>>();
				}
				lastFailures.Add(new Tuple<string, string>(language, key));
			}

			if (translationErrorFormatter == null)
			{
				translationErrorFormatter = new TranslationErrorFormatter();
			}
			translationErrorFormatter.AddTranslationError(language, key, message, source, translation);
		}

		[ThreadSafe]
		static TranslationErrorFormatter translationErrorFormatter;

		[ThreadSafe]
		static HashSet<Tuple<string, string>> lastFailures;

		public static bool InLastFailures(string language, string key)
		{
			return lastFailures != null && lastFailures.Contains(new Tuple<string, string>(language, key));
		}

		public override void AfterEachTest(DateTime endTime)
		{
			base.AfterEachTest(endTime);
			if (translationErrorFormatter != null && translationErrorFormatter.HasError)
			{
				Assertion.HtmlFail(translationErrorFormatter.GetFormattedTranslationError());
			}
		}
	}
}
