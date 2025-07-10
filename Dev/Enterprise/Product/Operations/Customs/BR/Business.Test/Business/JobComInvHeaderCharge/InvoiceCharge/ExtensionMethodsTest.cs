using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ExtensionMethodsTest : TestCaseWithFactory
	{
		public void TestGetIncoTermChargeFactoryCacheKey()
		{
			var declaration = Factory.New<JobDeclaration>();
			foreach (var messageType in declaration.Lookups.MessageTypeList.GetAllCodes())
			{
				declaration.JE_MessageType = messageType;
				AssertEquals(messageType, GetExceptedCacheKeyForMesasgeType(messageType), declaration.GetIncoTermChargeFactoryCacheKey());
			}
		}

		public void TestGetCustomsChargeTypeListCacheKey_InvoiceGroup()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceGroup1 = declaration.TopGroupInvoice as JobComInvoiceGroupHeader;
			var invoiceGroup2 = Factory.New<JobComInvoiceGroupHeader>();
			foreach (var messageType in declaration.Lookups.MessageTypeList.GetAllCodes())
			{
				declaration.JE_MessageType = messageType;
				AssertEquals(messageType, GetExceptedCacheKeyForMesasgeType(messageType), invoiceGroup1.GetCustomsChargeTypeListCacheKey());
				AssertEquals(messageType, string.Empty, invoiceGroup2.GetCustomsChargeTypeListCacheKey());
			}
		}

		public void TestGetCustomsChargeTypeListCacheKey_InvoiceHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			var invoice2 = Factory.New<JobComInvoiceHeader>();
			foreach (var messageType in declaration.Lookups.MessageTypeList.GetAllCodes())
			{
				declaration.JE_MessageType = messageType;
				AssertEquals(messageType, GetExceptedCacheKeyForMesasgeType(messageType), invoice1.GetCustomsChargeTypeListCacheKey());
			}
			foreach (var messageType in invoice1.Lookups.MessageTypes.GetAllCodes())
			{
				invoice2.JZ_MessageType = messageType;
				AssertEquals(messageType, GetExceptedCacheKeyForMesasgeType(messageType), invoice2.GetCustomsChargeTypeListCacheKey());
			}
		}

		string GetExceptedCacheKeyForMesasgeType(string messageType)
		{
			switch (messageType)
			{
				case BRJobMessageTypeList.Codes.ImportLicense:
				case BRJobMessageTypeList.Codes.Export:
					return messageType;
				case BRJobMessageTypeList.Codes.ImportSiscomex:
				case BRJobMessageTypeList.Codes.Import:
				case BRJobMessageTypeList.Codes.WarehousedByExternalAgent:
				case JobMessageTypeList.MoreCodes.AdvanceShippingNotice:
					return BRJobMessageTypeList.Codes.Import;
				default:
					return "";
			}
		}
	}
}
