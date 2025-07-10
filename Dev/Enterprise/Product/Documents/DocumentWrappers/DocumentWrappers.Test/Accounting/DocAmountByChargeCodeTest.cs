using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocAmountByChargeCode))]
	sealed class DocAmountByChargeCodeTest : GenericWrapperTest
	{
		public void TestChargeCode()
		{
			DocAmountByChargeCode wrapper = DocAmountByChargeCode.New(new AmountByChargeCode("FRT", 100m, 90m), Factory);
			AssertEquals("FRT", wrapper.ChargeCode);
		}

		public void TestTotalAmount()
		{
			DocAmountByChargeCode wrapper = DocAmountByChargeCode.New(new AmountByChargeCode("FRT", 100m, 90m), Factory);
			AssertEquals(100m, wrapper.TotalAmount);
		}

		public void TestTotalAmountExTax()
		{
			DocAmountByChargeCode wrapper = DocAmountByChargeCode.New(new AmountByChargeCode("FRT", 100m, 90m), Factory);
			AssertEquals(90m, wrapper.TotalAmountExTax);
		}

		#region Implementation

		public override void TestWrapperMappingsEmpty()
		{
			DocAmountByChargeCode wrapper = (DocAmountByChargeCode)GetNewDocumentWrapper();
			AssertEquals("wrapper.ChargeCode", "", wrapper.ChargeCode);
			AssertEquals("wrapper.TotalAmount", 0m, wrapper.TotalAmount);
			AssertEquals("wrapper.TotalAmountExTax", 0m, wrapper.TotalAmountExTax);
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
			return DocAmountByChargeCode.New(new AmountByChargeCode("FRT", 100, 90), Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"DocAmountByChargeCode                     (Default Field: TotalAmount)
======================================================================
Name                                    Type
----------------------------------------------------------------------
ChargeCode                              String
TotalAmount                             Decimal
TotalAmountExTax                        Decimal";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return DocAmountByChargeCode.New(new AmountByChargeCode(), Factory);
		}

		#endregion
	}
}
