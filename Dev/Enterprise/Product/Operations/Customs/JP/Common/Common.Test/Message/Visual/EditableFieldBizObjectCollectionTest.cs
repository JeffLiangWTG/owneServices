using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(EditableFieldBizObjectCollection))]
	sealed class EditableFieldBizObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EditableFieldBizObjectCollection>
	{
		protected override EditableFieldBizObjectCollection GetCollectionToTest() => new EditableFieldBizObjectCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var sourceField = new EditableMessageField(new FieldDefinition());
			return new EditableFieldBizObject(JPProcedureCodeList.Codes.EDA, sourceField);
		}
	}
}
