using System;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;

namespace Enterprise.DocumentEngine.DeliveryMethods.PrinterLanguages
{
	/// <summary>
	/// Abstract printer language class.
	/// </summary>
	public abstract class Base
	{
		public ZBlob PaperFeedSequence(int twentyFourthsOfAnInchToFeed)
		{
			if (twentyFourthsOfAnInchToFeed == 0)
			{
				return ZBlob.Empty;
			}
			else
			{
				return GetNonZeroPaperFeedSequence(twentyFourthsOfAnInchToFeed);
			}
		}

		protected internal abstract ZBlob GetNonZeroPaperFeedSequence(int twentyFourthsOfAnInchToFeed);

		/// <summary>
		/// Factory method to create a concrete subclass for a specific language.
		/// </summary>
		/// <param name="languageCode">Language code from PrintLanguage CodeDescriptionPairList</param>
		/// <returns>Concrete subclass that implements the given language</returns>
		public static Base Create(string languageCode)
		{
			switch (languageCode)
			{
				case StmPrintQueue.PrintLanguageTypes.Epson:
					return new EpsonEscP();

				case StmPrintQueue.PrintLanguageTypes.IBM:
					return new IbmProPrinter();

				case StmPrintQueue.PrintLanguageTypes.OKI:
					return new OkiMicroline();

				case StmPrintQueue.PrintLanguageTypes.None:
				case "":
				case null:
					return new None();

				default:
					throw new ArgumentOutOfRangeException("LanguageCode", languageCode, "Unsupported printer language");
			}
		}
	}
}
