namespace Enterprise.Accounting.ElectronicMessaging.Mexico
{
	static class MexicoConstants
	{
		public static class DataContext
		{
			public const string MexicoRemainingStamps = "EINV_MX_RemainingStamps";
		}

		#region TaxCodes

		public sealed class TaxCodes
		{
			public const string IVA4 = "IVA4";
			public const string IVAREC = "IVAREC";
		}

		#endregion

		#region PercentageOfTax

		[WTG.StaticAnalysis.Annotation.CodeAlive("Already used in CFDiConceptoImpuestosBuilder class. But sealed class with const fields are compiled as inline ones.")]
		public sealed class PercentageTax
		{
			public const decimal Perc016 = 0.16m;
			public const decimal Perc025 = 0.25m;
			public const decimal Perc0060 = 0.060m;
		}

		#endregion
	}
}
