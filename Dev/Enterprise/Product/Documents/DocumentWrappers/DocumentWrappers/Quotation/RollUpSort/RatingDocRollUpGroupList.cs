using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.HelperClasses.DocRollUpSort;

namespace Enterprise.DocumentWrappers.Quotation.RollUpSort
{
	sealed class RatingDocRollUpGroupList : BaseDocRollUpGroupList<ZString, DocLineList>
	{
		protected override DocLineList GetNewLine(BusinessObjectFactory factory)
			=> new DocLineList();
	}
}
