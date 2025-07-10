using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	[TestedType(typeof(FECChallenge))]
	public class FECChallengeTest : Customs.Business.Testing.CusCodeDataTest<FECChallenge>
	{
		(JobDeclaration JobDeclaration, JobComInvoiceLineForTesting JobComInvoiceLine) SetupJobDeclaration()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType("GB", "IMP");
			Factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "11111111", ZDateTime.BrettsBirthday, ZDateTime.Today.AddDays(1));
			helper.CreateTariffUOM(tariff, Constants.UnitOfMeasureTypes.StatisticalUOMType, "CU1");

			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;

			var jobComInvoiceLine = Factory.New<JobComInvoiceLineForTesting>();
			jobComInvoiceLine.UniversalTariffReturns = tariff;
			jobComInvoiceLine.UseUniversalTariffCoreReturns = true;
			dec.InvoiceLines.Add(jobComInvoiceLine);
			return (dec, jobComInvoiceLine);
		}

		public void TestIsParentEntryLine()
		{
			var dec = SetupJobDeclaration().JobDeclaration;

			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			var fec = SetupFECChallenge(entryLine.PK, "JI_SuppUQ", "ML");
			AssertEquals(true, fec.IsParentEntryLine);
			fec.CY_ParentID = ZGuid.Empty;
			AssertEquals(false, fec.IsParentEntryLine);
			AssertEquals(false, SetupFECChallenge(entryHeader.PK, "JE_FLG", "CN", "CH").IsParentEntryLine);
		}

		public void TestDefaultValue()
		{
			var dec = SetupJobDeclaration().JobDeclaration;
			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			AssertEquals("FEC", SetupFECChallenge(entryHeader.PK, "JE_FLG", "CN", "CH").CY_Type);
		}

		public void TestIsParentEntryHeader()
		{
			var dec = SetupJobDeclaration().JobDeclaration;
			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			var fec = SetupFECChallenge(entryHeader.PK, "JE_FLG", "CN", "CH");

			AssertEquals(true, fec.IsParentEntryHeader);
			fec.CY_ParentID = ZGuid.Empty;
			AssertEquals(false, fec.IsParentEntryHeader);
			AssertEquals(false, SetupFECChallenge(entryLine.PK, "JI_SuppUQ", "ML").IsParentEntryHeader);
		}

		public void TestLookupsAndValidation()
		{
			var dec = SetupJobDeclaration().JobDeclaration;
			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			var fec = SetupFECChallenge(entryHeader.PK, "JE_FLG", "CN", "CH");

			AssertEquals(typeof(FECChallengeLookups), fec.Lookups.GetType());
			AssertEquals(typeof(FECChallengeValidation), fec.Validation.GetType());
		}

		public void TestErrorReportWhenSavingEmptyCusCodeData()
		{
			var factory = new BusinessObjectFactory();
			var fecChallenge = factory.New<FECChallenge>();

			AssertExceptionThrown<ZSaveException>(() => factory.Save());

			var errorMsg = ErrorReporter.LastMessageReported;
			AssertStartsWith("Should report stack trace of creating", "Saving an empty FECChallenge\r\nSaving stack trace:", errorMsg);
			AssertContains("Parent ID: ", errorMsg);
			AssertContains("Code: ", errorMsg);
			AssertContains("IsDeleted: ", errorMsg);
			AssertContains("SetDefaultValues stack trace:", errorMsg);
			ErrorReporter.Clear();
		}

		protected override IEnumerable<FECChallenge> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var header = factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			var headerFEC = factory.New<FECChallenge>();
			headerFEC.CY_ParentID = header.PK;
			headerFEC.CY_ParentTableCode = header.TablePrefix;
			headerFEC.CY_Code = "JE_FLG";
			headerFEC.CY_Data = "CN";
			headerFEC.CY_Order = 1;
			headerFEC.CY_IsOverridden = false;
			yield return headerFEC;
			var line = header.MergedLines.AddNew();
			var lineFEC = factory.New<FECChallenge>();
			lineFEC.CY_ParentID = line.PK;
			lineFEC.CY_ParentTableCode = line.TablePrefix;
			lineFEC.CY_Code = "JE_DSP";
			lineFEC.CY_Data = "CNDSP";
			lineFEC.CY_Order = 1;
			lineFEC.CY_IsOverridden = false;
			yield return lineFEC;
		}

		FECChallenge SetupFECChallenge(ZGuid pk, string code, string data, string parentTableCode = "CL", ZShort order = default, bool isOverriden = false)
		{
			var fec = Factory.New<FECChallenge>();
			fec.CY_ParentID = pk;
			fec.CY_ParentTableCode = parentTableCode;
			fec.CY_Code = code;
			fec.CY_Data = data;
			fec.CY_Order = order == default ? new ZShort(order) : order;
			fec.CY_IsOverridden = isOverriden;

			return fec;
		}

		public void TestNewValueAndNewValueDecimal()
		{
			var tuple = SetupJobDeclaration();
			var dec = tuple.JobDeclaration;
			var jobComInvoiceLine = tuple.JobComInvoiceLine;

			var invoiceLine = dec.InvoiceLines.AddNew();
			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();

			jobComInvoiceLine.JI_CL = entryLine.PK;
			jobComInvoiceLine.JI_CustomsUnitQty = "KGM";
			var fec = new FECChallenge[9];
			fec[0] = SetupFECChallenge(entryHeader.PK, "JE_FLG", "CN", "CH");
			fec[1] = SetupFECChallenge(entryHeader.PK, "JE_DSP", "CNDSP", "CH");
			fec[2] = SetupFECChallenge(entryHeader.PK, "JE_DST", "CNDST", "CH");
			fec[3] = SetupFECChallenge(entryLine.PK, "JI_ORG", "CN");
			fec[4] = SetupFECChallenge(entryLine.PK, "JI_NettMass", "100");
			fec[5] = SetupFECChallenge(entryLine.PK, "JI_Supp", "100");
			fec[6] = SetupFECChallenge(entryLine.PK, "JI_NettMassUQ", "KG");
			fec[7] = SetupFECChallenge(entryLine.PK, "JI_SuppUQ", "ML");
			fec[8] = SetupFECChallenge(entryLine.PK, "JI_Price", "ML");

			dec.JE_RN_NKTransportNationality = "CN";
			AssertEquals("CN", fec[0].NewValue);
			AssertEquals(0m, fec[0].NewValue_Decimal);

			dec.JE_RL_NKOrigin = "CNDSP";
			AssertEquals("CNDSP", fec[1].NewValue);
			AssertEquals(0m, fec[1].NewValue_Decimal);

			dec.JE_RL_NKFinalDestination = "CNDST";
			AssertEquals("CNDST", fec[2].NewValue);
			AssertEquals(0m, fec[2].NewValue_Decimal);

			jobComInvoiceLine.JI_CountryOfOrigin = "CN";
			AssertEquals("CN", fec[3].NewValue);
			AssertEquals(0m, fec[3].NewValue_Decimal);

			jobComInvoiceLine.JI_CustomsQuantity = 100m;
			AssertEquals("100", fec[4].NewValue);
			AssertEquals(100m, fec[4].NewValue_Decimal);

			jobComInvoiceLine.JI_CustomsSecondQuantity = 100m;
			AssertEquals("100", fec[5].NewValue);
			AssertEquals(100m, fec[5].NewValue_Decimal);

			jobComInvoiceLine.JI_CustomsUnitQty = "KG";
			AssertEquals("KG", fec[6].NewValue);
			AssertEquals(0m, fec[6].NewValue_Decimal);

			jobComInvoiceLine.JI_CustomsSecondUnitQty = "ML";
			AssertEquals("ML", fec[7].NewValue);
			AssertEquals(0m, fec[7].NewValue_Decimal);

			jobComInvoiceLine.JI_LinePrice = 100M;
			AssertEquals("100", fec[8].NewValue);
			AssertEquals(100M, fec[8].NewValue_Decimal);

			invoiceLine.JI_CL = entryLine.PK;
			entryLine.InvoiceLines.Load();
			invoiceLine.JI_CustomsQuantity = 100m;
			AssertEquals("200", fec[4].NewValue);
			AssertEquals(200m, fec[4].NewValue_Decimal);

			invoiceLine.JI_CustomsSecondQuantity = 100m;
			AssertEquals("200", fec[5].NewValue);
			AssertEquals(200m, fec[5].NewValue_Decimal);

			invoiceLine.JI_LinePrice = 100M;
			AssertEquals("200", fec[8].NewValue);
			AssertEquals(200m, fec[8].NewValue_Decimal);
		}

		(FECChallenge[] FECChallenges, JobDeclaration JobDeclaration, CusEntryLine CusEntryLine) SetupFECChallenges()
		{
			var tuple = SetupJobDeclaration();
			var dec = tuple.JobDeclaration;
			var invoiceLine = tuple.JobComInvoiceLine;

			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();

			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			var fec = new FECChallenge[9];
			fec[0] = SetupFECChallenge(entryHeader.PK, "JE_FLG", "CN", "CH");
			fec[1] = SetupFECChallenge(entryHeader.PK, "JE_DSP", "CNDSP", "CH");
			fec[2] = SetupFECChallenge(entryHeader.PK, "JE_DST", "CNDST", "CH");
			fec[3] = SetupFECChallenge(entryLine.PK, "JI_ORG", "CN");
			fec[4] = SetupFECChallenge(entryLine.PK, "JI_NettMass", "100");
			fec[5] = SetupFECChallenge(entryLine.PK, "JI_Supp", "100");
			fec[6] = SetupFECChallenge(entryLine.PK, "JI_NettMassUQ", "KG");
			fec[7] = SetupFECChallenge(entryLine.PK, "JI_SuppUQ", "ML");
			fec[8] = SetupFECChallenge(entryLine.PK, "JI_Price", "ML");

			return (fec, dec, entryLine);
		}

		public void TestNewValueFieldType()
		{
			var tuple = SetupFECChallenges();
			var fec = tuple.FECChallenges;

			AssertEquals(nameof(FieldType.TextCodeFindBox), fec[0].NewValueFieldType);
			AssertEquals(nameof(FieldType.TextCodeFindBox), fec[1].NewValueFieldType);
			AssertEquals(nameof(FieldType.TextCodeFindBox), fec[2].NewValueFieldType);
			AssertEquals(nameof(FieldType.TextCodeFindBox), fec[3].NewValueFieldType);
			AssertEquals(nameof(FieldType.Decimal), fec[4].NewValueFieldType);
			AssertEquals(nameof(FieldType.Decimal), fec[5].NewValueFieldType);
			AssertEquals(nameof(FieldType.TextDropEdit), fec[6].NewValueFieldType);
			AssertEquals(nameof(FieldType.TextDropEdit), fec[7].NewValueFieldType);
			AssertEquals(nameof(FieldType.Decimal), fec[8].NewValueFieldType);
		}

		public void TestNewValueDisabled()
		{
			var tuple = SetupFECChallenges();
			var fec = tuple.FECChallenges;
			var dec = tuple.JobDeclaration;
			var entryLine = tuple.CusEntryLine;
			var invoiceLine = dec.InvoiceLines.AddNew();

			// First assert that values are as expected, then set IsOverridden value for next assert.
			for (int i = 0; i < fec.Length; i++)
			{
				// Assert that new value is not disabled, except for item 6.
				if (i == 6)
				{
					AssertEquals(true, fec[i].IsNewValueDisabled);
				}
				else
				{
					AssertEquals(false, fec[i].IsNewValueDisabled);
				}

				// Set IsOverridden to true.
				fec[i].CY_IsOverridden = true;

				// Assert that all challenges have their new values disabled.
				AssertEquals(true, fec[i].IsNewValueDisabled);

				// Set all IsOverridden to false.
				fec[i].CY_IsOverridden = false;
			}

			invoiceLine.JI_CL = entryLine.PK;

			for (int i = 0; i < fec.Length; i++)
			{
				if (i == 6)
				{
					AssertEquals(true, fec[i].IsNewValueDisabled);
				}
				else
				{
					AssertEquals(false, fec[i].IsNewValueDisabled);
				}
			}
		}

		public void TestRelatedObjectName()
		{
			var fec = Factory.New<FECChallenge>();
			AssertEquals(ZString.Empty, fec.RelatedObjectName);
		}

		public void TestCY_IsOverridden()
		{
			var dec = SetupJobDeclaration().JobDeclaration;

			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();

			var fec1 = SetupFECChallenge(entryHeader.PK, "JE_FLG", "CN", "CH");
			var fec2 = SetupFECChallenge(entryLine.PK, "JI_Price", "ML");

			((CusEntryHeader)fec1.Parent).Declaration.JE_ApplicationCode = "CDS";
			Assert("CY_IsOverridden should be readonly for CDS Jobs.", fec1.CY_IsOverriddenInfo.ReadOnly);

			Assert("CY_IsOverridden should be readonly for CDS Jobs.", fec2.CY_IsOverriddenInfo.ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObject(Factory);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			return GetNewBusinessObject(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject(factory);
		}

		protected BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_RouteFRequested = true;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var dsp = entryHeader.FECChallenges.AddNew();
			dsp.CY_Code = FECChallengeFields.Codes.JE_DSP;
			dsp.CY_ParentTableCode = "CH";
			dsp.CY_ParentID = entryHeader.PK;
			dsp.CY_IsOverridden = true;
			return dsp;
		}
	}

	sealed class JobComInvoiceLineForTesting : JobComInvoiceLine
	{
		public JobComInvoiceLineForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public TariffView UniversalTariffReturns { get; set; }
		public bool UseUniversalTariffCoreReturns { get; set; }

		public override TariffView UniversalTariff => UniversalTariffReturns;

		protected override bool UseUniversalTariffCore => UseUniversalTariffCoreReturns;
	}
}
