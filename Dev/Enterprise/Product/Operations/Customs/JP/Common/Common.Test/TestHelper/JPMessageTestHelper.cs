using CargoWise.Customs.JP.MessageDefinitions;

namespace Enterprise.Customs.JP.Common.Testing;

public static class JPMessageTestHelper
{
	public static string CreateCommonResponseHeader(string businessCode = "", string responseCode = "", string receivedDateTime = "", string userCode = "", string userMailAddress = "", string subject = "", string messageReference = "", string inputReference = "")
	{
		var reservedArea1 = new string(' ', 3);
		var reservedArea2 = new string(' ', 17);
		var reservedArea3 = new string(' ', 40);
		var divisionNumber = new string(' ', 3);
		var lastMessage = new string(' ', 1);
		var messageType = new string(' ', 1);
		var reservedArea4 = new string(' ', 3);
		var splitReference = new string(' ', 100);
		var managementType = new string(' ', 1);
		var reservedArea5 = new string(' ', 28);
		var messageLength = new string(' ', 6);

		return reservedArea1 + businessCode.PadRight(5, ' ') + responseCode.PadRight(7, ' ') + receivedDateTime.PadRight(14, ' ') + userCode.PadRight(5, ' ') + reservedArea2 + userMailAddress.PadRight(64, ' ') + subject.PadRight(64, ' ') + reservedArea3
				+ messageReference.PadRight(26, ' ') + divisionNumber + lastMessage + messageType + reservedArea4 + inputReference.PadRight(10, ' ') + splitReference + managementType + reservedArea5 + messageLength;
	}

	public static EditableFieldBizObject CreateEditableFieldBizObject()
	{
		var fieldDefinition = CreateField(1, "Container Count", @"コンテナ本数", "COS", "an", 30);
		var sourceField = new EditableMessageField(fieldDefinition);
		var fieldBizObj = new EditableFieldBizObject(JPProcedureCodeList.Codes.EDA, sourceField);
		return fieldBizObj;
	}

	public static FieldDefinition CreateField(int no, string name, string jpName, string id, string type, int size)
	{
		return new FieldDefinition()
		{
			No = no,
			Name = name,
			JPName = jpName,
			ID = id,
			Type = type,
			Size = size,
		};
	}

	public static (FieldDefinition FieldDefinition, string Data) CreateField(int no, string name, string jpName, string id, string repeat, string repeat1, string data)
	{
		return (new FieldDefinition()
		{
			No = no,
			Name = name,
			JPName = jpName,
			ID = id,
			Repeat = repeat,
			Repeat1 = repeat1,
		}, data);
	}
}
