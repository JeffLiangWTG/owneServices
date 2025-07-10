using System;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ES.Business.Testing
{
	public static class CustomsFunctionalityTemporarySetterHelper
	{
		public static IDisposable SetESFUNCSImportMessageVersionUCC6(bool enabled = true) =>
			SetESFUNCSImportMessageVersionUCC6(ZDate.Today, enabled);

		public static IDisposable SetESFUNCSImportMessageVersionUCC6(ZDate date, bool enabled = true) =>
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.ImportMessageVersionUCC6, Core.Constants.CountryCodes.Spain,
				date, enabled);
	}
}
