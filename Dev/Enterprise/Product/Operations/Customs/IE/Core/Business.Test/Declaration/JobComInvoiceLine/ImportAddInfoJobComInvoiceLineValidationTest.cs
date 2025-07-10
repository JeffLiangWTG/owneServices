using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.IE;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	public class ImportAddInfoJobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZG_CountryOfDispatch_BR0614()
		{
			invoiceLine.ZG_CountryOfDispatch = "IE";
			AssertHasMessageError(invoiceLine.ZG_CountryOfDispatchInfo, message_BR0514);
		}

		public void TestCheckRuleCD5161()
		{
			instruction.CEI_Style = "I1";
			invoiceLine.JI_PrimaryPreference = "475";
			invoiceLine.AddInfoValidation.ValidateZG_CountryOfSupply();

			var countryOfOriginRequiredForPreferenceMsgError = "[CD5161] Preferential Origin (Pref. Orig.) is required when the first digit of Preference is ‘2’, ‘3’, ‘4’ or ‘5’.";
			AssertHasMessageError("PrimaryPreference starts with 4", invoiceLine.ZG_CountryOfSupplyInfo, countryOfOriginRequiredForPreferenceMsgError);

			invoiceLine.ZG_CountryOfSupply = Core.Constants.CountryCodes.Netherlands;
			AssertNoMessageError(invoiceLine.ZG_CountryOfSupplyInfo, countryOfOriginRequiredForPreferenceMsgError);
		}

		public void TestCheckZG_CountryOfDestination_NotEmpty()
		{
			var cusEntryInstruction = invoiceLine.EntryInstruction;
			var messageError = "You have not entered a Country of Destination.";

			CombineAssertions(() =>
			{
				AssertCountryOfDestinationisRequired(ImportDeclarationTypeList.Codes.H1);
				AssertCountryOfDestinationisRequired(ImportDeclarationTypeList.Codes.H2);
				AssertCountryOfDestinationisRequired(ImportDeclarationTypeList.Codes.H3);
				AssertCountryOfDestinationisRequired(ImportDeclarationTypeList.Codes.H4);
				AssertCountryOfDestinationisRequired(ImportDeclarationTypeList.Codes.H5);

				cusEntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.H6;
				invoiceLine.AddInfoValidation.ValidateZG_CountryOfDestination();
				AssertNoMessageError("No error for H6", invoiceLine.ZG_CountryOfDestinationInfo, messageError);
			});

			void AssertCountryOfDestinationisRequired(ZString cei_style)
			{
				cusEntryInstruction.CEI_Style = cei_style;
				invoiceLine.AddInfoValidation.ValidateZG_CountryOfDestination();
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.ZG_CountryOfDestinationInfo, messageError);
			}
		}

		public void TestCheckZG_CountryOfDestination_BR4013()
		{
			var targetInfo = invoiceLine.ZG_CountryOfDestinationInfo;
			foreach (var declarationType in ImportDeclarationTypeList.DeclarationTypeListBR4013)
			{
				instruction.CEI_Style = declarationType;
				invoiceLine.ZG_CountryOfDestination = "FR";
				AssertHasMessageError($"{declarationType} & country of destination <> IE & no charges available", targetInfo, Message_BR4013);

				invoiceLine.ZG_CountryOfDestination = "IE";
				AssertNoMessageError($"{declarationType} & country of destination = IE & no charges available", targetInfo, Message_BR4013);

				invoiceLine.ZG_CountryOfDestination = "FR";
				var charge = invoiceLine.Charges.AddNew();
				charge.J7_ChargeType = "AD";
				invoiceLine.AddInfoValidation.ValidateZG_CountryOfDestination();
				AssertNoMessageError($"{declarationType} & country of destination <> IE & charge AD available", targetInfo, Message_BR4013);

				charge.J7_ChargeType = "AK";
				invoiceLine.AddInfoValidation.ValidateZG_CountryOfDestination();
				AssertNoMessageError($"{declarationType} & country of destination <> IE & charge AK available", targetInfo, Message_BR4013);

				charge.Delete();
				var apportionedCharge = invoiceLine.ApportionedCharges.AddNew();
				apportionedCharge.J7_ChargeType = "AD";
				invoiceLine.AddInfoValidation.ValidateZG_CountryOfDestination();
				AssertNoMessageError($"{declarationType} & country of destination <> IE & apportioned charge AD available", targetInfo, Message_BR4013);

				apportionedCharge.J7_ChargeType = "AK";
				invoiceLine.AddInfoValidation.ValidateZG_CountryOfDestination();
				AssertNoMessageError($"{declarationType} & country of destination <> IE & apportioned charge AK available", targetInfo, Message_BR4013);
				apportionedCharge.Delete();
			}

			instruction.CEI_Style = "I1";
			invoiceLine.AddInfoValidation.ValidateZG_CountryOfDestination();
			AssertNoMessageError("I1 & country of destination <> IE & no charges available", targetInfo, Message_BR4013);
		}

		const string message_BR0514 = "[BR0514] Country of Dispatch cannot be IE when Message Type is IMP.";
		const string Message_BR4013 = "[BR4013] Please enter Charge 'AK' or 'AD' under Invoice Header > Invoice Charges when Destination is outside IE.";

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			instruction = invoiceLine.EntryInstruction;
		}

		JobComInvoiceLine invoiceLine;
		CusEntryInstruction instruction;
	}
}
