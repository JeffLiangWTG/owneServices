using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(GuaranteeAccessCodesUserSelectionObject))]
	sealed class GuaranteeAccessCodesUserSelectionObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new GuaranteeAccessCodesUserSelectionObject(null));
		}

		public void TestGuaranteeType()
		{
			var guarantee = Factory.New<NctsGuarantee>();
			var sendingObject = new GuaranteeAccessCodesUserSelectionObject(guarantee);

			guarantee.PW_BondType = ZString.Empty;
			AssertEquals("Empty", ZString.Empty, sendingObject.GuaranteeType);

			guarantee.PW_BondType = "ABC";
			AssertEquals("With value", "ABC", sendingObject.GuaranteeType);
		}

		public void TestGuaranteeReferenceNumber()
		{
			var guarantee = Factory.New<NctsGuarantee>();
			var sendingObject = new GuaranteeAccessCodesUserSelectionObject(guarantee);

			guarantee.PW_BondNumber = ZString.Empty;
			AssertEquals("Empty", ZString.Empty, sendingObject.GuaranteeReferenceNumber);

			guarantee.PW_BondNumber = "GRN";
			AssertEquals("With value", "GRN", sendingObject.GuaranteeReferenceNumber);
		}

		public void TestOtherGuaranteeReference()
		{
			var guarantee = Factory.New<NctsGuarantee>();
			var sendingObject = new GuaranteeAccessCodesUserSelectionObject(guarantee);

			guarantee.PW_BondNumber2 = ZString.Empty;
			AssertEquals("Empty", ZString.Empty, sendingObject.OtherGuaranteeReference);

			guarantee.PW_BondNumber2 = "BondNumber";
			AssertEquals("With value", "BondNumber", sendingObject.OtherGuaranteeReference);
		}

		public void TestAccessCode()
		{
			var guarantee = Factory.New<NctsGuarantee>();
			var sendingObject = new GuaranteeAccessCodesUserSelectionObject(guarantee);

			guarantee.PW_Password = ZString.Empty;
			AssertEquals("Empty", ZString.Empty, sendingObject.AccessCode);

			guarantee.PW_Password = "Pass";
			AssertEquals("With value", "Pass", sendingObject.AccessCode);
		}
	}
}
