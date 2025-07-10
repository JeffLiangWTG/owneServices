using System.Collections.Generic;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Module.Testing
{
	public class EntryHeaderFilterUserControlTest : EU.Module.Testing.EntryHeaderFilterUserControlTest
	{
		protected override List<string> FilteredGridColumns
		{
			get
			{
				var list = new List<string>()
				{
					CusEntryHeader.Schema.CustomsDocStatus,
					CusEntryHeader.Schema.CustomsDocStatusDesc,
				};
				list.AddRange(base.FilteredGridColumns);
				return list;
			}
		}

		protected override List<string> ColumnNamesInSortOrder
		{
			get
			{
				var list = base.ColumnNamesInSortOrder;
				list.AddRange(new List<string>()
				{
					CusEntryHeader.Schema.CustomsDocStatus,
					CusEntryHeader.Schema.CustomsDocStatusDesc,
				});
				return list;
			}
		}

		protected override Customs.Module.EntryHeaderFilterUserControl GetNewEntryHeaderFilterUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cusEntryHeaders = new EU.Business.Declaration.CusEntryHeaderCollection<CusEntryHeader>(declaration, Factory);
			var filterBusinessObject = new EntryHeaderFilterBusinessObject();
			return new EntryHeaderFilterUserControl(cusEntryHeaders, filterBusinessObject);
		}
	}
}
