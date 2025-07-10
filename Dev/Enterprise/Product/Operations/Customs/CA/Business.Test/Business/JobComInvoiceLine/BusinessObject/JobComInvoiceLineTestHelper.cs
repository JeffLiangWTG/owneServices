using System.IO;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	public static class JobComInvoiceLineTestHelper
	{
		public static BusinessObjectFactory PopulateDutiesAndTaxesRefFilesReturningFactory()
		{
			#region Ref Files Data

			const string refFilesData = @"
RA108466939090        9960
RA20846693909090020090801999999992009-06-CPFTA   NN2009-06-CPFTA2009080199999999NN
RA3084669390902009080199999999N   N2009-06-CPFTA
RA40F000000000000000000000000000V001800000000000000000000000 000000000000000000000000000 00000000000000000000000000002N
RA520012008010199999999Y1V001600000   NORMAL RATE                                                 N
RA54E901991040399999999S000007500   ALL OTHER CONDITIONALLY EXEMPT GOODS                        N
RA6084669390902009080199999999NGRMS001400000000000000000000000 000000000000000000000000000N
RA100301104567        9902
RA20030110456790020090801999999992009-06-CPFTA   NN2009-06-CPFTA2009080199999999NN
RA3003011045672009080199999999N   N2009-06-CPFTA
RA40F000000000000000000000000000V001800000000000000000000000 000000000000000000000000000 00000000000000000000000000002N
RA520012008010199999999Y1V001600000   NORMAL RATE                                                 N
RA54E901991040399999999S000007500   ALL OTHER CONDITIONALLY EXEMPT GOODS                        N
RA6003011045672009080199999999NGRMS001400000000000000000000000 000000000000000000000000000N
RA100403204599        9902
RA20040320459990020090801999999992009-06-CPFTA   NN2009-06-CPFTA2009080199999999NN
RA3004032045992009080199999999N   N2009-06-CPFTA
RA40F000000000000000000000000000S000550000000000000000000000 000000000000000000000000000 00000000000000000000000000002N
RA520012008010199999999Y1V001600000   NORMAL RATE                                                 N
RA54E901991040399999999S000007500   ALL OTHER CONDITIONALLY EXEMPT GOODS                        N
RA6004032045992009080199999999NGRMS001400000000000000000000000 000000000000000000000000000N
";

			#endregion

			var parser = new CACTestingDataHelper.CadexMessageProcessor();
			parser.ProcessRA(new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(refFilesData))));

			var factory = ((IFactoryProvider)parser).Factory;
			CreateCAGSTRateCode(factory, 16m);

			return factory;
		}

		public static void CreateCAGSTRateCode(BusinessObjectFactory factory, ZDecimal rateValue)
		{
			var universalHelper = new UniversalReferenceTestDataHelper(factory);
			universalHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CAGSTRateCodes, "CAGSTRateCodes", Core.Constants.CountryCodes.Canada);
			universalHelper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CAGST_Rate, "DESC", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CAGSTRateCodes, Core.Constants.CountryCodes.Canada);
			universalHelper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CAGST_RateType, "DESC", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CAGSTRateCodes, Core.Constants.CountryCodes.Canada);
			var gst1 = universalHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CAGSTRateCodes, "001", "001 DESC", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			gst1.Attributes.Where(x => x.ZZE_ZXE_NKName.EqualsIgnoringCase(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CAGST_Rate)).DeleteAll();
			var rateType = universalHelper.CreateNewOrGetExistingCusCodeListAttribute(gst1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CAGST_RateType, RateTypes.Codes.AdValorem);
			var rate = universalHelper.CreateNewOrGetExistingCusCodeListAttribute(gst1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CAGST_Rate, "5.00");
			rate.ZZE_Value = rateValue.ToString("0.00");
			factory.Save();
		}

		[TestDate(2014, 1, 1)]
		public static BusinessObjectFactory PopulateDutiesAndTaxesRefFilesForExciseTestReturningFactory()
		{
			#region Ref Files Data

			const string refFilesData = @"
GA20240319004190020140101999999992014-01-TARIFNMBNN2014-01-TARIF2014010199999999NN
GA3024031900412014010199999999N   N2014-01-TARIF
GA40V000400000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000002N
GA40V003500000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000003N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000007N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000008N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000010N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000011N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000014N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000021N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000022N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000025N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000026N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000027N
GA40F000000000000000000000000000 000000000000000000000000000 000000000000000000000000000 00000000000000000000000000028N
GA5024031900412014010199999999N001E38
";

			#endregion

			var parser = new CACTestingDataHelper.CadexMessageProcessor();
			parser.ProcessAA(new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(refFilesData))));
			return ((IFactoryProvider)parser).Factory;
		}

		internal static void FillInvoiceLine(JobComInvoiceLine line, short number, decimal linePrice, decimal sima)
		{
			FillInvoiceLine(line, number, 1, linePrice, 0, 0, 0, Core.Constants.Weight.Kilograms, sima);
		}

		internal static void FillInvoiceLine(JobComInvoiceLine line, short number, int pageNumber, decimal linePrice)
		{
			FillInvoiceLine(line, number, pageNumber, linePrice, 0, 0, 0, Core.Constants.Weight.Kilograms);
		}

		public static CACClassHeader AddTariffRecord(JobComInvoiceLine line)
		{
			var tariff = CACClassHeader.Load(line.Factory, line.EffectiveDateForDutyRate, line.JI_Tariff)
				?? line.Factory.New<CACClassHeader>();
			tariff.ZA_EffectiveDate = line.EffectiveDate.AddDays(-1);
			tariff.ZA_ExpiryDate = line.EffectiveDate.AddDays(1);
			tariff.ZA_ClassificationNumber = line.JI_Tariff.Left(tariff.ZA_ClassificationNumberInfo.MaxLength);
			tariff.ZA_AreaCode = "AAA";
			tariff.ZA_StatisticalUOMCode = line.JI_CustomsUnitQty.Left(3);
			return tariff;
		}

		public static void FillInvoiceLine(JobComInvoiceLine line, short number, int pageNumber, decimal linePrice, decimal quantity1, decimal quantity2, decimal quantity3, string weightUQ, decimal sima = 0)
		{
			FillInvoiceLine(line, linePrice);
			line.JI_LineNo = number;
			line.CA_PageNumber = pageNumber;
			line.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Litre;
			line.JI_CustomsQuantity = quantity1;
			line.JI_CustomsSecondUnitQty = CustomsUnitOfMeasureList.Codes.Kilogram;
			line.JI_CustomsSecondQuantity = quantity2;
			line.JI_CustomsThirdUnitQty = CustomsUnitOfMeasureList.Codes.Gram;
			line.JI_CustomsThirdQuantity = quantity3;
			line.CA_AuthorityNumber = "AUTHORITYNUMBER";
			line.CA_TRSNumber = "TRSNUMBER";
			line.JI_Description = "NUMBER DESCRIPTION UP TO 39 CHARACTERS1";

			if (sima > 0)
			{
				var simaDuty = line.DutiesAndTaxes.FirstOrDefault() ?? line.DutiesAndTaxes.AddNew();
				simaDuty.C1_Override = true;
				simaDuty.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
				simaDuty.C1_ExemptCode = SIMACodes.Codes.C31;
				simaDuty.C1_Amount = sima;
				simaDuty.B7_ParentID = line.PK;
				simaDuty.B7_ParentTableCode = line.TablePrefix;
			}
		}

		internal static void FillInvoiceLine(JobComInvoiceLine line, decimal linePrice)
		{
			line.JI_LineNo = 1;
			line.CA_PageNumber = 1;
			line.JI_Tariff = "8466.93.90 90";
			line.CA_99TariffCode = "9960";
			line.CA_TreatmentCode = TariffTreatmentCodes.Codes.MostFavouredNation;
			line.CA_ValueForDutyCode = ValueForDutyCodes.Codes.RelatedFirmsPaidPayableWithoutAdjustments;
			line.JI_LinePrice = linePrice;
			line.JI_CountryOfOrigin = Core.Constants.CountryCodes.Japan;
		}

		internal static void FillInvoiceLine(JobComInvoiceLine line, short number, int pageNumber, string tariff, decimal linePrice)
		{
			line.JI_LineNo = number;
			line.CA_PageNumber = pageNumber;
			line.JI_Tariff = tariff;
			line.CA_TreatmentCode = TariffTreatmentCodes.Codes.MostFavouredNation;
			line.CA_ValueForDutyCode = ValueForDutyCodes.Codes.RelatedFirmsPaidPayableWithoutAdjustments;
			line.JI_LinePrice = linePrice;
			line.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			line.JI_StateOrRegionOfOrigin = USStatesList.Codes.Michigan;
		}
	}
}
