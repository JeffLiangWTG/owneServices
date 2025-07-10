using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.PAVE.MENT.Business
{
	public class SQLColumnSpecificationLookups : ZLookups
	{
		public SQLColumnSpecificationLookups(SQLColumnSpecification parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList ColumnList
		{
			get { return new MENTColumns(); }
		}
	}
}
