using Enterprise.Customs.Business;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.NCTS.Business
{
	sealed class AddOrCvdDepartureRateCalculationVisitor : RateCalculationVisitor
	{
		public class Creator : IRateCalculationVisitorCreator
		{
			IRateCalculationVisitor IRateCalculationVisitorCreator.NewVisitor(IUniversalRateCalcData rateCalcData, FormulaErrorListener errorListener) =>
				new AddOrCvdDepartureRateCalculationVisitor(rateCalcData, errorListener);
		}

		public new static IDutyCalculationResult CalculateParticipatingFees(IUniversalRateCalcData rateCalcData, string formulaString) =>
			CalculateDuties(rateCalcData, formulaString, new Creator());

		AddOrCvdDepartureRateCalculationVisitor(IUniversalRateCalcData rateCalcData, FormulaErrorListener errorListener)
			: base(rateCalcData, errorListener)
		{
		}

		protected override RateFormulaResult VisitMinMaxCondition(bool condition, RateFormulaResult opTrue, RateFormulaResult opFalse) =>
			condition ? opTrue : opFalse;

		protected override bool EvaluateHasExpression(RateFormulaParser.HasExpressionContext context) =>
			HasExpressionWithTypeCertificate(context) || base.EvaluateHasExpression(context);

		public override RateFormulaResult VisitIfExpression(RateFormulaParser.IfExpressionContext context)
		{
			if (ConditionIsHasCertExpression(context.boolExpression())
				&& (BranchIsIfExpressionWithHasCertCondition(context.trueExp) || BranchIsIfExpressionWithHasCertCondition(context.falseExp)))
			{
				var trueResult = Visit(context.trueExp);
				var falseResult = Visit(context.falseExp);

				return VisitMinMaxCondition(trueResult.ResultAmount >= falseResult.ResultAmount, trueResult, falseResult);
			}
			else
			{
				return base.VisitIfExpression(context);
			}

			bool ConditionIsHasCertExpression(RateFormulaParser.BoolExpressionContext boolExpContext) =>
				boolExpContext?.ChildCount > 0 && boolExpContext.GetChild(0) is RateFormulaParser.BoolAndExpressionContext boolAndExpContext
					&& boolAndExpContext.ChildCount > 0 && boolAndExpContext.GetChild(0) is RateFormulaParser.BoolHASContext hasContext
					&& HasExpressionWithTypeCertificate(hasContext.hasExpression());

			bool BranchIsIfExpressionWithHasCertCondition(RateFormulaParser.ExpressionContext branchContext) =>
				branchContext?.ChildCount > 0 && branchContext.GetChild(0) is RateFormulaParser.MultiplyingExpressionContext multExpContext
				&& multExpContext.ChildCount > 0 && multExpContext.GetChild(0) is RateFormulaParser.AtomIFContext atomIfContext
				&& atomIfContext.ChildCount > 0 && atomIfContext.GetChild(0) is RateFormulaParser.IfExpressionContext ifExpContext
				&& ConditionIsHasCertExpression(ifExpContext.boolExpression());
		}

		bool HasExpressionWithTypeCertificate(RateFormulaParser.HasExpressionContext context) =>
			context.opleft.Text.Trim('"') == HasTypeCertificate;

		const string HasTypeCertificate = "CERT";
	}
}
