using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocInvoiceHasChargeCode))]
	sealed class DocInvoiceHasChargeCodeTest : GenericWrapperTest
	{
		public void TestChargeCode()
		{
			DocInvoiceHasChargeCode wrapper = DocInvoiceHasChargeCode.New(new InvoiceHasChargeCode("FRT", false), Factory);
			AssertEquals("FRT", wrapper.ChargeCode);
		}

		public void TestExists()
		{
			DocInvoiceHasChargeCode wrapper = DocInvoiceHasChargeCode.New(new InvoiceHasChargeCode("FRT", false), Factory);
			AssertEquals(false, wrapper.Exists);
		}

		#region Implementation

		public override void TestWrapperMappingsEmpty()
		{
			DocInvoiceHasChargeCode wrapper = (DocInvoiceHasChargeCode)GetNewDocumentWrapper();
			AssertEquals("wrapper.ChargeCode", "", wrapper.ChargeCode);
			AssertEquals("wrapper.Exists", false, wrapper.Exists);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return "Registry : (No Default Field Value Available on Registry)";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return DocInvoiceHasChargeCode.New(new InvoiceHasChargeCode("FRT", ZBool.True), Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"DocInvoiceHasChargeCode                        (Default Field: Exists)
======================================================================
Name                                    Type
----------------------------------------------------------------------
ChargeCode                              String
Exists                                  Bool";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return DocInvoiceHasChargeCode.New(new InvoiceHasChargeCode(), Factory);
		}

		#endregion
	}
}
