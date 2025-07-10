using System.Text.RegularExpressions;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class ImportSADNumberValidation : EU.EMCS.Business.ImportSADNumberValidation
	{
		public ImportSADNumberValidation(ImportSADNumber bizObj) : base(bizObj)
		{
		}

		protected override void CheckCSI_Description()
		{
			base.CheckCSI_Description();
			var sadCode = Parent.CSI_Description;
			if (!sadCode.IsEmpty)
			{
				var atlasRegex = new Regex(@"^(?!0000)\d{4}AT[A-Z][0-9]{2}0[0-9]{5}(01|02|03|04|05|06|07|08|09|10|11|12)20[0-9]{2}$");
				var mrnRegex = new Regex(@"^(?!000)\d{3}\d{2}[A-Z]{2}\d{4}[A-L][A-Z][A-Z][A\d-Z\d]{5}[A-Z]\d$");

				if (!atlasRegex.IsMatch(sadCode) && !mrnRegex.IsMatch(sadCode))
				{
					Parent.CSI_DescriptionInfo.AddMessageError(Res.GetString("467BCDD9-D430-4478-8EF8-E42BB99FDAE8", @"Invalid Import SAD Number structure. Please enter an Import SAD Number (21 alphanumeric characters) in the following format: 
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
• Check digit (1 digit)"));
				}
			}
		}
	}
}
