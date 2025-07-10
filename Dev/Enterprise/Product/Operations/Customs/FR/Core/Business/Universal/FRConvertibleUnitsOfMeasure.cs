using System.Collections.Generic;
using System.Collections.Immutable;

namespace Enterprise.Customs.FR.Business
{
	public static class FRConvertibleUnitsOfMeasure
	{
		public static readonly ImmutableDictionary<string, decimal> WeightConversionDictionary = new Dictionary<string, decimal>()
			{
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.HundredKgDemiBrut, 100m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.TonneNettDeViandeImporteeDeductionFaiteDuPoidsDesAbats, 1000m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.CapaciteDeChargeEnTonnesMetriques, 1000m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Carats, 0.0002m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.KilogrammeDeDihydrostreptomycine, 1m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Hectokilogramme, 100m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.GrammeIsotopeFissile, 0.001m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Gramme, 0.001m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.KilogrammeDeChlorureDeCholine, 1m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.TonneDeChlorureDePotassium, 1000m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogramme, 1m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.KilogrammeDeMethylamine, 1m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.KilogrammeDazote, 1m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.KilogrammeDePeroxydeDhydrogene, 1m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.KilogrammeDhydroxydeDePotassium, 1m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.KilogrammeDoxydeDePotassium, 1m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.KilogrammeDePentaoxydeDeDiphosphore, 1m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.KilogrammeDeMatiereSecheA90Percent, 1m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.KilogrammeDhydroxydeDeSodium, 1m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.KilogrammeDuranium, 1m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.ThousandPaires, 1000m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Tonne, 1000m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Tonne3, 1000m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.KilogrammeN, 1m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.TonneN, 1000m }
			}.ToImmutableDictionary();

		public static readonly ImmutableDictionary<string, decimal> VolumeConversionDictionary = new Dictionary<string, decimal>()
			{
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Volumn.Decilitre, 0.1m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Volumn.HundredMetresCube, 100000m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Volumn.Hectolitre, 100m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Volumn.ThousandLitres, 1000m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Volumn.Litre, 1m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Volumn.MetreCube, 1000m }
			}.ToImmutableDictionary();

		public static readonly ImmutableDictionary<string, decimal> AlcoholConversionDictionary = new Dictionary<string, decimal>()
			{
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Alcohol.DecilitreDAlcoolPur, 0.1m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Alcohol.PercentageVol, 1m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Alcohol.LitreDalcoolPur, 1m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Alcohol.HectolitreDAlcoolPur, 100m }
			}.ToImmutableDictionary();

		public static readonly ImmutableDictionary<string, decimal> NumberConversionDictionary = new Dictionary<string, decimal>()
			{
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.ParTeteDeBetail, 1m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.HundredPieces, 100m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.ThousandPieces, 1000m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NombreDePieces, 1m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NombreDelements, 1m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NombreDePaires, 1m }
			}.ToImmutableDictionary();

		public static readonly ImmutableDictionary<string, decimal> LengthConversionDictionary = new Dictionary<string, decimal>()
			{
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Length.Hectometre, 100m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Length.Kilometre, 1000m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Length.Metre, 1m }
			}.ToImmutableDictionary();

		public static readonly ImmutableDictionary<string, decimal> SurfaceConversionDictionary = new Dictionary<string, decimal>()
			{
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Surface.MetreCarre, 1m }
			}.ToImmutableDictionary();

		public static readonly ImmutableDictionary<string, decimal> EnergyConversionDictionary = new Dictionary<string, decimal>()
			{
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Energy.ThousandKilowattsHeure, 1000m },
				{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Energy.Terajoule, 360000m }
			}.ToImmutableDictionary();
	}
}
