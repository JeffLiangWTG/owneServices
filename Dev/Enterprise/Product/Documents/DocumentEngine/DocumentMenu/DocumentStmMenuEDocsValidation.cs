using Enterprise.DocumentEngine.DataProviders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine
{
	public class DocumentStmMenuEDocsValidation : StmMenuEDocsValidation
	{
		public DocumentStmMenuEDocsValidation(DocumentStmMenuEDocs parent) : base(parent)
		{
		}

		protected override void CheckSX_Filter()
		{
			base.CheckSX_Filter();

			if (!Parent.SX_Filter.IsEmpty)
			{
				string message;
				if (!ZExpressionEvaluator.IsValidOtherDocumentOrEDocsFilterName(Parent.SX_Filter, out message))
				{
					Parent.SX_FilterInfo.AddError(message);
				}
			}
		}
	}
}
