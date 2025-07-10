using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class SingleAccountingPeriodBuilder : FilterBuilder, ICustomBuilder
	{
		public SingleAccountingPeriodBuilder(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle)
			: base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
		{
		}

		protected override IReportDocumenter GetDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter((NoResString)"Single [Accounting ]Period", Res.GetString("FilterDocumentation|B2D7B4C4-5124-4386-92EC-E7CCEA053890", "Generates a single accounting period control. Filters data from the typed period."), supportedProperties);
		}

		public override bool CanBuild(string filterType)
		{
			return Regex.IsMatch(filterType, @"^Single (Accounting )?Period$", RegexOptions.IgnoreCase);
		}

		protected override FilterField GetFilterField()
		{
			return new SingleAccountingPeriodField(fBusinessObjectFactory);
		}

		#region ICustomBuilder

		void ICustomBuilder.DoCustomBuilding(StringTreeNode fieldTree, FilterField newField)
		{
			var required = newField.Validators.Find(x => x is RequiredFilterValidator) as RequiredFilterValidator;
			var validator = newField.Validators.Find(x => x is OnlyCurrentPeriodIfPayByWebServiceValidator) as OnlyCurrentPeriodIfPayByWebServiceValidator;
			if (required != null && validator != null && validator.IsPaymentWebServiceEnabled)
			{
				((SingleAccountingPeriodField)newField).SinglePeriod = validator.PeriodCalculator.GetPeriodFromDate(ZDateTime.Today);
			}
		}

		public void DoCustomBuildingInTaskBuild(StringTreeNode fieldTree, FilterField newField)
		{
		}

		#endregion
	}
}
