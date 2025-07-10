using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Client.MFI.DocWrappers
{
	public class MFIPrintStatementDocumentSupporter : PrintStatementDocumentSupporter
	{
		#region Constructor & Type Override

		protected MFIPrintStatementDocumentSupporter(PrintStatement printStatement)
			: base(printStatement)
		{
		}

		public new static PrintStatementDocumentSupporter New(PrintStatement printStatement)
		{
			return (printStatement == null) ? null : new MFIPrintStatementDocumentSupporter(printStatement);
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(New);
		}

		#endregion

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Enterprise.Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result = null;

			if (dataContext == Core.Constants.DataContext.Statement)
			{
				result = new DocumentWrapper[] { DocMFIPrintStatement.New(PrintStatement, Factory) };
			}
			else
			{
				result = base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
			}

			return result;
		}

		public override string GetMenuTemplateFilterValue(MenuTemplateFilterType filterType, IBODocDataProvider docWrapperForCurrentPivot)
		{
			string result = null;

			switch (filterType)
			{
				case MenuTemplateFilterType.PrintStandard:
					result = (MFIConstants.NZ.ClientSpecificCondition) ? ZBool.False.ToString() : ZBool.True.ToString();
					break;
				case MenuTemplateFilterType.PrintClientSpecific:
					result = MFIConstants.NZ.ClientSpecificCondition.ToString();
					break;
				default:
					result = ZBool.False.ToString();
					break;
			}

			return result;
		}
	}
}
