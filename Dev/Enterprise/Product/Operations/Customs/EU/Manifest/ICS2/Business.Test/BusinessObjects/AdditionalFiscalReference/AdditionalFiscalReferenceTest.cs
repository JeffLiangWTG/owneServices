using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(AdditionalFiscalReference))]
	sealed class AdditionalFiscalReferenceTest : CusReferenceAbstractTest<AdditionalFiscalReference>
	{
		public void TestFieldCaption()
		{
			var additionalFiscalReference = Factory.New<AdditionalFiscalReference>();

			var referenceRESAttribute = additionalFiscalReference.CFR_ReferenceInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals("Identification Number", referenceRESAttribute.Caption);
			AssertEquals("ID No.", referenceRESAttribute.ShortCaption);

			var codeRESAttribute = additionalFiscalReference.CFR_CodeInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals("Type", codeRESAttribute.Caption);
		}

		public void TestCFR_Reference_MaxLength()
		{
			var additionalFiscalReference = Factory.New<AdditionalFiscalReference>();
			AssertEquals(17, additionalFiscalReference.CFR_ReferenceInfo.MaxLength);
		}

		public void TestLookups()
		{
			var additionalFiscalReference = Factory.New<AdditionalFiscalReference>();
			AssertType<AdditionalFiscalReferenceLookups>(additionalFiscalReference.Lookups);
		}

		public void TestValidation()
		{
			var additionalFiscalReference = Factory.New<AdditionalFiscalReference>();
			AssertType<AdditionalFiscalReferenceValidation>(additionalFiscalReference.Validation);
		}

		protected override IEnumerable<AdditionalFiscalReference> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return (AdditionalFiscalReference)GetNewBusinessObjectForDeleteTest(factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;
			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F43;

			var bill = header.Bills.AddNew();
			return FillWithValidData(bill.AdditionalFiscalReferences.AddNew());
		}

		AdditionalFiscalReference FillWithValidData(AdditionalFiscalReference reference)
		{
			reference.CFR_Code = EUICS2AdditionalFiscalReferenceTypes.Codes.FR5;
			reference.CFR_Reference = "FR5";
			return reference;
		}
	}
}
