using System;
using Enterprise.Client.JAS.Business.Testing;

namespace Enterprise.Client.JAS.Business.Matching.Testing
{
	internal class PreMatchedDataExporterValidationTest : JASDataExporterBizOValidationTest
	{
		public void TestValidateAllIncludesNettingCycle()
		{
			Exporter.DeliveryMethod = "";
			Exporter.Validation.ValidateAll();
			AssertMandatoryValidationError(Exporter.NettingCycleInfo, true);
			AssertMandatoryValidationError(Exporter.DeliveryMethodInfo, true);
			Exporter.DeliveryMethod = PreMatchedDataExporter.EmailDeliveryMethodCode;
			Exporter.IsIndividualEmailRecipient = true;
			Exporter.Validation.ValidateAll();
			AssertMandatoryValidationError(Exporter.NettingCycleInfo, true);
			AssertMandatoryValidationError(Exporter.DeliveryMethodInfo, false);
			AssertMandatoryValidationError(Exporter.EmailAddressInfo, true);
			AssertMandatoryValidationError(Exporter.EmailGroupPKInfo, false);
			AssertMandatoryValidationError(Exporter.ExportDirectoryInfo, false);
			Exporter.IsGroupEmailRecipient = true;
			Exporter.Validation.ValidateAll();
			AssertMandatoryValidationError(Exporter.NettingCycleInfo, true);
			AssertMandatoryValidationError(Exporter.DeliveryMethodInfo, false);
			AssertMandatoryValidationError(Exporter.EmailAddressInfo, false);
			AssertMandatoryValidationError(Exporter.EmailGroupPKInfo, true);
			AssertMandatoryValidationError(Exporter.ExportDirectoryInfo, false);
			Exporter.DeliveryMethod = PreMatchedDataExporter.DirectoryDeliveryMethodCode;
			Exporter.Validation.ValidateAll();
			AssertMandatoryValidationError(Exporter.NettingCycleInfo, true);
			AssertMandatoryValidationError(Exporter.DeliveryMethodInfo, false);
			AssertMandatoryValidationError(Exporter.EmailAddressInfo, false);
			AssertMandatoryValidationError(Exporter.EmailGroupPKInfo, false);
			AssertMandatoryValidationError(Exporter.ExportDirectoryInfo, true);
		}

		public void TestValidateNettingCycle()
		{
			Exporter.NettingCycle = "";
			Exporter.Validation.ValidateNettingCycle();
			AssertMandatoryValidationError(Exporter.NettingCycleInfo, true);
			Exporter.NettingCycle = "asdfasdf";
			AssertMandatoryValidationError(Exporter.NettingCycleInfo, false);
			AssertListValidationInvalidCodeError(Exporter.NettingCycleInfo, true);
			Exporter.NettingCycle = "11-JAN-11";
			AssertListValidationInvalidCodeError(Exporter.NettingCycleInfo, true);
			Exporter.NettingCycle = Exporter.NettingCycleList[0].Code;
			AssertListValidationInvalidCodeError(Exporter.NettingCycleInfo, false);
		}

		PreMatchedDataExporter Exporter
		{
			get
			{
				return (PreMatchedDataExporter)base.DataExporterBizO;
			}
		}

		protected override JASDataExporterBizO GetNewJASDataExporterBizO()
		{
			return new PreMatchedDataExporter();
		}

		protected override Type ExpectedAutoValidationType
		{
			get
			{
				return typeof(PreMatchedDataExporterValidation);
			}
		}
	}
}
