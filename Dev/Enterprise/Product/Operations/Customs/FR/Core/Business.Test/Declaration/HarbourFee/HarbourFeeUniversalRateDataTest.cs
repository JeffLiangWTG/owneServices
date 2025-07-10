using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class HarbourFeeUniversalRateDataTests : TestCaseWithFactory
	{
		public void TestValueForDutyCalculateFromFormula()
		{
			var formulasForTest = new Dictionary<ZString, ZDecimal>();
			FillFormulasForTest(formulasForTest);
			var declaration = EntryHeaderUniversalRateHelper.CreateDeclarationForHarbourRateTest(Factory);

			CombineAssertions(() =>
			{
				foreach (var testKey in formulasForTest.Keys)
				{
					try
					{
						var expectedResult = formulasForTest[testKey];
						var wrapper = new HarbourFeeEntryHeaderWrapper(declaration);
						var testRateData = new HarbourFeeUniversalRateData(wrapper, testKey);
						AssertEquals(string.Format("{0}=>{1}", testKey, expectedResult), expectedResult, testRateData.ValueForDuty);
					}
					catch (Exception e)
					{
						AssertEquals(testKey + " Exception", string.Empty, e.Message);
					}
				}
			});
		}

		void FillFormulasForTest(Dictionary<ZString, ZDecimal> formulasForTest)
		{
			formulasForTest.Add("IF([TLCL] > 1, MAX(2, 0.5 * [TLCL]), 0)", 4.5);
			formulasForTest.Add("IF([TFCL] > 1, MAX(2, 0.5 * [TFCL]), 0)", 6.0);
			formulasForTest.Add("[20LCL] * 10 + [40LCL] * 5 + [45LCL] * 2", 19.0);
			formulasForTest.Add("[20FCL] * 10 + [40FCL] * 5 + [45FCL] * 2", 19.0);
			formulasForTest.Add("IF([UNDG] = 1, [TLCL] * 2.0, [TLCL] * 1.5)", 18.0);
			formulasForTest.Add("IF([UNDG] = 1, [TFCL] * 2.0, [TFCL] * 1.5)", 24.0);
			formulasForTest.Add("[CON] * 5", 40.0);
		}
	}

	public static class EntryHeaderUniversalRateHelper
	{
		public static JobDeclaration CreateDeclarationForHarbourRateTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();

			var entryHeader = CreateEntryHeaderForHarbourRateTest(declaration);

			var entryLine = entryHeader.AllEntryLines.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.UNDGs.AddNew();
			foreach (var container in declaration.CusContainers)
			{
				var containerInvoiceLine = invoiceLine.ContainersPivot.AddNew();
				containerInvoiceLine.C2_CO = container.PK;
			}

			factory.Save();

			return declaration;
		}

		static CusEntryHeader CreateEntryHeaderForHarbourRateTest(JobDeclaration declaration)
		{
			var factory = new BusinessObjectFactory();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var container20LCL = declaration.CusContainers.AddNew();
			container20LCL.CO_Weight = 999.1m;
			container20LCL.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;

			var container20FCL = declaration.CusContainers.AddNew();
			container20FCL.CO_Weight = 1999.1m;
			container20FCL.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;

			var container40LCL = declaration.CusContainers.AddNew();
			container40LCL.CO_Weight = 2999.1m;
			container40LCL.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;

			var container40FCL = declaration.CusContainers.AddNew();
			container40FCL.CO_Weight = 3999.1m;
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

			factory.Save();

			var refContainer20LCL = factory.New<RefContainer>();
			refContainer20LCL.RC_StorageClass = "20";
			refContainer20LCL.RC_Code = "C20LCL";
			container20LCL.CO_RC = refContainer20LCL.PK;

			var refContainer20FCL = factory.New<RefContainer>();
			refContainer20FCL.RC_StorageClass = "20";
			refContainer20FCL.RC_Code = "C20FCL";
			container20FCL.CO_RC = refContainer20FCL.PK;

			var refContainer40LCL = factory.New<RefContainer>();
			refContainer40LCL.RC_StorageClass = "40";
			refContainer40LCL.RC_Code = "C40LCL";
			container40LCL.CO_RC = refContainer40LCL.PK;

			var refContainer40FCL = factory.New<RefContainer>();
			refContainer40FCL.RC_StorageClass = "40";
			refContainer40FCL.RC_Code = "C40FCL";
			container40FCL.CO_RC = refContainer40FCL.PK;

			var refContainer45LCL = factory.New<RefContainer>();
			refContainer45LCL.RC_StorageClass = "45";
			refContainer45LCL.RC_Code = "C45LCL";
			container45LCL.CO_RC = refContainer45LCL.PK;

			var refContainer45FCL = factory.New<RefContainer>();
			refContainer45FCL.RC_StorageClass = "45";
			refContainer45FCL.RC_Code = "C45FCL";
			container45FCL.CO_RC = refContainer45FCL.PK;

			empty45LCLContainer.CO_RC = refContainer45LCL.PK;
			empty45FCLContainer.CO_RC = refContainer45FCL.PK;

			factory.Save();

			container20LCL.JobContainer.JC_RC = refContainer20LCL.PK;
			container20FCL.JobContainer.JC_RC = refContainer20FCL.PK;
			container40LCL.JobContainer.JC_RC = refContainer40LCL.PK;
			container40FCL.JobContainer.JC_RC = refContainer40FCL.PK;
			container45LCL.JobContainer.JC_RC = refContainer45LCL.PK;
			container45FCL.JobContainer.JC_RC = refContainer45FCL.PK;
			empty45LCLContainer.JobContainer.JC_RC = refContainer45LCL.PK;
			empty45FCLContainer.JobContainer.JC_RC = refContainer45FCL.PK;

			factory.Save();

			return entryHeader;
		}
	}
}
