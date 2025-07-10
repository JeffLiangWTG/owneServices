using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(CommonGuarantee))]
	public class CommonGuaranteeTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => factory.New<JobDeclaration>().Guarantees.AddNew();

		public void TestPW_HolderIdentificationDefault()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
			{
				var declarant = Factory.New<OrgHeader>();
				declarant.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "DEC22222222");

				var (guarantee1, importer) = CreateCusGuaranteeHeaderWithPartyHavingEori();
				var (guarantee2, _) = CreateCusGuaranteeHeaderWithPartyHavingNoEori();

				var declaration = Factory.New<JobDeclaration>();
				var guarantee = declaration.Guarantees.AddNew();
				declaration.JE_OH_Importer = importer.PK;
				declaration.Declarant.OA_OH = declarant.PK;

				AssertNullOrEmpty(guarantee.PW_HolderIdentification);
				guarantee.PW_HolderIdentification = "DEC22222222";
				AssertEquals("DEC22222222", guarantee.PW_HolderIdentification);

				guarantee.PW_Password = "Y";
				guarantee.PW_BondNumber = guarantee1.CPH_Number;
				AssertNullOrEmpty(guarantee.PW_BondType);
				AssertNullOrEmpty(guarantee.PW_Password);
				AssertEquals("ESIMP11111111", guarantee.PW_HolderIdentification);

				guarantee.PW_BondNumber = guarantee.PW_HolderIdentification;
				guarantee.PW_BondType = "G";
				guarantee.PW_Password = "Y";
				guarantee.PW_BondNumber = guarantee1.CPH_Number;
				AssertEquals("ESIMP11111111", guarantee.PW_HolderIdentification);
				AssertEquals("G", guarantee.PW_BondType);
				AssertNullOrEmpty(guarantee.PW_Password);

				guarantee1.CPH_SubType = "ACO";
				guarantee.PW_BondNumber = guarantee.PW_HolderIdentification;
				guarantee.PW_BondType = "G";
				guarantee.PW_Password = "Y";
				guarantee.PW_BondNumber = guarantee1.CPH_Number;
				AssertEquals("ESIMP11111111", guarantee.PW_HolderIdentification);
				AssertEquals(guarantee.PW_BondType, "ACO");
				AssertNullOrEmpty(guarantee.PW_Password);

				guarantee.PW_BondNumber = guarantee2.CPH_Number;
				guarantee.PW_BondType = string.Empty;
				AssertNullOrEmpty("Empty if guarantee do not have an EOR Number", guarantee.PW_HolderIdentification);
			}
		}

		public void TestHolderIdentification_OnBondNumber2Change()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				var (guarantee1, _) = CreateCusGuaranteeHeaderWithPartyHavingEori();
				var (guarantee2, _) = CreateCusGuaranteeHeaderWithPartyHavingNoEori();

				var bo = Factory.New<DeclarationWithCustomGuaranteeForTest>();
				var guarantee = bo.Guarantees.AddNew();

				AssertNullOrEmpty("PW_HolderIdentification", guarantee.PW_HolderIdentification);
				guarantee.PW_HolderIdentification = "IT12345";
				AssertEquals("PW_HolderIdentification", "IT12345", guarantee.PW_HolderIdentification);

				guarantee.PW_Password = "Y";
				guarantee.PW_BondNumber2 = guarantee1.CPH_Number;
				AssertNullOrEmpty("PW_BondType", guarantee.PW_BondType);
				AssertNullOrEmpty("PW_Password", guarantee.PW_Password);
				AssertEquals("PW_HolderIdentification", "ITIMP11111111", guarantee.PW_HolderIdentification);

				guarantee.PW_BondNumber2 = guarantee.PW_HolderIdentification;
				guarantee.PW_BondType = "G";
				guarantee.PW_Password = "Y";
				guarantee.PW_BondNumber2 = guarantee1.CPH_Number;
				AssertEquals("PW_HolderIdentification", "ITIMP11111111", guarantee.PW_HolderIdentification);
				AssertEquals("PW_BondType", "G", guarantee.PW_BondType);
				AssertNullOrEmpty("PW_BondType", guarantee.PW_Password);

				guarantee1.CPH_SubType = "ACO";
				guarantee.PW_BondNumber2 = guarantee.PW_HolderIdentification;
				guarantee.PW_BondType = "G";
				guarantee.PW_Password = "Y";
				guarantee.PW_BondNumber2 = guarantee1.CPH_Number;
				AssertEquals("PW_HolderIdentification", "ITIMP11111111", guarantee.PW_HolderIdentification);
				AssertEquals("PW_BondType", "ACO", guarantee.PW_BondType);
				AssertNullOrEmpty("PW_Password", guarantee.PW_Password);

				guarantee.PW_BondNumber2 = guarantee2.CPH_Number;
				guarantee.PW_BondType = string.Empty;
				AssertNullOrEmpty("Empty if guarantee do not have an EOR Number", guarantee.PW_HolderIdentification);
			}
		}

		public void TestGetWarningErrorIfRemainingBalanceIsNotEnough()
		{
			var guarantee = Factory.New<CommonGuarantee>();
			var (cusGuarantee, _) = CreateCusGuaranteeHeaderWithPartyHavingEori();
			guarantee.PW_CPH_Guarantee = cusGuarantee.PK;

			guarantee.PW_BondNumber = cusGuarantee.CPH_Number;
			cusGuarantee.AddTransaction("OPENING", "OPENING", ZString.Empty, ZString.Empty, 1000.0m, 0, transactionType: Customs.Business.PermitTransactionTypeList.Codes.OBL, isAggregated: true);

			CombineAssertions(() =>
			{
				AssertEquals("[PreReq] CPH_Calc_TotalBalanceIncludingPendingDecimal", 1000.0m, cusGuarantee.CPH_Calc_TotalBalanceIncludingPendingDecimal);

				guarantee.PW_BondAmount = 10.0m;
				var expectedMessage = guarantee.GetWarningErrorIfRemainingBalanceIsNotEnough();
				AssertEquals("If there is enough balance then empty", ZString.Empty, expectedMessage);

				guarantee.PW_BondAmount = 10000.5m;
				expectedMessage = guarantee.GetWarningErrorIfRemainingBalanceIsNotEnough();
				AssertEquals("If there is not enough balance then Message is created", "Guarantee Nº (G1) has not enough remaining balance (1000.00) to create the guarantee transaction (10000.50). It will be created anyway.", expectedMessage);
			});
		}

		public virtual void TestLookups()
		{
			var guarantee = Factory.New<CommonGuarantee>();
			AssertType<CommonGuaranteeLookups>(guarantee.Lookups);
		}

		public void TestCusGuarantee()
		{
			var guarantee = Factory.New<CommonGuarantee>();
			CombineAssertions(() =>
			{
				AssertNull("Default", guarantee.CusGuarantee);

				var cusGuarantee = CreateCusGuarantee();
				guarantee.PW_CPH_Guarantee = cusGuarantee.PK;
				AssertEquals("Not null", cusGuarantee, guarantee.CusGuarantee);

				guarantee.CusGuarantee.Delete();
				AssertNull("Deleted", guarantee.CusGuarantee);
			});
		}

		CusGuaranteeHeader CreateCusGuarantee()
		{
			var permitHolder = Factory.New<OrgHeader>();
			permitHolder.OH_Code = "AB1";
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_OH_PermitHolder = permitHolder.PK;
			guaranteeHeader.CPH_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			guaranteeHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			guaranteeHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
			guaranteeHeader.CPH_EndDate = ZDate.Today.AddDays(1);
			return guaranteeHeader;
		}

		(CusGuaranteeHeader, OrgHeader) CreateCusGuaranteeHeaderWithPartyHavingEori()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "IMP11111111");

			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Type = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			guaranteeHeader.CPH_Number = "G1";
			guaranteeHeader.CPH_OH_PermitHolder = orgHeader.PK;

			return (guaranteeHeader, orgHeader);
		}

		(CusGuaranteeHeader, OrgHeader) CreateCusGuaranteeHeaderWithPartyHavingNoEori()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.ConsigneeExemptNo, "CEN12343");

			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Type = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			guaranteeHeader.CPH_Number = "G2";
			guaranteeHeader.CPH_OH_PermitHolder = orgHeader.PK;

			return (guaranteeHeader, orgHeader);
		}

		#region DeclarationWithCustomGuaranteeForTest

		sealed class DeclarationWithCustomGuaranteeForTest : JobDeclaration
		{
			public DeclarationWithCustomGuaranteeForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override GuaranteeForDeclarationCollection GetGuaranteesCore() => new GuaranteeCollecGuaranteeForDeclarationCollectionForTest(this);
		}

		sealed class GuaranteeCollecGuaranteeForDeclarationCollectionForTest : GuaranteeForDeclarationCollection
		{
			public GuaranteeCollecGuaranteeForDeclarationCollectionForTest(JobDeclaration declaration) : base(declaration)
			{
			}

			public new GuaranteeForTest this[int index] => (GuaranteeForTest)base[index];

			public new GuaranteeForTest AddNew() => (GuaranteeForTest)base.AddNew();
		}

		sealed class GuaranteeForTest : GuaranteeForDeclaration
		{
			public GuaranteeForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override ZBool ShouldSetupHolderIdentificationOnBondNumber2Change => true;

			protected override ZBool ShouldSetupHolderIdentificationOnBondNumberChange => false;
		}

		#endregion
	}
}
