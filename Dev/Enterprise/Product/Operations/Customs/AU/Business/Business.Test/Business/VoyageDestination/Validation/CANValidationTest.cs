using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CANValidationTest : TestCase
	{
		public void TestValidateCAN()
		{
			var factory = new BusinessObjectFactory();
			var dummy = new BusinessObjectForTest(factory);
			var testValidation = new CANValidation();

			dummy.SomeProperty = "";
			AssertEquals("No Notifications should be on J1_EntryNumberInfo after calling ClearAllNotifications()", false, dummy.SomePropertyInfo.HasMessageErrors());

			dummy.SomePropertyInfo.ClearAllNotifications();
			dummy.SomeProperty = "CY46W3XAH";
			testValidation.ValidateCAN(dummy.SomePropertyInfo);
			AssertEquals("Valid Entry Number Valid", false, dummy.SomePropertyInfo.HasNotifications());

			dummy.SomePropertyInfo.ClearAllNotifications();
			dummy.SomeProperty = "BADLENGTHNUMBER";
			testValidation.ValidateCAN(dummy.SomePropertyInfo);
			AssertEquals("Bad Entry Number - Length to long", "Entry Number is not the correct length - should be 9 characters.", dummy.SomePropertyInfo.GetMessageErrors().GetFirstMessage());

			dummy.SomePropertyInfo.ClearAllNotifications();
			dummy.SomeProperty = "A76GG9FEG";
			testValidation.ValidateCAN(dummy.SomePropertyInfo);
			AssertEquals("Valid Entry Number Valid", false, dummy.SomePropertyInfo.HasNotifications());

			dummy.SomePropertyInfo.ClearAllNotifications();
			dummy.SomeProperty = "A76IO9FBG";
			testValidation.ValidateCAN(dummy.SomePropertyInfo);
			AssertEquals("Invalid Characters In string", string.Format("Entry Number contains invalid characters : '{0}'", "IOB"), dummy.SomePropertyInfo.GetMessageErrors().GetFirstMessage());
		}

		sealed class BusinessObjectForTest : NonPersistentBusinessObject, IObsoleteValidation
		{
			public BusinessObjectForTest(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			[MaxLength(50)]
			public ZString SomeProperty
			{
				get => someProperty;
				set => SetNonPersistentPropertyValue(SomePropertyInfo, ref someProperty, value);
			}
			ZString someProperty;

			public ZPropertyInfo SomePropertyInfo => GetZPropertyInfo(nameof(SomeProperty));
		}
	}
}
