using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	class Report_CustomsEntriesInvoiceLinesTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestReport_CustomsEntriesInvoiceLines_DatabaseObjectTest()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_OH_Importer = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			dec.JE_OH_Supplier = dec.JE_OH_Importer;
			dec.JE_RL_NKFinalDestination = "GBLBA";

			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			invoiceHeader.JZ_JE = dec.PK;
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_JZ = invoiceHeader.PK;

			var ceh = Factory.New<CusEntryHeader>();
			ceh.CH_JE = dec.PK;
			ceh.CH_EntryReleaseDate = ZDateTime.BrettsBirthday;
			ceh.CH_BondValidToDate = new ZDate(2024, 8, 4);

			var cel = Factory.New<CusEntryLine>();
			cel.CL_CH = ceh.PK;

			invoiceLine.JI_CL = cel.PK;

			invoiceLine.JI_AddInfo = "ProcedureCode=Defunct*StatisticalValue=123.45";
			invoiceLine.JI_Procedure = "PROC";
			invoiceLine.JI_NetWeight = 1.2m;
			invoiceLine.JI_NetWeightUQ = "LB";
			invoiceLine.JI_Weight = 1.1m;
			invoiceLine.JI_WeightUQ = "KG";
			invoiceLine.JI_CountryOfOrigin = "DE";

			var cen = Factory.New<Common.CusEntryNumber>();
			cen.FillWithValidTestData();
			cen.CE_ParentID = ceh.PK;
			cen.CE_ParentTable = ceh.TableName;

			Factory.Save();

			var sqlText = $"SELECT * from dbo.Report_CustomsEntriesInvoiceLines('{dec.Branch.Company.PK}', null, null)";
			var soBOC = new DynamicBusinessObjectCollection(Factory);
			soBOC.Load(sqlText);

			var bizO = soBOC[0];
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(soBOC.Count, Is.GreaterThan(0), "Something Loaded");
				NUnit.Framework.Assert.That(bizO["FinalDestinationCountry"].ToString(), Is.EqualTo("GB"), "FinalDestinationCountry");
				NUnit.Framework.Assert.That(bizO["OriginCountry"].ToString(), Is.EqualTo("DE"), "OriginCountry");
				NUnit.Framework.Assert.That(bizO["ProcedureCode"].ToString(), Is.EqualTo("PROC"), "ProcedureCode");
				NUnit.Framework.Assert.That((ZDecimal)bizO["StatisticalValue"], Is.EqualTo(123.45m).Using(CustomComparers.TypeComparison), "StatisticalValue");
				NUnit.Framework.Assert.That((ZDecimal)bizO["NettWeight"], Is.EqualTo(1.2m).Using(CustomComparers.TypeComparison), "NettWeight");
				NUnit.Framework.Assert.That(bizO["NettWeightUnit"].ToString(), Is.EqualTo("LB"), "NettWeightUnit");
				NUnit.Framework.Assert.That((ZDecimal)bizO["Weight"], Is.EqualTo(1.1m).Using(CustomComparers.TypeComparison), "Weight");
				NUnit.Framework.Assert.That(bizO["WeightUnit"].ToString(), Is.EqualTo("KG"), "WeightUnit");
				NUnit.Framework.Assert.That((ZDateTime)bizO["ReleaseDate"], Is.EqualTo(ZDateTime.BrettsBirthday), "ReleaseDate");
				NUnit.Framework.Assert.That((ZDateTime)bizO["ReExportDate"], Is.EqualTo(new ZDateTime(2024, 08, 04)), "ReExportDate");
			});
		}
	}
}
