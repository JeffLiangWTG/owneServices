using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Registry;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(JobDeclarationCustomsOfficeRequirementHelper))]
	class JobDeclarationCustomsOfficeRequirementHelperTest : EU.Business.Declaration.Testing.JobDeclarationCustomsOfficeRequirementHelperAbstractTest<JobDeclarationCustomsOfficeRequirementHelper>
	{
		public void TestCheckOfficeForFallback()
		{
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_CustomsQuantity = 10;
			invoiceLine1.JI_Weight = 5;
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CustomsQuantity = 10;
			invoiceLine2.JI_Weight = 5;

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();

			cusEntryHeader.CH_BGMReference = "19212081311";

			var cusEntryLine = cusEntryHeader.AllEntryLines.AddNew();
			invoiceLine1.JI_CL = cusEntryLine.PK;
			invoiceLine2.JI_CL = cusEntryLine.PK;

			var fallbackSetting = new FallbackSettings();
			fallbackSetting.End = ZDateTime.Today.AddDays(1);
			fallbackSetting.Start = ZDateTime.Today.AddDays(-1);
			FRCustomsDataRegistry.Instance.DeltaGMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting);

			declaration.CustomsOffices.RemoveAndDeleteAll();

			var requirementHelper = declaration.CustomsOfficeRequirementHelper;
			var errors = requirementHelper.Validate();

			Assert(errors.ToList().Contains("An office of type CAU is needed"));
			Assert(!errors.ToList().Contains("This office of type CAU must have an email address"));

			var newOfficeOfDeclaration = declaration.CustomsOffices.AddNew();
			newOfficeOfDeclaration.CY_Code = EuOfficeCodesTypes.Codes.CompetentAuthorityCountryOfDep;
			newOfficeOfDeclaration.CY_Data = "test";
			newOfficeOfDeclaration.CY_Date = ZDateTime.Today;

			errors = requirementHelper.Validate();

			Assert(!errors.ToList().Contains("An office of type CAU is needed"));
			Assert(errors.ToList().Contains("The office of type CAU must have an email address"));

			var cusCodeList = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList.ZZD_Code = newOfficeOfDeclaration.CY_Data;
			cusCodeList.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
			cusCodeList.ZZD_CountryOrGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			cusCodeList.ZZD_EndDate = ZDateTime.Today.AddDays(2);
			cusCodeList.ZZD_StartDate = ZDateTime.Today.AddDays(-2);

			var officeAttribute = cusCodeList.Attributes.AddNew();
			officeAttribute.ZZE_Value = "email.email@email.com";
			officeAttribute.ZZE_ZXE_NKName = "EmailAddress";

			Factory.Save();

			errors = requirementHelper.Validate();

			Assert(!errors.ToList().Contains("The office of type CAU must have an email address"));
		}

		public override void TestMainOffice_Import()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			AssertCustomsOfficeRequirementEquals("Main office should be an office of Lodgement for Import non UCC declaration.", new CustomsOfficeRequirement("", true, true, "Office of Lodgement"), officeHelper.MainOffice);

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertCustomsOfficeRequirementEquals("Main office should be an office of Presentation for Import UCC declaration.", new CustomsOfficeRequirement("", true, true, "Office of Presentation"), officeHelper.MainOffice);
		}

		public override void TestMainOffice_Export()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			AssertCustomsOfficeRequirementEquals("Export", new CustomsOfficeRequirement("", true, true, "Office of Lodgement"), officeHelper.MainOffice);
		}

		public override void TestMainOffice_Miscellaneous()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertCustomsOfficeRequirementEquals("Misc", new CustomsOfficeRequirement("", true, false), officeHelper.MainOffice);
		}

		public override void TestOtherRequirements_Import()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			var transportTypeList = declaration.Lookups.TransportTypeList.GetAllCodes();
			foreach (var transportType in transportTypeList)
			{
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
				declaration.JE_TransportMode = transportType;
				AssertCustomsOfficeRequirementEquals("CAU Office should be required and not necessarily local in DeltaG import declaration.", new CustomsOfficeRequirement("CAU", true, false, "Office of Declaration"), GetOfficeHelperOtherRequirement("CAU"));
				AssertCustomsOfficeRequirementEquals("DES Office should be required  and not necessarily local in DeltaG declaration.", new CustomsOfficeRequirement("DES", false, false, "Office of Clearance"), GetOfficeHelperOtherRequirement("DES"));
				AssertCustomsOfficeRequirementEquals("ENT Office should not be required but local in DeltaG declaration.", new CustomsOfficeRequirement("ENT", false, true, "Office of Entry"), GetOfficeHelperOtherRequirement("ENT"));

				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
				declaration.JE_TransportMode = transportType;
				AssertCustomsOfficeRequirementEquals("CAU Office should be required and not necessarily local in DeltaIE import declaration.", new CustomsOfficeRequirement("CAU", true, false, "Supervising Customs Office"), GetOfficeHelperOtherRequirement("CAU"));

				ValidateDISOfficeRequirement("DIS OfficeRole should be required in DeltaIE import declaration when CEI_Style is H2.", DeclarationApplicationCodeList.Codes.DeltaIE, "H2", true);
				ValidateDISOfficeRequirement("DIS OfficeRole should be required in DeltaIE import declaration when CEI_Style is H3.", DeclarationApplicationCodeList.Codes.DeltaIE, "H3", true);
				ValidateDISOfficeRequirement("DIS OfficeRole should be required in DeltaIE import declaration when CEI_Style is H4.", DeclarationApplicationCodeList.Codes.DeltaIE, "H4", true);
				ValidateDISOfficeRequirement("DIS OfficeRole should not be required in DeltaIE import declaration when CEI_Style is other than H2 or H3 or H4 (Example: H1).", DeclarationApplicationCodeList.Codes.DeltaIE, "H1", false);
				ValidateDISOfficeRequirement("DIS OfficeRole should not be required in DeltaIE import declaration when CEI_Style is other than H2 or H3 or H4 (Example: H5).", DeclarationApplicationCodeList.Codes.DeltaIE, "H5", false);

				ValidateDISOfficeRequirement("DIS OfficeRole should not be required in DeltaG import declaration for any CEI_Style - H1.", DeclarationApplicationCodeList.Codes.DeltaG, "H1", false);
				ValidateDISOfficeRequirement("DIS OfficeRole should not be required in DeltaG import declaration for any CEI_Style - H2.", DeclarationApplicationCodeList.Codes.DeltaG, "H2", false);
				ValidateDISOfficeRequirement("DIS OfficeRole should not be required in DeltaG import declaration for any CEI_Style - H3.", DeclarationApplicationCodeList.Codes.DeltaG, "H3", false);
				ValidateDISOfficeRequirement("DIS OfficeRole should not be required in DeltaG import declaration for any CEI_Style - H4.", DeclarationApplicationCodeList.Codes.DeltaG, "H4", false);
				ValidateDISOfficeRequirement("DIS OfficeRole should not be required in DeltaG import declaration for any CEI_Style - H5.", DeclarationApplicationCodeList.Codes.DeltaG, "H5", false);

				void ValidateDISOfficeRequirement(ZString comment, ZString applicationCode, ZString style, bool officeRoleExists)
				{
					var officeRole = "DIS";
					declaration.JE_ApplicationCode = applicationCode;
					declaration.CustomsEntryInstructions.FirstOrDefault().CEI_Style = style;
					if (officeRoleExists)
					{
						var disRequirement = new CustomsOfficeRequirement(officeRole, true, false, "Office of Discharge")
						{
							MaxOfficeCountLimit = 99
						};
						AssertCustomsOfficeRequirementEquals(comment, disRequirement, GetOfficeHelperOtherRequirement(officeRole));
					}
					else
					{
						Assert(comment, !GetOfficeHelperOtherRequirementExists(officeRole));
					}
				}
			}
		}

		public override void TestOtherRequirements_Export()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			AssertCustomsOfficeRequirementEquals("Export CAU", new CustomsOfficeRequirement("CAU", true, false, "Office of Declaration"), GetOfficeHelperOtherRequirement("CAU"));
			AssertCustomsOfficeRequirementEquals("Export EXT", new CustomsOfficeRequirement("EXT", true, false, "Office of Exit"), GetOfficeHelperOtherRequirement("EXT"));
			AssertCustomsOfficeRequirementEquals("Export DES", new CustomsOfficeRequirement("DEP", false, false, "Office of Clearance"), GetOfficeHelperOtherRequirement("DEP"));
		}

		public override void TestOtherRequirements_Miscellaneous()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals(false, officeHelper.OtherRequirements.Any());
		}

		protected override string SetupDeclarationForCacheKey()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			return "JobDeclarationCustomsOfficeRequirementHelper,FR,IMP,AIR";
		}

		protected override EU.Business.Declaration.JobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();

		CustomsOfficeRequirement GetOfficeHelperOtherRequirement(string officeRole) => officeHelper.OtherRequirements.Single(x => x.OfficeRole == officeRole);

		bool GetOfficeHelperOtherRequirementExists(string officeRole) => officeHelper.OtherRequirements.Any(x => x.OfficeRole == officeRole);
	}
}
