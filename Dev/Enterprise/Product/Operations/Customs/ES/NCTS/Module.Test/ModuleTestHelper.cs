using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Module.Testing
{
	public static class ModuleTestHelper
	{
		public static void AssertMessageSent(ZString entryName, NctsHeader nctsHeader)
		{
			var messages = nctsHeader.Messages;
			Assertion.AssertEquals("1 EDIMessage was created for " + entryName, 1, messages.Count);
			var message = messages[0];
			Assertion.AssertEquals("Sent message for " + entryName + " has correct EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
			Assertion.AssertEquals("Sent message for " + entryName + " has correct EM_MessageType", DeclarationMessageTypeList.Codes.TransitNcts5Query, message.EM_MessageType);
		}

		public static void AddGuaranteeToHeader(NctsHeader header, ZString guaranteeCode)
		{
			var guarantee = header.GetEffectiveGuarantees().AddNew();
			guarantee.PW_BondNumber = guaranteeCode;
		}

		public static CusGuaranteeHeader CreateGuaranteeHeaderDetail(BusinessObjectFactory factory, ZString guaranteeCode, bool addOBLTransaction = true, decimal oblTranValue = 1000m, string oblTranReference = "OPENING", bool withOldEndDate = false)
		{
			var holder = factory.NewWithValidTestData<OrgHeader>();

			var guaranteeHeader = factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Number = guaranteeCode;
			guaranteeHeader.CPH_OH_PermitHolder = holder.PK;
			guaranteeHeader.CPH_StartDate = ZDate.Today.AddMonths(-1);
			guaranteeHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
			guaranteeHeader.CPH_SystemCreateTimeUtc = ZDate.Today;
			guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
			guaranteeHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Spain;

			if (addOBLTransaction)
			{
				AddOBLTransaction(guaranteeHeader, oblTranValue, oblTranReference);
			}

			if (withOldEndDate)
			{
				guaranteeHeader.CPH_EndDate = ZDate.Today.AddDays(-10);
			}

			return guaranteeHeader;
		}

		static void AddOBLTransaction(CusGuaranteeHeader guaranteeHeader, ZDecimal value, ZString reference)
		{
			var transaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transaction.CPL_Reference = reference;
			transaction.CPL_TranValue = value;
			transaction.CPL_Comment = "OPENING";
			transaction.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
		}
	}
}
