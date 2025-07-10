using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class CustomPropertyExtensionsTest : TestCaseWithFactory
	{
		public void TestIsMatchingLegacyIdentifier()
		{
			var customPropertyName = "Rt_+.field @_1";
			var customPropertyIdentifier = CustomPropertyHelper.GeneratePropertyIdentifier(customPropertyName, typeof(ZString));

			var dummyCustomFactory = ObjectFactory.Get<IDummyWithCustomFieldsFactory>();
			var dummy = dummyCustomFactory.GetDummyWithCustomFields(Factory, out var addCustomField, out var setCustomField, out var getCustomField, out var getCustomFieldGuiBinding);
			addCustomField((customPropertyName, "STR"));
			var customField = ((ICustomFieldProvider)dummy).GetCustomBusinessObject().GetPropertyAndDescription(customPropertyIdentifier).Property;
			var legacyIdentifiers = new List<string>()
			{
				CustomBusinessObject.GetLegacyIdentifier1(customPropertyName, typeof(ZString)),
				CustomBusinessObject.GetLegacyIdentifier2(customPropertyName, typeof(ZString)),
				CustomBusinessObject.GetLegacyIdentifier3(customPropertyName, typeof(ZString)),
			};

			foreach (var legacyIdentifier in legacyIdentifiers)
			{
				Assert(customField.HasMatchingLegacyIdentifier(legacyIdentifier));
			}

			Assert(customField.HasMatchingLegacyIdentifier(customPropertyIdentifier));
		}
	}
}
