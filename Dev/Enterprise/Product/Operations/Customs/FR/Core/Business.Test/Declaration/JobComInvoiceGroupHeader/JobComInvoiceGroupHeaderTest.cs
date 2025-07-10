using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.FR.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using CustomsChargeTypeList = Enterprise.Customs.Business.CustomsChargeTypeList;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceGroupHeader))]
	class JobComInvoiceGroupHeaderTest : EU.Business.Declaration.Testing.JobComInvoiceGroupHeaderTest
	{
		public void TestExportCreatedChargeIsIncludedInLineFlag()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = "SEA";
			var invoice = declaration.Invoices.AddNew();
			invoice.ZG_AgreedPlaceCode = "1";
			invoice.JZ_IncoTerm = "EXW";
			AssertEquals("There should be 6 group charges created", 2, declaration.TopGroupInvoice.Charges.Count);
			AssertEquals("First charge to create should be of type FRE", "FRE", declaration.TopGroupInvoice.Charges[0].ChargeCode.Code);
			AssertEquals("J7_IsIncludedInITOT should be false for FRE charge", false, declaration.TopGroupInvoice.Charges[0].J7_IsIncludedInITOT);
			AssertEquals("Second charge to create should be of type FNE", "FNE", declaration.TopGroupInvoice.Charges[1].ChargeCode.Code);
			AssertEquals("J7_IsIncludedInITOT should be false for FNE charge", false, declaration.TopGroupInvoice.Charges[1].J7_IsIncludedInITOT);
		}

		public void TestImportCreatedChargeIsIncludedInLineFlag()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = "SEA";
			declaration.JE_AirRouteType = "";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = "CIP";
			invoice.ZG_AgreedPlaceCode = "1";

			AssertEquals("There should be 6 group charges created", 6, declaration.TopGroupInvoice.Charges.Count);
			AssertEquals("First charge to create should be of type CEI", "CEI", declaration.TopGroupInvoice.Charges[0].ChargeCode.Code);
			AssertEquals("J7_IsIncludedInITOT should be true for CEI charge", true, declaration.TopGroupInvoice.Charges[0].J7_IsIncludedInITOT);
			AssertEquals("Second charge to create should be of type CNI", "CNI", declaration.TopGroupInvoice.Charges[1].ChargeCode.Code);
			AssertEquals("J7_IsIncludedInITOT should be true for CNI charge", true, declaration.TopGroupInvoice.Charges[1].J7_IsIncludedInITOT);
			AssertEquals("Third charge to create should be of type FRI", "FRI", declaration.TopGroupInvoice.Charges[2].ChargeCode.Code);
			AssertEquals("J7_IsIncludedInITOT should be true for FRI charge", true, declaration.TopGroupInvoice.Charges[2].J7_IsIncludedInITOT);
			AssertEquals("Fourth charge to create should be of type FNI", "FNI", declaration.TopGroupInvoice.Charges[3].ChargeCode.Code);
			AssertEquals("J7_IsIncludedInITOT should be true for FNI charge", true, declaration.TopGroupInvoice.Charges[3].J7_IsIncludedInITOT);
			AssertEquals("Fifth charge to create should be of type FRE", "FRE", declaration.TopGroupInvoice.Charges[4].ChargeCode.Code);
			AssertEquals("J7_IsIncludedInITOT should be false for FRE charge", false, declaration.TopGroupInvoice.Charges[4].J7_IsIncludedInITOT);
			AssertEquals("Sixth charge to create should be of type FNE", "FNE", declaration.TopGroupInvoice.Charges[5].ChargeCode.Code);
			AssertEquals("J7_IsIncludedInITOT should be false for FNE charge", false, declaration.TopGroupInvoice.Charges[5].J7_IsIncludedInITOT);
		}

		public void TestGetStandaloneIncoTermAndChargeFactoryCountryContext()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var groupHeader = Factory.New<JobComInvoiceGroupHeaderForTest>();
				AssertEquals("Export", "FREXP", groupHeader.GetStandaloneIncoTermAndChargeFactoryCountryContext());

				groupHeader.JZ_JE = declaration.PK;
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertEquals("Import", "FR", groupHeader.GetStandaloneIncoTermAndChargeFactoryCountryContext());

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				AssertEquals("Export", "FREXP", groupHeader.GetStandaloneIncoTermAndChargeFactoryCountryContext());
			});
		}

		public void TestChargesPropertiesForExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			var config = XMLExtractor.GetIncoTermsConfiguration("Enterprise.Customs.FR.Business.Declaration.Valuation.ExportIncoTermsConfiguration.xml");

			TestChargesProperties(declaration, config);
		}

		public void TestChargesPropertiesForUCC6Export()
		{
			using (FRCustomsDataRegistry.Instance.EnableDeltaIEForExports.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				var config = XMLExtractor.GetIncoTermsConfiguration("Enterprise.Customs.FR.Business.Declaration.Valuation.ExportIncoTermsConfiguration.xml");

				TestChargesProperties(declaration, config);
			}
		}

		public void TestChargesPropertiesForImport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var config = XMLExtractor.GetIncoTermsConfiguration("Enterprise.Customs.FR.Business.Declaration.Valuation.ImportIncoTermsConfiguration.xml");

			TestChargesProperties(declaration, config);
		}

		public void TestChargesPropertiesForUCC6Import()
		{
			using (FRCustomsDataRegistry.Instance.EnableDeltaIEForImports.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				var config = XMLExtractor.GetIncoTermsConfiguration("Enterprise.Customs.FR.Business.Declaration.Valuation.ImportIncoTermsConfiguration.xml");

				TestChargesProperties(declaration, config);
			}
		}

		public void TestChargesProperties(JobDeclaration declaration, IncoTermsConfiguration config)
		{
			var invoice = declaration.Invoices.AddNew();
			foreach (var mapping in config.Mapping)
			{
				foreach (var transport in mapping.Filter.TransportModes.TransportMode)
				{
					declaration.JE_TransportMode = transport;

					if (mapping.Filter.AirRouteTypes != null)
					{
						foreach (var airRouteType in mapping.Filter.AirRouteTypes.AirRouteType)
						{
							declaration.JE_AirRouteType = airRouteType;
							AssertChargesAreMatching(declaration, invoice, mapping.Filter, mapping.Charges.Charge, declaration.TopGroupInvoice.Charges);
						}
					}
					else
					{
						declaration.JE_AirRouteType = "";
						AssertChargesAreMatching(declaration, invoice, mapping.Filter, mapping.Charges.Charge, declaration.TopGroupInvoice.Charges);
					}
				}
			}
		}

		public override void TestChargesToImportForLandedCosting()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_TransportMode = Core.Constants.TransportModes.Air;
			var groupHeader = dec.JobComInvoiceGroupHeaders[0];
			var fobInvoice = groupHeader.JobComInvoiceHeaders.AddNew();
			fobInvoice.JZ_IncoTerm = "FOB";
			fobInvoice.JZ_InvoiceAmount = 10000m;
			fobInvoice.JZ_RX_NKInvoice_Currency = dec.LocalCurrencyCode;

			var groupCharges = groupHeader.Charges;
			groupCharges.AddNew(OverseasFreightCode, 1000m, dec.LocalCurrencyCode).J7_DistributeBy = Common.ChargeDistributeByList.Codes.Value;
			var deductionCharge = groupCharges.AddNew(CustomsChargeTypeList.Codes.DeductionCharge, 100m, dec.LocalCurrencyCode);
			deductionCharge.J7_DistributeBy = Common.ChargeDistributeByList.Codes.Value;
			PerformExtraDeductionChargeInitialisation(deductionCharge);
			var groupCommission = groupCharges.AddNew(CustomsChargeTypeList.Codes.Commission, 100m, dec.LocalCurrencyCode);
			groupCommission.J7_DistributeBy = Common.ChargeDistributeByList.Codes.Value;

			dec.ResumeApportionment();
			AssertEquals("FOB Invoice has three charges apportioned", 3, fobInvoice.GroupCharges.Count);
			AssertEquals("First row is OFT", OverseasFreightCode, fobInvoice.GroupCharges[0].J7_ChargeType);
			AssertEquals("Apportioned OFT not included in lines", false, fobInvoice.GroupCharges[0].J7_IsIncludedInITOT);

			AssertEquals("Second row is DED", CustomsChargeTypeList.Codes.DeductionCharge, fobInvoice.GroupCharges[1].J7_ChargeType);
			AssertEquals("Apportioned DED included in lines", true, fobInvoice.GroupCharges[1].J7_IsIncludedInITOT);

			groupCommission.J7_IsIncludedInITOT = true;
			dec.ResumeApportionment();

			var result = new List<IDefaultLandedCostInput>(((ILandedCostChargeHolder)groupHeader).ChargesToImportForLandedCosting).ToArray();
			AssertEquals("two charges in Result", 2, result.Length);
			AssertEquals("Overseas freight code", true, result[0].ChargeDescription.Contains(OverseasFreightDescription.ToUpper()));
			AssertEquals("DED should be brought", true, result[1].ChargeDescription.Contains("Deduction (or Discount) from Entry"));
			AssertEquals("DED should be brought as a negative amount", -100m, result[1].AmountToDistribute.Amount);
			AssertEquals("OTH charge should not be brought as this is included in lines", false, result[1].ChargeDescription.Contains(CustomsChargeTypeList.Descriptions.Commission.ToString().ToUpper()));
		}

		void AssertChargesAreMatching(JobDeclaration declaration, JobComInvoiceHeader invoice, Filter filter, Charge[] expectedCharges, GroupInvoiceChargeCollection<GroupInvoiceCharge> actualCharges)
		{
			invoice.JZ_IncoTerm = filter.IncoTerm;
			SetAgreedPlaceCode(filter.AgreedPlace);

			var message = "IncoTerm : " + invoice.JZ_IncoTerm + ", AgreedPlaceCode : " + invoice.ZG_AgreedPlaceCode + ", TransportMode : " + declaration.JE_TransportMode + ", AirRouteType : " + declaration.JE_AirRouteType;

			var expectedChargesKeys = new HashSet<MessageChargeKey>();
			foreach (var expectedCharge in expectedCharges)
			{
				expectedChargesKeys.Add(new MessageChargeKey(expectedCharge.ChargeType, expectedCharge.IsDutiable, expectedCharge.IsGSTApplicable, expectedCharge.IsIncludedInInvoice, expectedCharge.IsStatisticalValueApplicable));
			}

			var actualChargesKeys = new HashSet<MessageChargeKey>();
			foreach (BaseGroupInvoiceCharge actualCharge in actualCharges)
			{
				actualChargesKeys.Add(new MessageChargeKey(actualCharge.J7_ChargeType, actualCharge.J7_IsDutiable, actualCharge.J7_IsGSTApplicable, actualCharge.J7_IsIncludedInITOT, actualCharge.J7_IsStatisticalValueApplicable));
			}

			AssertContainsElementsInAnyOrder(message, expectedChargesKeys, actualChargesKeys);

			invoice.JZ_IncoTerm = "";
			invoice.ZG_AgreedPlaceCode = "";

			void SetAgreedPlaceCode(string agreedPlaceCode)
			{
				switch (declaration.JE_ApplicationCode)
				{
					case DeclarationApplicationCodeList.Codes.DeltaG:
						invoice.ZG_AgreedPlaceCode = agreedPlaceCode;
						break;
					case DeclarationApplicationCodeList.Codes.DeltaIE:
						switch (agreedPlaceCode)
						{
							case FRConstants.IncoTermKeys.ThisMemberState:
								invoice.ZG_AgreedPlaceCode = "FRLEH";
								break;
							case FRConstants.IncoTermKeys.AnotherMemberState:
								invoice.ZG_AgreedPlaceCode = "DEHAM";
								break;
							case FRConstants.IncoTermKeys.OutsideUnion:
								invoice.ZG_AgreedPlaceCode = "USNYC";
								break;
						}
						break;
				}
			}

			void AssertContainsElementsInAnyOrder<T>(string message, IEnumerable<T> expected, IEnumerable<T> actual)
			{
				var actualSet = new HashSet<T>(actual);
				foreach (var expectedElement in expected)
				{
					Assert("{message}: {expectedElement} was not found in the actual collection.", actualSet.Contains(expectedElement));
				}
			}
		}

		protected override void PerformExtraDeductionChargeInitialisation(BaseGroupInvoiceCharge charge)
		{
			Assert(!charge.J7_Calc_IsIncludedInITOT_ReadOnly);
			charge.J7_IsIncludedInITOT = true;
		}

		public void TestUpdateCharges()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = "SEA";
			var invoice = declaration.Invoices.AddNew();
			invoice.ZG_AgreedPlaceCode = "1";
			invoice.JZ_IncoTerm = "EXW";
			AssertEquals("There should be 2 group charges created", 2, declaration.TopGroupInvoice.Charges.Count);

			var nonZeroCharge = declaration.TopGroupInvoice.Charges.AddNew();
			nonZeroCharge.J7_ChargeType = "ABC";
			nonZeroCharge.J7_Amount = 100;

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			invoice.JZ_IncoTerm = "CIP";
			invoice.ZG_AgreedPlaceCode = "1";
			AssertEquals("There should be 7 group charges created including one non-zero charge", 7, declaration.TopGroupInvoice.Charges.Count);
			AssertEquals("Non-zero charge should be present after updating charges", true, declaration.TopGroupInvoice.Charges.Contains(nonZeroCharge));

			invoice.JZ_IncoTerm = "EXW";
			AssertEquals("There should be 7 group charges created including one non-zero charge", 7, declaration.TopGroupInvoice.Charges.Count);
			AssertEquals("Non-zero charge should be present after updating charges", true, declaration.TopGroupInvoice.Charges.Contains(nonZeroCharge));
		}

		public void TestUpdateCharges_RequiredChargesShouldNotBeDeletedDuringUpdating()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = "SEA";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = "CIP";
			invoice.ZG_AgreedPlaceCode = "1";
			AssertEquals("There should be 6 group charges created as configured.", 6, declaration.TopGroupInvoice.Charges.Count);
			var firstCharge = declaration.TopGroupInvoice.Charges[0];
			AssertEquals("Prerequisite: Amount of first charge is 0 by default.", 0m, firstCharge.J7_Amount);

			declaration.TopGroupInvoice.UpdateCharges();
			AssertEquals("Required charges should not be deleted even if the amount is 0.", false, firstCharge.IsDeleted);
		}

		[ExpectNoExceptions]
		public void TestUpdateChargesNoTypeCastException()
		{
			foreach (var countryCode in Core.Constants.CountryCodes.FranceAndOverseasDepartmentsAndTerritories)
			{
				UpdateChargesNoTypeCastException(countryCode);
			}
		}

		void UpdateChargesNoTypeCastException(string countryCode)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var baseDeclaration = Factory.New<BaseJobDeclaration>();
				if (baseDeclaration is JobDeclaration declaration)
				{
					declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
					declaration.JE_TransportMode = "SEA";
					_ = declaration.Invoices.AddNew();
					var topGroupInvoice = declaration.TopGroupInvoice;
					topGroupInvoice.UpdateCharges();
				}
			}
		}

		protected override Type ExpectedTypeOfCharges => typeof(GroupInvoiceChargeCollection<GroupInvoiceCharge>);
		protected override ZString DistributedByForApportionDefaultValue => ChargeDistributeByList.Codes.Value;
		protected override ZBool DistributeByShouldBeChangedForApportion => true;

		#region ChargeType

		protected override CodeDescriptionPairList GetExpectedCustomsChargeTypeList()
		{
			var customsChargeTypeList = new FRCustomsChargeTypeList();
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.Additions71Charge);
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.CommissionExceptBuyingCommissionsCharge);
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.ConstructionErectionAssemblyCharge);
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.ContainersAndPackingCharge);
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.Deductions71Charge);
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.DiscountNotElsewhereDeclaredCharge);
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge);
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.IndirectAndOtherPaymentsCharge);
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.ProceedsOfAnySubsequentResaleCharge);
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.RightToReproduceCharge);
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge);
			customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.ToolsMiesMouldsCharge);
			customsChargeTypeList.Sort();
			return customsChargeTypeList;
		}

		protected override string OverseasFreightCode => UCCCustomsChargeTypeList.Codes.TransportCostsCharge;
		protected override string OverseasFreightDescription => UCCCustomsChargeTypeList.Descriptions.TransportCostsCharge;
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				var testDec = factory.New<JobDeclaration>();
				testDec.MergeManager.DisablePreSaveMergeRequirementForTesting();
				var groupHeader = testDec.JobComInvoiceGroupHeaders[0];
				groupHeader.JobComInvoiceHeaders.AddNew();
				groupHeader.Charges.AddNew();
				return groupHeader;
			}
		}
		#endregion
	}

	class JobComInvoiceGroupHeaderForTest : JobComInvoiceGroupHeader
	{
		public JobComInvoiceGroupHeaderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new string GetStandaloneIncoTermAndChargeFactoryCountryContext() => base.GetStandaloneIncoTermAndChargeFactoryCountryContext();
	}
}
