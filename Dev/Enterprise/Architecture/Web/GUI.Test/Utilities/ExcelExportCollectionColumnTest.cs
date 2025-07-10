using CargoWise.Types;
using Enterprise.ZArchitecture.Excel;
using Enterprise.ZArchitecture.Excel.Testing;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.GUI.Testing
{
	public class ExcelExportCollectionColumnTest : ExcelExportColumnBaseTestCase
	{
		public override void TestGetValueForExport()
		{
			var collection = new PKDescriptionCollection();
			collection.Add(new PKDescription(ZGuid.NewZGuid(), "Item 1"));
			collection.Add(new PKDescription(ZGuid.NewZGuid(), "Item 2"));

			Dummy.CollectionWithPublicSetter = collection;

			AssertFormattedResult(Dummy, ArrayToTextConverter.ConvertToCommaSeparatedMultilineText("Item 1", "Item 2"));
		}

		public override void TestComment()
		{
			var column = GetNewExcelExportColumn();
			AssertEquals("Comment should be empty", "", column.GetComment(Dummy));
		}

		public override void TestColor()
		{
			var column = GetNewExcelExportColumn();
			AssertNull("Color should be empty", column.GetColor(Dummy));
		}

		#region Implementation

		protected override ZString ExpectedFormatPattern
		{
			get { return ZString.Empty; }
		}

		protected override ExcelExportColumnBase GetNewExcelExportColumn(IExcelExportCustomFunction baseColumn)
		{
			return new ExcelExportCollectionColumn("Header Text", "CollectionWithPublicSetter", PKDescription.Schema.Description);
		}

		#endregion
	}
}
