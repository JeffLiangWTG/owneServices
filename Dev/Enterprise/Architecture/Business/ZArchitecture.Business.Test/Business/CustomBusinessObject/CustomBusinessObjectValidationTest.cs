using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace CargoWise.EntityFramework.Testing
{
	sealed class CustomBusinessObjectValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateAll()
		{
			CustomBusinessObject cusObj = GetCustomBusinessObject();
			ZPropertyInfo propInfo = cusObj.GetZPropertyInfo("ZZZ_String");
			AssertNoErrors(propInfo);
			cusObj.Validation.ValidateAll();
			AssertHasError(propInfo, "Please enter a value.");
		}

		public void TestValidateDateTime()
		{
			CustomBusinessObject cusObj = GetCustomBusinessObject();
			ZPropertyInfo propInfo = cusObj.GetZPropertyInfo("ZZZ_DateTime");
			AssertNoErrors(propInfo);
			cusObj["ZZZ_DateTime"] = ZDateTime.Invalid;
			AssertHasError(propInfo, "Enter a valid selection.");
			cusObj["ZZZ_DateTime"] = ZDateTime.Today;
			AssertNoErrors(propInfo);
		}

		public void TestValidateDateTimeOffset()
		{
			CustomBusinessObject cusObj = GetCustomBusinessObject();
			ZPropertyInfo propInfo = cusObj.GetZPropertyInfo("ZZZ_DateTimeOffset");
			AssertNoErrors(propInfo);
			cusObj["ZZZ_DateTimeOffset"] = ZDateTimeOffset.Invalid;
			AssertHasError(propInfo, "Enter a valid selection.");
			cusObj["ZZZ_DateTimeOffset"] = ZDateTimeOffset.Today;
			AssertNoErrors(propInfo);
		}

		public void TestValidateGeography()
		{
			CustomBusinessObject cusObj = GetCustomBusinessObject();
			ZPropertyInfo propInfo = cusObj.GetZPropertyInfo("ZZZ_Geography");
			AssertNoErrors(propInfo);
			cusObj["ZZZ_Geography"] = ZGeography.Invalid;
			AssertHasErrors(propInfo);
			cusObj["ZZZ_Geography"] = new ZGeography("121 48");
			AssertNoErrors(propInfo);
		}

		void ValidateString(ZPropertyInfo propInfo)
		{
			MandatoryValidation.CheckEntered(propInfo);
		}

		void ValidateDateTime(ZPropertyInfo propInfo)
		{
			TypeValidation.CheckValidZDateTimeWithoutRange(propInfo);
		}

		void ValidateDateTimeOffset(ZPropertyInfo propInfo)
		{
			TypeValidation.CheckValidZDateTimeOffsetWithoutRange(propInfo);
		}

		void ValidateGeography(ZPropertyInfo propInfo)
		{
			TypeValidation.CheckValidGeography(propInfo);
		}

		CustomBusinessObject GetCustomBusinessObject()
		{
			Dictionary<string, object> values = new Dictionary<string, object>();

			return new CustomBusinessObject(Factory, null, new CustomPropertyCollectionImpl(
				propertyName =>
				{
					object value;
					return values.TryGetValue(propertyName, out value) ? value : null;
				},
				(propertyName, value) =>
				{
					values[propertyName] = value;
					return true;
				})
			{
				{ typeof(ZString), "ZZZ_String", ValidateString },
				{ typeof(ZDecimal), "ZZZ_Decimal" },
				{ typeof(ZDateTime), "ZZZ_DateTime", ValidateDateTime },
				{ typeof(ZDateTimeOffset), "ZZZ_DateTimeOffset", ValidateDateTimeOffset },
				{ typeof(ZGeography), "ZZZ_Geography", ValidateGeography },
				{ typeof(ZBool), "ZZZ_Bool" },
			});
		}
	}
}
