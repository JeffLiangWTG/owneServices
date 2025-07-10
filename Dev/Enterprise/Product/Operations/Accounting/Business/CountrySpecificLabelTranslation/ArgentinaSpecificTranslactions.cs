using CargoWise.Types;

namespace Enterprise.Accounting.Business
{
	internal static class ArgentinaSpecificTranslactions
	{
		internal static ZString? TranslateLabel(LabelsEnum? label, params object[] parameters)
		{
			var language = Res.CurrentLanguage;

			if (language == Core.SharedConstants.Languages.Spanish || language == Core.SharedConstants.Languages.SpanishLatin)
			{
				return SpanishLabelSpecificTranslations(label, parameters);
			}

			return null;
		}

		#region SuppressResourceStringsCheckRegion

		static ZString? SpanishLabelSpecificTranslations(LabelsEnum? label, params object[] parameters)
		{
			switch (label)
			{
				case LabelsEnum.RecipientConsumptionTaxRegimeHeading:
					return ZString.Format("CONDICIÓN FRENTE AL {0}", parameters);
				default:
					return null;
			}
		}

		#endregion
	}
}
