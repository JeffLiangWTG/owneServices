using System.Text;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(CreditControlledDocumentsCheckConfigurationRegistryDataType))]
	class CreditControlledDocumentsCheckConfigurationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CreditControlledDocumentsCheckConfigurationRegistryDataType>
	{
		#region Implementation

		protected override CreditControlledDocumentsCheckConfigurationRegistryDataType GetNewDataType()
		{
			return new CreditControlledDocumentsCheckConfigurationRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "CreditControlledDocumentsCheckConfigurationRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			CreditControlledDocumentsCheckConfigurationCollection collection = new CreditControlledDocumentsCheckConfigurationCollection();
			var upToCfg = new CreditControlledDocumentsCheckConfiguration();
			upToCfg.Amount = 1000M;
			upToCfg.InvoiceType = "ALL";
			upToCfg.NumberOfDaysOverdue = 10;
			upToCfg.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			upToCfg.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			collection.Add(upToCfg);
			var cfg = new CreditControlledDocumentsCheckConfiguration();
			cfg.Amount = 1000M;
			cfg.InvoiceType = "ALL";
			cfg.NumberOfDaysOverdue = 10;
			cfg.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			cfg.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			collection.Add(cfg);

			#region ByteArrayValue
			byte[] byteArrayValue = Encoding.Unicode.GetBytes(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfCreditControlledDocumentsCheckConfiguration xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
<CreditControlledDocumentsCheckConfiguration>
<Amount>1000</Amount>
<Range>Up to</Range>
<AuthorisationRequirement>None</AuthorisationRequirement>
<InvoiceType>ALL</InvoiceType>
<NumberOfDaysOverdue>10</NumberOfDaysOverdue>
</CreditControlledDocumentsCheckConfiguration>
<CreditControlledDocumentsCheckConfiguration>
<Amount>1000</Amount>
<Range>Above</Range>
<AuthorisationRequirement>1st Level Only</AuthorisationRequirement>
<InvoiceType>ALL</InvoiceType>
<NumberOfDaysOverdue>10</NumberOfDaysOverdue>
</CreditControlledDocumentsCheckConfiguration>
</ArrayOfCreditControlledDocumentsCheckConfiguration>");
#endregion

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, byteArrayValue)
			};
		}

		#endregion
	}
}
