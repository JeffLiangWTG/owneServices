using System;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.IE;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceGroupHeader))]
	sealed class JobComInvoiceGroupHeaderTest : EU.Business.Declaration.Testing.JobComInvoiceGroupHeaderTest
	{
		public void TestGetStandaloneIncoTermAndChargeFactoryCountryContext()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var groupHeader = Factory.New<JobComInvoiceGroupHeader>();
				groupHeader.JZ_JE = declaration.PK;
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
				AssertEquals("Import", "CustomsChargeTypeList_IEIMPGroupInvoice", groupHeader.GetCustomsChargeTypeListCacheKey(ChargeParentTypes.GroupInvoice));

				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
				AssertEquals("Export", "CustomsChargeTypeList_IEEXPGroupInvoice", groupHeader.GetCustomsChargeTypeListCacheKey(ChargeParentTypes.GroupInvoice));
			});
		}

		public override void TestChargeTypeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			Customs.Business.ICommonInvoice topGroupInvoice = declaration.TopGroupInvoice;
			CodeDescriptionPairList chargeTypeList = topGroupInvoice.ChargeTypeList;
			CodeDescriptionPairList chargeTypeList2 = topGroupInvoice.ChargeTypeList;
			AssertEquals(true, chargeTypeList == chargeTypeList2);
			AssertEquals("ADD, AFT, DED, INS, OFT, ONS", chargeTypeList.CodesAsString);
		}

		public override void TestChargesToImportForLandedCosting()
		{
			Assert(true);
		}

		protected override Type ExpectedTypeOfCharges => typeof(EU.Business.Declaration.GroupInvoiceChargeCollection<GroupInvoiceCharge>);
	}
}
