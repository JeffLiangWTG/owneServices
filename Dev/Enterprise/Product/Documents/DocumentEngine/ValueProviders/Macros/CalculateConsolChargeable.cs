using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.DocumentEngine.Areas;
using Enterprise.Environment;
using Enterprise.Freight.Integration;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	internal class CalculateConsolChargeable : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<CalculateConsolChargeable({xxx})>",
				ResString.GetMultilingualString("ce467935-5f60-4fe9-91f5-ee8258d6d803", @"Returns a calculated chargeable for the Consol indicated by the {0}, Cached inside.", "UniqueConsignRef"),
				new List<(string example, object expectedResult)> { ("<CalculateConsolChargeable(<Consol.JK_UniqueConsignRef>)>", 100m) });
		}

		#region GetReplacement

		protected override object GetReplacementCore(string macro, Report report)
		{
			ZString consolUniqueRef = Regex.Match(macro).Groups[1].Value;

			if (consolUniqueRef.IsEmpty)
			{
				return ZDecimal.Zero;
			}

			if (consolChargeCalculationResult.TryGetValue(consolUniqueRef, out var chargeable))
			{
				return chargeable;
			}

			var weightUnit = Env.Registry.FreightWeightUnit;
			var volumeUnit = Env.Registry.FreightVolumeUnit;

			var uniqueRefs = new HashSet<ZString>() { consolUniqueRef };
			var rows = report.Analyser?.Areas?.OfType<SectionBodyArea>().FirstOrDefault()?.DataRowSource;
			if (rows != null)
			{
				for (var i = 0; i < rows.RowCount; i++)
				{
					ZString consolID = report.DataProvider.GetColumnValue(rows, i, "ReportData.ConsolID").ToString();
					if (!consolID.IsEmpty && !consolChargeCalculationResult.ContainsKey(consolID))
					{
						uniqueRefs.Add(consolID);
					}
				}
			}

			var chargeables = ConsolChargeableCalculationHelper.CalculateChargeables(uniqueRefs, weightUnit, volumeUnit);
			foreach (var item in chargeables)
			{
				consolChargeCalculationResult.Add(item.Key, item.Value);
			}

			return consolChargeCalculationResult[consolUniqueRef];
		}

		IConsolChargeableCalculationHelper ConsolChargeableCalculationHelper
		{
			get { return consolChargeableCalculationHelper ?? (consolChargeableCalculationHelper = ObjectFactory.Get<IConsolChargeableCalculationHelper>()); }
		}
		IConsolChargeableCalculationHelper consolChargeableCalculationHelper;

		#endregion

		protected override void ResetCore()
		{
			base.ResetCore();

			consolChargeCalculationResult.Clear();
		}

		readonly Dictionary<ZString, ZDecimal> consolChargeCalculationResult = new Dictionary<ZString, ZDecimal>();

		public override Regex Regex
		{
			get { return regex; }
		}
		static readonly Regex regex = new Regex(@"^<CalculateConsolChargeable\(\""*([^\""]+)\""*\)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
