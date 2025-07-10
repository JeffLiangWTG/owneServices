using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(ReadOnlyAdditionalInfo))]
	sealed class ReadOnlyAdditionalInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null Additional Info", () => new ReadOnlyAdditionalInfo(null));
		}

		public void TestProperties()
		{
			var additionalInfo = GetAdditionalInfo("REF111", "TRA");

			var readOnlyAdditionalInfo = new ReadOnlyAdditionalInfo(additionalInfo);

			CombineAssertions("ReadOnlyAdditionalInfo", () =>
			{
				AssertEquals("CSI_Code", additionalInfo.CSI_Code, readOnlyAdditionalInfo.CSI_Code);
				AssertEquals("CSI_Description", additionalInfo.CSI_Description, readOnlyAdditionalInfo.CSI_Description);
				AssertEquals("CSI_SubType", additionalInfo.CSI_SubType, readOnlyAdditionalInfo.CSI_SubType);
				AssertEquals("CSI_ReferenceNumber", additionalInfo.CSI_ReferenceNumber, readOnlyAdditionalInfo.CSI_ReferenceNumber);
				AssertEquals("CSI_ReferenceNumber2", additionalInfo.CSI_ReferenceNumber2, readOnlyAdditionalInfo.CSI_ReferenceNumber2);
				AssertEquals("CSI_RX_NKCurrency", additionalInfo.CSI_RX_NKCurrency, readOnlyAdditionalInfo.CSI_RX_NKCurrency);
				AssertEquals("CSI_Value", additionalInfo.CSI_Value, readOnlyAdditionalInfo.CSI_Value);
				AssertEquals("CSI_Status", additionalInfo.CSI_Status, readOnlyAdditionalInfo.CSI_Status);
				AssertEquals("CSI_DataModel", additionalInfo.CSI_DataModel, readOnlyAdditionalInfo.CSI_DataModel);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var additionalInfo = GetAdditionalInfo("REF111", "TRA");

			return new ReadOnlyAdditionalInfo(additionalInfo);
		}

		AdditionalInfo GetAdditionalInfo(ZString refNumber, ZString kind)
		{
			var addInf = Factory.New<AdditionalInfo>();
			addInf.SuspendValidation();

			addInf.CSI_Code = "1234";
			addInf.CSI_Description = "description";
			addInf.CSI_SubType = kind;
			addInf.CSI_ReferenceNumber = refNumber;
			addInf.CSI_ReferenceNumber2 = refNumber + "Extra";
			addInf.CSI_RX_NKCurrency = "EUR";
			addInf.CSI_Value = 20;
			addInf.CSI_Status = "QWE";
			addInf.CSI_DataModel = Core.Constants.CountryCodes.Spain;

			return addInf;
		}
	}
}
