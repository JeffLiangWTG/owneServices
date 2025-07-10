using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.Base.Testing
{
	[TestedType(typeof(DocBaseCusEntryHeaderCharges))]
	sealed class DocBaseCusEntryHeaderChargesTestCase : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { ChargesWrapper };
		}

		public void TestChargeAmount()
		{
			Charges.C1_ChargeAmount = 100M;
			AssertEquals("Charge amount", 100M, ChargesWrapper.ChargeAmount);
		}

		public void TestChargeType()
		{
			Charges.C1_ChargeType = "XXX";
			AssertEquals("ChargeType", "XXX", ChargesWrapper.ChargeType);
		}

		public void TestDescription()
		{
			Charges.C1_ChargeType = "XXX";
			AssertEquals("Description", "Test Description", ChargesWrapper.Description);
			AssertEquals("ToString()", "Test Description", ChargesWrapper.ToString());
		}

		protected override void SetUp()
		{
			Header = Factory.New<CusEntryHeaderForTest>();
			Charges = Header.Charges.AddNew();
			ChargesWrapper = DocBaseCusEntryHeaderChargesForTest.New(Charges, Factory);
			base.SetUp();
		}

		CusEntryHeaderForTest Header;
		CusEntryHeaderCharges Charges;
		DocBaseCusEntryHeaderChargesForTest ChargesWrapper;

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

		public class DocBaseCusEntryHeaderChargesForTest : DocBaseCusEntryHeaderCharges
		{
			DocBaseCusEntryHeaderChargesForTest(CusEntryHeaderCharges cusEntryHeaderCharges, BusinessObjectFactory factory)
				: base(cusEntryHeaderCharges, factory)
			{
			}

			public static new DocBaseCusEntryHeaderChargesForTest New(CusEntryHeaderCharges cusEntryHeaderCharges, BusinessObjectFactory factoryToWrap)
			{
				if (cusEntryHeaderCharges == null)
				{
					return null;
				}
				else
				{
					return new DocBaseCusEntryHeaderChargesForTest(cusEntryHeaderCharges, factoryToWrap);
				}
			}

			protected override CusEntryHeader EntryHeader
			{
				get
				{
					return Factory.Load<CusEntryHeaderForTest>(Charges.C1_CH);
				}
			}

			protected CusEntryHeaderCharges Charges
			{
				get { return (CusEntryHeaderCharges)WrappedObject; }
			}
		}
	}
}
