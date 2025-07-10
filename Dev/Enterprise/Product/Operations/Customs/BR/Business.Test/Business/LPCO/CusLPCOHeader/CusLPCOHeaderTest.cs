using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(CusLPCOHeader))]
	class CusLPCOHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestPopulateFromInvoiceLine()
		{
			var supplier = Factory.New<OrgHeader>();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			var permit = Factory.New<CusLPCOHeader>();
			permit.PopulateFromInvoiceLine(null);
			AssertEquals("CPH_OH_PermitHolder", ZGuid.Empty, permit.CPH_OH_PermitHolder);

			permit.PopulateFromInvoiceLine(invoiceLine);
			AssertEquals("CPH_OH_PermitHolder", ZGuid.Empty, permit.CPH_OH_PermitHolder);

			declaration.JE_OH_Supplier = supplier.PK;
			permit.PopulateFromInvoiceLine(invoiceLine);
			AssertEquals("CPH_OH_PermitHolder", supplier.PK, permit.CPH_OH_PermitHolder);
		}

		public void TestMessages()
		{
			var permit1 = Factory.New<CusLPCOHeader>();
			var permit2 = Factory.New<CusLPCOHeader>();

			var message1 = Factory.New<BREDIMessage>();
			message1.EM_LinkedObject = permit1;

			var message2 = Factory.New<BREDIMessage>();
			message2.EM_LinkedObject = permit1;

			var message3 = Factory.New<BREDIMessage>();
			message3.EM_LinkedObject = permit2;

			AssertEquals(2, permit1.Messages.Count);
			AssertEquals(1, permit2.Messages.Count);

			AssertEquals(true, permit1.Messages.ReadOnly);
			AssertEquals(true, permit1.Messages.IsManagedForDataRefresh);
		}

		public void TestSetDefaultValues()
		{
			var cusLPCO = Factory.New<CusLPCOHeader>();
			AssertEquals(CusPermitHeaderApplicationCodeList.Codes.Operational, cusLPCO.CPH_ApplicationCode);
			AssertEquals(Core.Constants.CountryCodes.Brazil, cusLPCO.CPH_RN_NKCountryCode);
			AssertEquals(GlbCompany.CurrentCompany.PK, cusLPCO.CPH_GC_Company);
			AssertEquals(PermitTypeList.Codes.LPC, cusLPCO.CPH_Type);
		}

		public void TestIMessageAttachee()
		{
			var cusLPCO = Factory.New<CusLPCOHeader>();
			AssertEquals(GlbCompany.CurrentCompany.FirstActiveBranch.PK, ((IMessageAttachee)cusLPCO).BranchPK);

			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();
			cusLPCO.CPH_GC_Company = company.PK;
			AssertEquals(branch.PK, ((IMessageAttachee)cusLPCO).BranchPK);
		}

		public void TestCPH_JobNumber()
		{
			var cusLPCO = Factory.New<CusLPCOHeader>();
			AssertEquals(true, cusLPCO.CPH_JobNumberInfo.ReadOnly);
		}

		[TestDate(2024, 1, 1)]
		public void TestCPH_JobNumber_IsGeneratedByNumberFountain()
		{
			var billItem = new UniqueNumberCustomisation(false);
			billItem.Elements.Cast<BillOfLadingNumberCustomisationElement>().ForEach(element => element.Include = true);
			using (BRCustomsDataRegistry.Instance.LPCOJobNumberCustomization.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, billItem))
			{
				var supplier = Factory.New<OrgHeader>();
				supplier.OH_Code = "PermitHolder";

				var supplier2 = Factory.New<OrgHeader>();
				supplier2.OH_Code = "PermitHolde";

				Factory.Save();

				CombineAssertions(() =>
				{
					var lpco = Factory.New<CusLPCOHeader>();

					AssertEquals(ZString.Empty, lpco.CPH_JobNumber);

					lpco.CPH_OH_PermitHolder = ZGuid.Invalid;
					lpco.CPH_StartDate = ZDate.Today;

					AssertExceptionThrown<ZSaveException>(Factory.Save);
					AssertEquals(ZString.Empty, lpco.CPH_JobNumber);

					lpco.CPH_OH_PermitHolder = supplier.PK;
					lpco.CPH_StartDate = ZDate.Today;

					Factory.Save();
					AssertEquals("BNEEDIEDI01ADAT4XQ100000002", lpco.CPH_JobNumber);

					var lpco2 = Factory.New<CusLPCOHeader>();
					lpco2.CPH_OH_PermitHolder = supplier2.PK;
					lpco2.CPH_StartDate = ZDate.Today;

					Factory.Save();
					AssertEquals("BNEEDIEDI01ADAT4XQ100000003", lpco2.CPH_JobNumber);

					lpco2.CPH_OH_PermitHolder = ZGuid.Invalid;
					AssertExceptionThrown<ZSaveException>(Factory.Save);
					AssertEquals("BNEEDIEDI01ADAT4XQ100000003", lpco2.CPH_JobNumber);
				});
			}
		}

		public void TestPropertiesToExcludeFromCloning()
		{
			var lpco = Factory.New<CusLPCOHeader>();
			AssertEquals("SupportsClone", true, lpco.SupportsClone());
			lpco.CPH_JobNumber = "BNEEDIEDI01ADAT4XQ100000001";
			lpco.CPH_CustomsStatus = "CLR";
			lpco.CPH_MessageStatus = "ACC";

			var clonedLpco = lpco.Clone() as CusLPCOHeader;
			AssertEquals("CPH_JobNumber should not be copied", ZString.Empty, clonedLpco.CPH_JobNumber);
			AssertEquals("CPH_CustomsStatus should not be copied", ZString.Empty, clonedLpco.CPH_CustomsStatus);
			AssertEquals("CPH_MessageStatus should not be copied", ZString.Empty, clonedLpco.CPH_MessageStatus);
		}

		public void TestCPH_MessageStatusReadOnly()
		{
			var cusLPCO = Factory.NewWithValidTestData<CusLPCOHeader>();
			Assert("CPH_MessageStatus should be ReadOnly", cusLPCO.CPH_MessageStatusInfo.ReadOnly);
		}

		public void TestInvoicingSupporter()
		{
			var lpcoHeader = Factory.NewWithValidTestData<CusLPCOHeader>();
			AssertType<CusLPCOHeaderInvoicingSupporter>(lpcoHeader.InvoicingSupporter);
		}
	}
}
