using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.Base.Testing
{
	[TestedType(typeof(DocBaseCusEntryLineFee))]
	sealed class DocBaseCusEntryLineFeeTestCase : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { LineFeeWrapper };
		}

		public void TestChargeAmount()
		{
			LineFee.CF_ChargeAmount = 100M;
			AssertEquals("Charge amount", 100M, LineFeeWrapper.ChargeAmount);
		}

		public void TestChargeType()
		{
			LineFee.CF_ChargeType = "ABC";
			AssertEquals("Charge type", "ABC", LineFeeWrapper.ChargeType);
		}

		public void TestDescription()
		{
			LineFee.CF_ChargeType = "XXX";
			AssertEquals("Charge Description", "Test Description", LineFeeWrapper.Description);
		}

		protected override void SetUp()
		{
			var header = Factory.New<CusEntryHeaderForTest>();
			var line = Factory.New<CusEntryLineForTest>();
			line.CL_CH = header.PK;
			LineFee = Factory.New<CusEntryLineFeeForTest>();
			LineFee.CF_CL = line.PK;
			LineFeeWrapper = DocBaseCusEntryLineFee.New(LineFee, Factory);
			base.SetUp();
		}

		DocBaseCusEntryLineFee LineFeeWrapper;
		CusEntryLineFeeForTest LineFee;

		public class CusEntryHeaderForTest : CusEntryHeader
		{
			public CusEntryHeaderForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override EntryChargeTypeList GetEntryChargeTypeList()
			{
				EntryChargeTypeList chargeTypes = new EmptyEntryChargeTypeList();
				chargeTypes.Add("XXX", "Test Description", false, "");
				return chargeTypes;
			}
		}

		public class CusEntryLineForTest : CusEntryLine
		{
			public CusEntryLineForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override CusEntryHeader Header
			{
				get { return Factory.Load<CusEntryHeaderForTest>(CL_CH); }
			}
		}

		public class CusEntryLineFeeForTest : CusEntryLineFee
		{
			public CusEntryLineFeeForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override CusEntryLine GetEntryLine()
			{
				return Factory.Load<CusEntryLineForTest>(CF_CL);
			}
		}
	}
}
