using System;
using System.Text;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(TaxRecognitionDefaultingRulesRegistryDataType))]
	class TaxRecognitionDefaultingRulesRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<TaxRecognitionDefaultingRulesRegistryDataType>
	{
		public void TestValidation()
		{
			var glbCompany = new BusinessObjectFactory().LoadTop1<GlbCompany>(new ZQuery());
			var dataType = GetNewDataType();
			var registryItem = new TaxRecognitionDefaultingRulesRegistryItem(String.Empty, null, null, null, RegistryStorageFlags.Company);

			glbCompany.GC_IsGSTCashBasis = false;
			glbCompany.Factory.Save();
			AssertExceptionThrown<RegistryValidationException>(() => dataType.Validate(registryItem, new TaxRecognitionDefaultingRules(), glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));

			glbCompany.GC_IsGSTCashBasis = true;
			glbCompany.Factory.Save();
			AssertNoExceptionThrown(() => dataType.Validate(registryItem, new TaxRecognitionDefaultingRules(), glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}

		protected override string ExpectedEditorName
		{
			get { return "TaxRecognitionDefaultingRulesRegistryItemEditor"; }
		}

		protected override TaxRecognitionDefaultingRulesRegistryDataType GetNewDataType()
		{
			return new TaxRecognitionDefaultingRulesRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var sample = new TaxRecognitionDefaultingRules();

			string stringValue = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
								"<TaxRecognitionDefaultingRules>" +
									"<APInputGoods>ACR</APInputGoods>" +
									"<APInputServices>ACR</APInputServices>" +
									"<APOrganizationOverride>NO</APOrganizationOverride>" +
									"<AROutputGoods>ACR</AROutputGoods>" +
									"<AROutputServices>ACR</AROutputServices>" +
									"<AROrganizationOverride>NO</AROrganizationOverride>" +
								"</TaxRecognitionDefaultingRules>";

			return new ValidSampleAndBinaryValueInDB[] { new ValidSampleAndBinaryValueInDB(sample, Encoding.Unicode.GetBytes(stringValue)) };
		}
	}
}
