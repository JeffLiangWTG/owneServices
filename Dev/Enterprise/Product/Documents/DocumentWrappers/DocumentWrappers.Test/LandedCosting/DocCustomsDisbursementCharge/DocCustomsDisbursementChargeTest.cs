using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocCustomsDisbursementCharge))]
	sealed class DocCustomsDisbursementChargeTest : GenericWrapperTest
	{
		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return "Registry : (No Default Field Value Available on Registry)";
			}
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"DocCustomsDisbursementCharge                   (Default Field: Amount)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Amount                                  Decimal
ChargeCode                              String
DocumentCustomLabelCode                 String
Label                                   String";
			}
		}

		public override void TestWrapperMappingsEmpty()
		{
			DocCustomsDisbursementCharge wrapper = (DocCustomsDisbursementCharge)GetNewDocumentWrapper();
			AssertEquals("wrapper.ChargeCode", ZString.Empty, wrapper.ChargeCode);
			AssertEquals("wrapper.Amount", 0m, wrapper.Amount);
			AssertEquals("wrapper.Label", ZString.Empty, wrapper.Label);
			AssertEquals("wrapper.DocumentCustomLabelCode", ZString.Empty, wrapper.DocumentCustomLabelCode);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return DocCustomsDisbursementCharge.New(new CustomsDisbursementCharge(), Factory);
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var mock = new Mock<ICustomsChargeLCItemSetting>();
			mock.Setup(m => m.CostType).Returns(new ZString("TDT"));
			mock.Setup(m => m.DocumentCustomLabelCode).Returns(new ZString("DutyAmount"));
			mock.Setup(m => m.Description).Returns(new ZString("Total Duty"));
			return DocCustomsDisbursementCharge.New(new CustomsDisbursementCharge(mock.Object, 10.02m), Factory);
		}
	}
}
