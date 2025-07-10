using CargoWise.Types;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	sealed class ImportSADNumberValidationTest
		: EU.EMCS.Business.Testing.ImportSADNumberValidationTest
	{
		public void TestCheckCSI_Description_Empty()
		{
			CombineAssertions(() =>
			{
				importSADNumber.CSI_Description = "INVALID";
				AssertHasMessageError("Invalid Code", importSADNumber.CSI_DescriptionInfo, InvalidSadCodeMessage);

				importSADNumber.CSI_Description = ZString.Empty;
				AssertNoMessageError("Empty", importSADNumber.CSI_DescriptionInfo, InvalidSadCodeMessage);
			});
		}

		public void TestCheckCSI_Description_Length()
		{
			CombineAssertions(() =>
			{
				importSADNumber.CSI_Description = "0001ATA7800000104202";
				AssertHasMessageError("Short", importSADNumber.CSI_DescriptionInfo, InvalidSadCodeMessage);

				importSADNumber.CSI_Description = "0001ATA78000001042020";
				AssertNoMessageError("Valid", importSADNumber.CSI_DescriptionInfo, InvalidSadCodeMessage);
			});
		}

		public void TestCheckCSI_Description_InvalidATLAS_LineNumber()
		{
			importSADNumber.CSI_Description = "0000ATA78000001042020";
			AssertHasMessageError("Wrong line number", importSADNumber.CSI_DescriptionInfo, InvalidSadCodeMessage);
		}

		public void TestCheckCSI_Description_InvalidATLAS_ProcedureCode()
		{
			importSADNumber.CSI_Description = "0001TTA78000001042020";
			AssertHasMessageError("Wrong procedure code", importSADNumber.CSI_DescriptionInfo, InvalidSadCodeMessage);
		}

		public void TestCheckCSI_Description_InvalidATLAS_DocumentType()
		{
			importSADNumber.CSI_Description = "0001AT178000001042020";
			AssertHasMessageError("Wrong document type", importSADNumber.CSI_DescriptionInfo, InvalidSadCodeMessage);
		}

		public void TestCheckCSI_Description_InvalidATLAS_ProcessCode()
		{
			importSADNumber.CSI_Description = "0001ATAA8000001042020";
			AssertHasMessageError("Wrong process code", importSADNumber.CSI_DescriptionInfo, InvalidSadCodeMessage);
		}

		public void TestCheckCSI_Description_InvalidATLAS_SequenceNumbers()
		{
			CombineAssertions(() =>
			{
				importSADNumber.CSI_Description = "0001ATA78100001042020";
				AssertHasMessageError("Number without a leading zero", importSADNumber.CSI_DescriptionInfo, InvalidSadCodeMessage);
				importSADNumber.CSI_Description = "0001ATA780A0001042020";
				AssertHasMessageError("Number with letters", importSADNumber.CSI_DescriptionInfo, InvalidSadCodeMessage);
			});
		}

		public void TestCheckCSI_Description_InvalidATLAS_Month()
		{
			CombineAssertions(() =>
			{
				importSADNumber.CSI_Description = "0001ATA78000001002020";
				AssertHasMessageError("Wrong month 00", importSADNumber.CSI_DescriptionInfo, InvalidSadCodeMessage);
				importSADNumber.CSI_Description = "0001ATA78000001132020";
				AssertHasMessageError("Wrong month 13", importSADNumber.CSI_DescriptionInfo, InvalidSadCodeMessage);
				importSADNumber.CSI_Description = "0001ATA780000010A2020";
				AssertHasMessageError("Wrong month with letter(s)", importSADNumber.CSI_DescriptionInfo, InvalidSadCodeMessage);
			});
		}

		public void TestCheckCSI_Description_InvalidATLAS_Century()
		{
			CombineAssertions(() =>
			{
				importSADNumber.CSI_Description = "0001ATA78000001042120";
				AssertHasMessageError("Wrong century 21", importSADNumber.CSI_DescriptionInfo, InvalidSadCodeMessage);
				importSADNumber.CSI_Description = "0001ATA78000001042A20";
				AssertHasMessageError("Wrong century with letter(s)", importSADNumber.CSI_DescriptionInfo, InvalidSadCodeMessage);
			});
		}

		public void TestCheckCSI_Description_InvalidATLAS_Year()
		{
			importSADNumber.CSI_Description = "0001ATA7800000104202A";
			AssertHasMessageError("Wrong year", importSADNumber.CSI_DescriptionInfo, InvalidSadCodeMessage);
		}

		public void TestCheckCSI_Description_ValidMRN()
		{
			CombineAssertions(() =>
			{
				importSADNumber.CSI_Description = "00023DE4851GAAA9999U7";
				AssertHasMessageError("Invalid MRN", importSADNumber.CSI_DescriptionInfo, InvalidSadCodeMessage);
				importSADNumber.CSI_Description = "00123DE4851GAAA9999U7";
				AssertNoMessageError("Valid MRN", importSADNumber.CSI_DescriptionInfo, InvalidSadCodeMessage);
			});
		}

		public void TestCheckCSI_Description_InvalidMRN_LineNumber()
		{
			importSADNumber.CSI_Description = "00023DE4851GAAA9999U7";
			AssertHasMessageError("Wrong MRN Line Number", importSADNumber.CSI_DescriptionInfo, InvalidSadCodeMessage);
		}

		public void TestCheckCSI_Description_InvalidMRN_Year()
		{
			importSADNumber.CSI_Description = "001XYDE4851GAAA9999U7";
			AssertHasMessageError("Wrong MRN Year", importSADNumber.CSI_DescriptionInfo, InvalidSadCodeMessage);
		}

		public void TestCheckCSI_Description_InvalidMRN_CountryCode()
		{
			importSADNumber.CSI_Description = "00123004851GAAA9999U7";
			AssertHasMessageError("Wrong MRN Country Code", importSADNumber.CSI_DescriptionInfo, InvalidSadCodeMessage);
		}

		public void TestCheckCSI_Description_InvalidMRN_CustomsOfficeCode()
		{
			importSADNumber.CSI_Description = "00123DEXYZTGAAA9999U7";
			AssertHasMessageError("Wrong MRN Customs Office Code", importSADNumber.CSI_DescriptionInfo, InvalidSadCodeMessage);
		}

		public void TestCheckCSI_Description_InvalidMRN_Month()
		{
			importSADNumber.CSI_Description = "00123DE4851XAAA9999U7";
			AssertHasMessageError("Wrong MRN Month", importSADNumber.CSI_DescriptionInfo, InvalidSadCodeMessage);
		}

		public void TestCheckCSI_Description_InvalidMRN_Type()
		{
			importSADNumber.CSI_Description = "00123DE4851G0AA9999U7";
			AssertHasMessageError("Wrong MRN Type", importSADNumber.CSI_DescriptionInfo, InvalidSadCodeMessage);
		}

		public void TestCheckCSI_Description_InvalidMRN_CPCCode()
		{
			importSADNumber.CSI_Description = "00123DE4851GA0A9999U7";
			AssertHasMessageError("Wrong MRN CPC Code", importSADNumber.CSI_DescriptionInfo, InvalidSadCodeMessage);
		}

		public void TestCheckCSI_Description_InvalidMRN_AlphanumericNumber()
		{
			importSADNumber.CSI_Description = "00123DE4851GAAA999@U7";
			AssertHasMessageError("Wrong MRN Alphanumeric Number", importSADNumber.CSI_DescriptionInfo, InvalidSadCodeMessage);
		}

		public void TestCheckCSI_Description_InvalidMRN_ProcessID()
		{
			importSADNumber.CSI_Description = "00123DE4851GAAA999977";
			AssertHasMessageError("Wrong MRN Process ID", importSADNumber.CSI_DescriptionInfo, InvalidSadCodeMessage);
		}

		public void TestCheckCSI_Description_InvalidMRN_CheckNumber()
		{
			importSADNumber.CSI_Description = "00123DE4851GAAA9999UA";
			AssertHasMessageError("Wrong MRN Check Number", importSADNumber.CSI_DescriptionInfo, InvalidSadCodeMessage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<EMCSJobDeclaration>();
			importSADNumber = declaration.ImportSADNumbers.AddNew();
		}
		ImportSADNumber importSADNumber;

		const string InvalidSadCodeMessage = @"Invalid Import SAD Number structure. Please enter an Import SAD Number (21 alphanumeric characters) in the following format: 
ATLAS Registration: 
• four numerics for the line number (0001-9999), 
• two letters 'AT' for the procedure code, 
• a letter for the document type 
• two numerics for the process code 
• six numerics for the sequence number with leading zeros 
• two numerics for the month (01-12) 
• two numerics for the century (20) 
• two numerics for the year (00-99) 
or MRN Registration: 
• three numerics for the line number (001-999), 
• Year '00' – '99' (2 digits) 
• ISO-Alpha-2-Country Code (2 digits) 
• Customs Office Code (4 digits) 
• Month 'A' – 'L' in capital letters (1 digit) 
• Type in capital letters (1 digit) 
• CPC Code in capital letters (1 digit) 
• Alphanumeric number (5 digits) 
• Process ID (1 digit) 
• Check digit (1 digit)";
	}
}
