using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Antlr4.Runtime;
using CargoWise.Common;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CA.Business
{
	public class CARateFormulaVisitor : RateFormulaBaseVisitor<bool>
	{
		public CARateFormulaVisitor(FormulaErrorListener errorListener)
		{
			this.errorListener = Argument.NotNull(errorListener, "errorListener");
			this.Results = new List<CARateResult>();
			this.currentRate = new CARateResult();
		}
		readonly FormulaErrorListener errorListener;
		CARateResult currentRate;
		public readonly List<CARateResult> Results;

		public override bool VisitExpression(RateFormulaParser.ExpressionContext context)
		{
			var firstExp = context.multiplyingExpression();
			var result = Visit(firstExp);

			if (!(currentRate.LastParserRuleContext is RateFormulaParser.MaxExpressionContext || currentRate.LastParserRuleContext is RateFormulaParser.MinExpressionContext))
			{
				Results.Add(currentRate);
			}

			foreach (var contExp in context.children.OfType<RateFormulaParser.ContExpressionContext>())
			{
				if (contExp.operand.Type == RateFormulaLexer.PLUS)
				{
					currentRate = new CARateResult();
					result = Visit(contExp.contExp);
					Results.Add(currentRate);
				}
			}

			return !errorListener.Errors.Any() && result;
		}

		public override bool VisitMaxExpression(RateFormulaParser.MaxExpressionContext context)
		{
			currentRate.LastParserRuleContext = null;
			var op1 = Visit(context.opleft);
			currentRate.LastParserRuleContext = context;
			var op2 = Visit(context.opright);

			return op1 & op2;
		}

		public override bool VisitMinExpression(RateFormulaParser.MinExpressionContext context)
		{
			currentRate.LastParserRuleContext = context;
			var op1 = Visit(context.opleft);
			currentRate.LastParserRuleContext = null;
			var op2 = Visit(context.opright);

			return op2;
		}

		#region VisitAtom

		public override bool VisitAtomExpression(RateFormulaParser.AtomExpressionContext context)
		{
			return Visit(context.exp);
		}

		public override bool VisitNumber(RateFormulaParser.NumberContext context)
		{
			var hasError = false;
			var value = context.numberBody.Text;
			var isParsed = decimal.TryParse(value, out var result);
			if (!isParsed)
			{
				errorListener.Report(FormulaVisitErrorType.SyntaxError, string.Format(CultureInfo.InvariantCulture, (NoResString)"Failed to convert \"{0}\" to Decimal", value));
				hasError = true;
			}

			if (currentRate.LastParserRuleContext is RateFormulaParser.MaxExpressionContext)
			{
				currentRate.DutyRateMin = result;
			}
			else if (currentRate.LastParserRuleContext is RateFormulaParser.MinExpressionContext)
			{
				currentRate.DutyRateMax = result;
			}
			else
			{
				currentRate.DutyRateRegular = result;
			}
			return hasError;
		}

		#endregion

		#region Getting Values from UniversalRateCalcData

		public override bool VisitReservedVFD(RateFormulaParser.ReservedVFDContext context)
		{
			var hasError = false;
			var vfd = context.VFD().GetText();
			if (!vfd.IsNullOrEmpty())
			{
				if (currentRate.LastParserRuleContext is RateFormulaParser.MaxExpressionContext)
				{
					currentRate.DutyRateMinUOM = RateTypes.Codes.AdValorem;
				}
				else if (currentRate.LastParserRuleContext is RateFormulaParser.MinExpressionContext)
				{
					currentRate.DutyRateMaxUOM = RateTypes.Codes.AdValorem;
				}
				else
				{
					currentRate.DutyRateRegularUOM = RateTypes.Codes.AdValorem;
				}
			}
			else
			{
				hasError = true;
			}
			return hasError;
		}

		public override bool VisitUomPlaceHolder(RateFormulaParser.UomPlaceHolderContext context)
		{
			var hasError = false;
			var uomCode = context.UOMCode().GetText().ToUpperInvariant().Trim('[', ']');
			if (!uomCode.IsNullOrEmpty())
			{
				if (currentRate.LastParserRuleContext is RateFormulaParser.MaxExpressionContext)
				{
					currentRate.DutyRateMinUOM = RateTypes.Codes.Specific;
				}
				else if (currentRate.LastParserRuleContext is RateFormulaParser.MinExpressionContext)
				{
					currentRate.DutyRateMaxUOM = RateTypes.Codes.Specific;
				}
				else
				{
					currentRate.DutyRateRegularUOM = RateTypes.Codes.Specific;
				}
			}
			else
			{
				hasError = true;
			}
			return hasError;
		}

		#endregion

		public class CARateResult
		{
			public decimal DutyRateMin { get; set; } = decimal.Zero;
			public string DutyRateMinUOM { get; set; } = string.Empty;
			public decimal DutyRateMax { get; set; } = decimal.Zero;
			public string DutyRateMaxUOM { get; set; } = string.Empty;
			public decimal DutyRateRegular { get; set; } = decimal.Zero;
			public string DutyRateRegularUOM { get; set; } = string.Empty;

			public ParserRuleContext LastParserRuleContext { get; set; }
		}
	}
}
