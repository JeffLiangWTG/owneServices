using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentEngine.MacroEvaluator
{
	public class FilterEvaluatorManager : EvaluatorManager
	{
		public FilterEvaluatorManager(BusinessObjectFactory factory, IDocumentSupportable parent, string macro = "")
			: base(factory, parent, macro)
		{ }

		public override ZString Evaluate()
		{
			ZString result = ZString.Empty;

			try
			{
				IBODocDataProvider boDocDataProvider = BODocDataProvider.Get(Parent as BusinessObject);
				Output = ZExpressionEvaluator.Evaluate(Macro, Parent, boDocDataProvider).ToString();
				result = ZExpressionEvaluator.LastValidDocumentFilterEnumExpressionError;
			}
			catch (Exception exception)
			{
				if (exception.IsCriticalException())
				{
					throw;
				}

				result = exception.InnerException != null ? exception.InnerException.Message : exception.Message;
			}

			return result;
		}

		#region Property overrides

		public override bool HasDataContext
		{
			get { return false; }
		}

		public override ResourceStringData Name
		{
			get { return Res.GetData("1ed6c53d-bd84-4df5-9efd-324cf28f5ee8", "Filter Evaluator"); }
		}

		public override ResourceStringData MacroDescription
		{
			get { return Res.GetData("166eaa2e-066b-41a2-aae5-09fec975fb2e", "Filter"); }
		}

		#endregion
	}
}
