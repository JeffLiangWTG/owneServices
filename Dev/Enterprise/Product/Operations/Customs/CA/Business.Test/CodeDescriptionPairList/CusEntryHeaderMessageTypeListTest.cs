using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusEntryHeaderMessageTypeListTest : TestCaseWithFactory
	{
		public void TestGetMessagesTypesRightFor()
		{
			AssertEquals(MessageType.G7ExportDeclaration, MessageTypeList.GetMessagesTypesRightFor(MessageTypeList.Codes.G7Export));
			AssertEquals(MessageType.DataLoadingModule, MessageTypeList.GetMessagesTypesRightFor(MessageTypeList.Codes.DataLoadingModule));
			AssertEquals(MessageType.Undefined, MessageTypeList.GetMessagesTypesRightFor(""));
			AssertExceptionThrown(typeof(InvalidOperationException), delegate
			{ MessageTypeList.GetMessagesTypesRightFor("ZZZ"); });
		}

		public void TestGetCADOrB3CMessageType()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				AssertEquals(MessageTypeList.Codes.B3CUSDEC, MessageTypeList.GetCADOrB3CMessageType(true));
				AssertEquals(MessageTypeList.Codes.CommercialAccountingDeclaration, MessageTypeList.GetCADOrB3CMessageType(false));
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, false))
			{
				AssertEquals(MessageTypeList.Codes.B3CUSDEC, MessageTypeList.GetCADOrB3CMessageType(true));
				AssertEquals(MessageTypeList.Codes.B3CUSDEC, MessageTypeList.GetCADOrB3CMessageType(false));
			}
		}
	}
}
