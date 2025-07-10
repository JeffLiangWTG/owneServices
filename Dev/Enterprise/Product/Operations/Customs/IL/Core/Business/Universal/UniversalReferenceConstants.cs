using System.Collections.Immutable;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.IL.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	public static class UniversalReferenceConstants
	{
		[CodeAlive("This class will be used in future workitems")]
		public static class RefCusRateCodes
		{
			public const string CustomsDutyOnIndustrialProducts = "A00";
			public const string DefunctUseA00InsteadCustomsDutyOnAgriculturalProducts = "A10";
			public const string AdditionalDutyCountervailingSafeguardChargeVariableCharge = "A20";
			public const string DefinitiveAntiDumpingDuty = "A30";
			public const string ProvisionalAntiDumpingDuty = "A35";
			public const string DefinitiveCountervailingDuty = "A40";
			public const string ProvisionalCountervailingDuty = "A45";
			public const string Vat = "B00";
			public const string VatOnAdditionalDutiesForNorthernIreland = "B05";
			public const string CompensatoryInterestVat = "B10";

			public const string AgriculturalComponent = "EA";
			public const string AdditionalDutyOnSugarContents = "ADSZ";
			public const string AdditionalDutyOnFlourContents = "ADFM";
			public const string ExportRefund = "350";
			public const string ClimateChangeLevy = "990";

			public static readonly ImmutableArray<string> AdditionalDuties = new string[] { AgriculturalComponent, AdditionalDutyOnSugarContents, AdditionalDutyOnFlourContents }.ToImmutableArray();
		}
	}
}
