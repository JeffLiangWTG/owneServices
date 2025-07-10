using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class MessageStatusCodeListTest : TestCaseWithFactory
	{
		public void TestHasBeenSentCustoms()
		{
			AssertEquals("Expect False When blank", false, MessageStatusCodeList.HasBeenSentCustoms(""));
			AssertEquals($"Expect False When {MessageStatusCodeList.Codes.NotSent}", false, MessageStatusCodeList.HasBeenSentCustoms(MessageStatusCodeList.Codes.NotSent));
			AssertEquals($"Expect False When {MessageStatusCodeList.Codes.Error}", false, MessageStatusCodeList.HasBeenSentCustoms(MessageStatusCodeList.Codes.Error));
			AssertEquals($"Expect True When {MessageStatusCodeList.Codes.Accepted}", true, MessageStatusCodeList.HasBeenSentCustoms(MessageStatusCodeList.Codes.Accepted));
			AssertEquals($"Expect True When {MessageStatusCodeList.Codes.Awaiting}", true, MessageStatusCodeList.HasBeenSentCustoms(MessageStatusCodeList.Codes.Awaiting));
			AssertEquals($"Expect True When {MessageStatusCodeList.Codes.Sent}", true, MessageStatusCodeList.HasBeenSentCustoms(MessageStatusCodeList.Codes.Sent));
			AssertEquals($"Expect True When {MessageStatusCodeList.Codes.Unknown}", true, MessageStatusCodeList.HasBeenSentCustoms(MessageStatusCodeList.Codes.Unknown));
			AssertEquals($"Expect True When {MessageStatusCodeList.Codes.Updated}", true, MessageStatusCodeList.HasBeenSentCustoms(MessageStatusCodeList.Codes.Updated));
		}

		public void TestGetMappedCode()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "ManifestMessageStatus");

			var sg8 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "8", "8 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg8.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MessageCleared, bool.TrueString);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg8.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ManifestType, "SGI");
			var sg9 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "9", "9 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg9.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MessageCleared, bool.FalseString);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg9.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ManifestType, "SGI");
			var au9 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "9", "9 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(au9.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MessageCleared, bool.TrueString);
			helper.CreateNewOrGetExistingCusCodeListAttribute(au9.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ManifestType, "SGI");
			var au10 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "10", "10 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(au10.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MessageCleared, bool.TrueString);
			helper.CreateNewOrGetExistingCusCodeListAttribute(au10.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ManifestType, "SGI");
			var sg11 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "11", "11 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg11.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MessageCleared, bool.TrueString);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg11.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ManifestType, "SGE");

			var sg12 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "12", "12 DESC", ZDateTime.Today.AddMonths(-3), ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg12.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MessageCleared, "BL");
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg12.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ManifestType, "SGI");
			var sg13 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "13", "13 DESC", ZDateTime.Today.AddMonths(-1), ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg13.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MessageCleared, "");
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg13.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ManifestType, "SGI");

			var sg14 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "14", "14 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg14.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MessageCleared, bool.TrueString);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg14.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ManifestType, "SGI");

			Factory.Save();

			AssertEquals(MessageStatusCodeList.Codes.Accepted, MessageStatusCodeList.GetMappedCode(Factory, Core.Constants.CountryCodes.Singapore, "8", "SGI"));
			AssertEquals(MessageStatusCodeList.Codes.Error, MessageStatusCodeList.GetMappedCode(Factory, Core.Constants.CountryCodes.Singapore, "9", "SGI"));
			AssertEquals(MessageStatusCodeList.Codes.Unknown, MessageStatusCodeList.GetMappedCode(Factory, Core.Constants.CountryCodes.Singapore, "10", "SGI"));
			AssertEquals(MessageStatusCodeList.Codes.Unknown, MessageStatusCodeList.GetMappedCode(Factory, Core.Constants.CountryCodes.Singapore, "11", "SGI"));
			AssertEquals(MessageStatusCodeList.Codes.Error, MessageStatusCodeList.GetMappedCode(Factory, Core.Constants.CountryCodes.Singapore, "12", "SGI"));
			AssertEquals(MessageStatusCodeList.Codes.Error, MessageStatusCodeList.GetMappedCode(Factory, Core.Constants.CountryCodes.Singapore, "13", "SGI"));
			AssertEquals(MessageStatusCodeList.Codes.Unknown, MessageStatusCodeList.GetMappedCode(Factory, Core.Constants.CountryCodes.Singapore, "14", "SGI"));
		}
	}
}
