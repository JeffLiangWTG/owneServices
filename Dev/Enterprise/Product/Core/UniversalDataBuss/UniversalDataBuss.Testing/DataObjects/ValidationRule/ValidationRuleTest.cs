using System.Collections.Generic;
using System.IO;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing;
using Enterprise.UniversalDataBuss.Testing.DataObjectCreators;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Core.Testing
{
	[TestedType(typeof(ValidationRule))]
	class ValidationRuleTest : DataObjectTestCase<ValidationRule>
	{
		public void TestSerialiseValidationRuleCollection()
		{
			var dataContext = DataContextCreator.Create(DataContextType.ForwardingShipment, "S001", companyCode: "EDI");
			var shipment = UniversalShipmentCreator.Create(dataContext);
			var ruleCollection = new List<ValidationRule>
			{
				ValidationRuleCreator.Create("R001", 2, "test warning one", "WARNING"),
				ValidationRuleCreator.Create("R002", 3, "test error one", "ERROR")
			};
			shipment.SetValidationRuleCollection(() => ruleCollection);
			AssertNoExceptionThrown(() =>
			{
				var stream = TopLevelDataObjectConverter.SerializeToStream(shipment);
				using (var reader = new StreamReader(stream))
				{
					var content = reader.ReadToEnd();
					var result = TopLevelDataObjectConverter.Deserialize<Shipment>(content);
					AssertEquals(2, result.ValidationRuleCollection.Count);
				}
			});
		}
	}
}
