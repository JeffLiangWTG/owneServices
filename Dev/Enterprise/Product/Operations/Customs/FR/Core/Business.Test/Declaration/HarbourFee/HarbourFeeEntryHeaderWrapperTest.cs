using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class HarbourFeeEntryHeaderWrapperTests : TestCaseWithFactory
	{
		public void TestNumberOfTwentyFootContainers()
		{
			var declaration = CreateTestDataForNumberOfContainersTest();
			var wrapper = new HarbourFeeEntryHeaderWrapper(declaration);
			AssertEquals("NumberOfTwentyFootLCLContainers should retrun number of 20 ft containers using LCL mode.", 1, wrapper.NumberOfTwentyFootLCLContainers);
			AssertEquals("NumberOfTwentyFootFCLContainers should retrun number of 20 ft containers using FCL mode.", 1, wrapper.NumberOfTwentyFootFCLContainers);
		}

		public void TestNumberOfFortyFootContainers()
		{
			var declaration = CreateTestDataForNumberOfContainersTest();
			var wrapper = new HarbourFeeEntryHeaderWrapper(declaration);
			AssertEquals("NumberOfFortyFootLCLContainers should retrun number of 40 ft containers using LCL mode.", 1, wrapper.NumberOfFortyFootLCLContainers);
			AssertEquals("NumberOfFortyFootFCLContainers should retrun number of 40 ft containers using FCL mode.", 1, wrapper.NumberOfFortyFootFCLContainers);
		}

		public void TestNumberOfFortyFiveFootContainers()
		{
			var declaration = CreateTestDataForNumberOfContainersTest();
			var wrapper = new HarbourFeeEntryHeaderWrapper(declaration);
			AssertEquals("NumberOfFortyFiveFootLCLContainers should retrun number of 45 ft containers using LCL mode, whatever their weight.", 2, wrapper.NumberOfFortyFiveFootLCLContainers);
			AssertEquals("NumberOfFortyFiveFootFCLContainers should retrun number of 45 ft containers using FCL mode, whatever their weight.", 2, wrapper.NumberOfFortyFiveFootFCLContainers);
		}

		public void TestTotalMassInTonnes()
		{
			var declaration = CreateTestDataForNumberOfContainersTest();
			var wrapper = new HarbourFeeEntryHeaderWrapper(declaration);
			AssertEquals("TotalLCLContainersMassInTonnes is the sum of weights of all containers using LCL mode.", (ZDecimal)10, wrapper.TotalLCLContainersMassInTonnes);
			AssertEquals("TotalFCLContainersMassInTonnes is the sum of weights of all containers using FCL mode.", (ZDecimal)11, wrapper.TotalFCLContainersMassInTonnes);
		}

		public void TestIsDangerousGoods()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var entryLine = entry.AllEntryLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;
			Factory.Save();

			var wrapper = new HarbourFeeEntryHeaderWrapper(declaration);
			Assert(!wrapper.IsDangerousGoods);

			invoiceLine1.UNDGs.AddNew();
			wrapper = new HarbourFeeEntryHeaderWrapper(declaration);
			Assert(wrapper.IsDangerousGoods);
		}

		[TestDate(2022, 10, 9)]
		public void TestDateOfValuation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var entryLine = entry.AllEntryLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ValuationDateOverride = ZDateTime.BrettsBirthday;
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;
			Factory.Save();

			var wrapper = new HarbourFeeEntryHeaderWrapper(declaration);
			AssertEquals(new DateTime(2022, 10, 9), wrapper.DateOfValuation);
		}

		public void TestCustomsValue()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var entryLine1 = entry.AllEntryLines.AddNew();
			entryLine1.CL_CustomsValue = 1m;
			var entryLine2 = entry.AllEntryLines.AddNew();
			entryLine2.CL_CustomsValue = 2m;
			Factory.Save();

			var wrapper = new HarbourFeeEntryHeaderWrapper(declaration);
			AssertEquals(3m, wrapper.CustomsValue);
		}

		public void TestContainerCount()
		{
			var declaration = CreateTestDataForNumberOfContainersTest();
			var wrapper = new HarbourFeeEntryHeaderWrapper(declaration);
			AssertEquals(8, wrapper.ContainerCount);
		}

		public void TestContainerCountPartial()
		{
			var declaration = CreateTestDataForNumberOfContainersTest(3);
			var wrapper = new HarbourFeeEntryHeaderWrapper(declaration);
			AssertEquals(3, wrapper.ContainerCount);
		}

		JobDeclaration CreateTestDataForNumberOfContainersTest(int numberContainersToAssociate = -1)
		{
			var declaration = Factory.New<JobDeclaration>();

			var container20LCL = declaration.CusContainers.AddNew();
			container20LCL.CO_Weight = 1659.1m;
			container20LCL.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;

			var container20FCL = declaration.CusContainers.AddNew();
			container20FCL.CO_Weight = 1659.1m;
			container20FCL.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;

			var container40LCL = declaration.CusContainers.AddNew();
			container40LCL.CO_Weight = 2219.1m;
			container40LCL.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;

			var container40FCL = declaration.CusContainers.AddNew();
			container40FCL.CO_Weight = 2219.1m;
			container40FCL.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;

			var container45LCL = declaration.CusContainers.AddNew();
			container45LCL.CO_Weight = 4999.1m;
			container45LCL.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;

			var container45FCL = declaration.CusContainers.AddNew();
			container45FCL.CO_Weight = 5999.1m;
			container45FCL.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;

			var empty45LCLContainer = declaration.CusContainers.AddNew();
			empty45LCLContainer.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;

			var empty45FCLContainer = declaration.CusContainers.AddNew();
			empty45FCLContainer.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;

			Factory.Save();

			var refContainer20LCL = Factory.New<RefContainer>();
			refContainer20LCL.RC_StorageClass = "20";
			refContainer20LCL.RC_Code = "C20LCL";
			container20LCL.CO_RC = refContainer20LCL.PK;

			var refContainer20FCL = Factory.New<RefContainer>();
			refContainer20FCL.RC_StorageClass = "20";
			refContainer20FCL.RC_Code = "C20FCL";
			container20FCL.CO_RC = refContainer20FCL.PK;

			var refContainer40LCL = Factory.New<RefContainer>();
			refContainer40LCL.RC_StorageClass = "40";
			refContainer40LCL.RC_Code = "C40LCL";
			container40LCL.CO_RC = refContainer40LCL.PK;

			var refContainer40FCL = Factory.New<RefContainer>();
			refContainer40FCL.RC_StorageClass = "40";
			refContainer40FCL.RC_Code = "C40FCL";
			container40FCL.CO_RC = refContainer40FCL.PK;

			var refContainer45LCL = Factory.New<RefContainer>();
			refContainer45LCL.RC_StorageClass = "45";
			refContainer45LCL.RC_Code = "C45LCL";
			container45LCL.CO_RC = refContainer45LCL.PK;

			var refContainer45FCL = Factory.New<RefContainer>();
			refContainer45FCL.RC_StorageClass = "45";
			refContainer45FCL.RC_Code = "C45FCL";
			container45FCL.CO_RC = refContainer45FCL.PK;

			empty45LCLContainer.CO_RC = refContainer45LCL.PK;
			empty45FCLContainer.CO_RC = refContainer45FCL.PK;

			Factory.Save();

			container20LCL.JobContainer.JC_RC = refContainer20LCL.PK;
			container20FCL.JobContainer.JC_RC = refContainer20FCL.PK;
			container40LCL.JobContainer.JC_RC = refContainer40LCL.PK;
			container40FCL.JobContainer.JC_RC = refContainer40FCL.PK;
			container45LCL.JobContainer.JC_RC = refContainer45LCL.PK;
			container45FCL.JobContainer.JC_RC = refContainer45FCL.PK;
			empty45LCLContainer.JobContainer.JC_RC = refContainer45LCL.PK;
			empty45FCLContainer.JobContainer.JC_RC = refContainer45FCL.PK;

			Factory.Save();

			var entryHeader = declaration.ActiveEntryHeaders.AddNew(typeof(CusEntryHeader)) as CusEntryHeader;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "A";
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			if (numberContainersToAssociate < 0 || numberContainersToAssociate > declaration.CusContainers.Count)
			{
				numberContainersToAssociate = declaration.CusContainers.Count;
			}

			for (var i = 0; i < numberContainersToAssociate; i++)
			{
				var containerInvoiceLine = invoiceLine.ContainersPivot.AddNew();
				containerInvoiceLine.C2_CO = declaration.CusContainers[i].PK;
			}

			Factory.Save();

			return declaration;
		}
	}
}
