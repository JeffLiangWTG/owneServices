using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(EditableFieldBizObject))]
	sealed class EditableFieldBizObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var fieldDefinition = JPMessageTestHelper.CreateField(1, "Result Code", "処理結果コード", "AAA", "an", 3);
			var sourceField = new EditableMessageField(fieldDefinition);
			sourceField.PrefixForDisplay = "2-1";

			return new EditableFieldBizObject(JPProcedureCodeList.Codes.EDA, sourceField);
		}

		public void TestProperties()
		{
			var fieldBizObj = (EditableFieldBizObject)GetNewBusinessObject();
			CombineAssertions(() =>
			{
				AssertEquals($"2-1 Result Code", fieldBizObj.Name);
				AssertEquals($"2-1 処理結果コード", fieldBizObj.JPName);
				AssertEquals(3, fieldBizObj.OverrideValueInfo.MaxLength);

				Assert("Default to false", !fieldBizObj.OverrideValueInfo.ReadOnly);

				((EditableMessageField)fieldBizObj.SourceField).IsReadOnly = true;
				Assert(fieldBizObj.OverrideValueInfo.ReadOnly);
			});
		}
	}
}
