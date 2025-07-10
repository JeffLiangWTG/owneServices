using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.IE;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(ImportAddInfoJobComInvoiceHeaderValidation))]
	sealed class ImportAddInfoJobComInvoiceHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZG_AgreedPlaceCode_RuleBR4010()
		{
			const string messageError = "[BR4010] Incoterm Place Code must be outside the EU.";
			var targetPropertyInfo = invoiceHeader.ZG_AgreedPlaceCodeInfo;
			var allIncoterms = new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms).GetAllCodes();
			var incotermsWithError = new string[]
			{
				Core.Constants.IncoTerms.ExWorks,
				Core.Constants.IncoTerms.FreeCarrier,
				Core.Constants.IncoTerms.FreeAlongsideShip,
				Core.Constants.IncoTerms.FreeOnBoard,
			};

			invoiceHeader.ZG_AgreedPlaceCode = "AT123";
			CombineAssertions("When Agreed Place Code first two characters starts with an EU Country Code", () =>
			{
				incotermsWithError.ForEach(incoterm =>
				{
					invoiceHeader.JZ_IncoTerm = incoterm;
					invoiceHeader.AddInfoValidation.ValidateZG_AgreedPlaceCode();
					AssertHasMessageErrorContaining($"When IncoTerm={incoterm}", targetPropertyInfo, messageError);
				});
				var incotermsWithNoError = allIncoterms.Except(incotermsWithError);
				incotermsWithNoError.ForEach(incoterm =>
				{
					invoiceHeader.JZ_IncoTerm = incoterm;
					invoiceHeader.AddInfoValidation.ValidateZG_AgreedPlaceCode();
					AssertNoMessageErrorContaining($"When IncoTerm={incoterm}", targetPropertyInfo, messageError);
				});
			});

			invoiceHeader.ZG_AgreedPlaceCode = "US122";
			CombineAssertions("When Agreed Place Code first two characters do not start with an EU Country Code", () =>
			{
				allIncoterms.ForEach(incoterm =>
				{
					invoiceHeader.JZ_IncoTerm = incoterm;
					invoiceHeader.AddInfoValidation.ValidateZG_AgreedPlaceCode();
					AssertNoMessageErrorContaining($"When IncoTerm={incoterm}", targetPropertyInfo, messageError);
				});
			});
		}

		public void TestIsMandatoryZG_AgreedPlaceCodeCore()
		{
			declaration.JE_ApplicationCode = "V1";
			var instrction = declaration.CustomsEntryInstructions.AddNew();
			var line = invoiceHeader.InvoiceLines.AddNew();
			line.JI_CEI = instrction.PK;
			instrction.CEI_Style = "H1";

			CombineAssertions(() =>
			{
				AssertEquals("H1 and UCC5", true, invoiceHeader.AddInfoValidation.IsMandatoryZG_AgreedPlaceCode);

				instrction.CEI_Style = "H2";
				AssertEquals("H2 and UCC5", false, invoiceHeader.AddInfoValidation.IsMandatoryZG_AgreedPlaceCode);
			});
		}

		public void TestCheckZG_AgreedPlaceCode_IsRequired()
		{
			const string messageError = "Incoterm Place Code or Country Code is required";
			declaration.JE_ApplicationCode = "V1";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var line = invoiceHeader.InvoiceLines.AddNew();
			line.JI_CEI = instruction.PK;
			instruction.CEI_Style = "H1";
			CombineAssertions(() =>
			{
				invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CarriageAndInsurancePaidTo;
				invoiceHeader.ZG_AgreedPlaceCode = CargoWise.Types.ZString.Empty;
				AssertHasMessageError("Empty ZG_AgreedPlaceCode", invoiceHeader.ZG_AgreedPlaceCodeInfo, messageError);

				instruction.CEI_Style = "H2";
				invoiceHeader.AddInfoValidation.ValidateZG_AgreedPlaceCode();
				AssertNoMessageError("Declaration type is H2 or I1", invoiceHeader.ZG_AgreedPlaceCodeInfo, messageError);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			invoiceHeader = declaration.Invoices.AddNew();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
		}
		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
	}
}
