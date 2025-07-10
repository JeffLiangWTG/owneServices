using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Core.Constants;

namespace Enterprise.ZArchitecture.Business
{
	public class UnitsConverter
	{
		public UnitsConverter(IEnumerable<ConversionFactor> conversionFactors = null)
		{
			if (conversionFactors != null)
			{
				conversionGraph = BuildGraph(conversionFactors);
			}
		}

		public IQuantity Convert(IQuantity quantity, string targetUnit, bool roundResult = false)
		{
			if (quantity == null)
			{
				throw new ArgumentNullException(nameof(quantity));
			}

			var sourceMeasureUnitType = ConversionFactor.GetMeasureUnitType(quantity.Unit);
			var targetMeasureUnitType = ConversionFactor.GetMeasureUnitType(targetUnit);

			if (sourceMeasureUnitType == MeasureUnitType.Unknown)
			{
				throw new UnitConversionException(quantity.Unit, targetUnit, true);
			}

			if (targetMeasureUnitType == MeasureUnitType.Unknown)
			{
				throw new UnitConversionException(quantity.Unit, targetUnit, false);
			}

			if (quantity.Unit == targetUnit)
			{
				return quantity;
			}

			if (sourceMeasureUnitType == targetMeasureUnitType)
			{
				return ConvertToSameMeasureUnitType(quantity, targetUnit, roundResult);
			}

			return ConvertToDifferentMeasureUnitType(quantity, targetUnit, roundResult);
		}

		static IQuantity ConvertToSameMeasureUnitType(IQuantity sourceQuantity, string targetUnit, bool roundResult)
		{
			switch (ConversionFactor.GetMeasureUnitType(sourceQuantity.Unit))
			{
				case MeasureUnitType.Weight:
					return new Quantity(Weight.Convert(sourceQuantity.Amount, sourceQuantity.Unit, targetUnit, roundResult), targetUnit);
				case MeasureUnitType.Volume:
					return new Quantity(Volume.Convert(sourceQuantity.Amount, sourceQuantity.Unit, targetUnit, roundResult), targetUnit);
				case MeasureUnitType.LoadingLength:
					return sourceQuantity;
				default:
					throw new UnitConversionException(sourceQuantity.Unit, targetUnit, true);
			}
		}

		static IEnumerable<Edge> BuildGraph(IEnumerable<ConversionFactor> factors)
		{
			var graph = new List<Edge>();

			foreach (var factor in factors)
			{
				graph.Add(new Edge(factor.NumeratorMeasureType, factor.DenominatorMeasureType, factor));
				graph.Add(new Edge(factor.DenominatorMeasureType, factor.NumeratorMeasureType, factor));
			}

			return graph;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Debug information, not indented for clients")]
		IQuantity ConvertToDifferentMeasureUnitType(IQuantity sourceQuantity, string targetUnit, bool roundResult)
		{
			if (conversionGraph == null)
			{
				return Quantity.Empty("No conversionGraph");
			}

			var sourceMeasureUnitType = ConversionFactor.GetMeasureUnitType(sourceQuantity.Unit);
			var targetMeasureUnitType = ConversionFactor.GetMeasureUnitType(targetUnit);

			var conversionPath = FindConversionPath(sourceMeasureUnitType, targetMeasureUnitType);
			if (!conversionPath.Any())
			{
				return Quantity.Empty("No path to conversion");
			}

			var result = sourceQuantity;

			foreach (var factor in conversionPath)
			{
				result = factor.Convert(result);
			}

			result = ConvertToSameMeasureUnitType(result, targetUnit, roundResult);
			return result;
		}

		IEnumerable<ConversionFactor> FindConversionPath(MeasureUnitType sourceType, MeasureUnitType targetType)
		{
			var minPath = new List<Edge>();
			FindConversionPath(sourceType, targetType, new List<Edge>(), minPath, new List<MeasureUnitType>());
			return minPath.Select(e => e.Factor).ToArray();
		}

		void FindConversionPath(MeasureUnitType sourceType, MeasureUnitType targetType, List<Edge> currentPath, List<Edge> minPath, List<MeasureUnitType> visitedNodes)
		{
			visitedNodes.Add(sourceType);

			foreach (var edge in conversionGraph.Where(e => e.From == sourceType && !visitedNodes.Contains(e.To)))
			{
				currentPath.Add(edge);

				if (edge.To == targetType)
				{
					if (currentPath.Count < minPath.Count || minPath.Count == 0)
					{
						minPath.Clear();
						minPath.AddRange(currentPath);
					}
				}
				else
				{
					FindConversionPath(edge.To, targetType, currentPath, minPath, visitedNodes);
				}

				currentPath.Remove(edge);
			}

			visitedNodes.Remove(sourceType);
		}

		readonly IEnumerable<Edge> conversionGraph;

		#region Types

		class Edge
		{
			public Edge(MeasureUnitType from, MeasureUnitType to, ConversionFactor factor)
			{
				From = from;
				To = to;
				Factor = factor;
			}

			public MeasureUnitType From { get; }
			public MeasureUnitType To { get; }
			public ConversionFactor Factor { get; }
		}
		#endregion
	}

	[Serializable]
	public class UnitConversionException : ZException
	{
		public UnitConversionException(string sourceUnit, string targetUnit, bool isSource)
			: base(GetExceptionMessage(sourceUnit, targetUnit, isSource))
		{
			SourceUnit = sourceUnit;
			TargetUnit = targetUnit;
			IsSource = isSource;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is an exception message")]
		static string GetExceptionMessage(string source, string target, bool isSource)
		{
			if (isSource)
			{
				return ZString.Format("Cannot convert {0} to {1}, The conversion logic for unit {0} is not implemented", source, target);
			}
			else
			{
				return ZString.Format("Cannot convert {0} to {1}, The conversion logic for unit {1} is not implemented", source, target);
			}
		}

#if NETFRAMEWORK
		protected UnitConversionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public string SourceUnit { get; }
		public string TargetUnit { get; }
		public bool IsSource { get; }

#if NET
		[Obsolete]
#endif
		override public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException(nameof(info));
			}

			info.AddValue("SourceUnit", SourceUnit);
			info.AddValue("TargetUnit", TargetUnit);
			info.AddValue("IsSource", IsSource);
			base.GetObjectData(info, context);
		}
	}
}
