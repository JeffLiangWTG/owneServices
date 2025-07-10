using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(ChargeWrapperCollection))]
	sealed class ChargeWrapperCollectionTest : GenericWrapperCollectionTest<ChargeWrapperCollection>
	{
		public void TestLoadFromJob()
		{
			SundryCharges sundry = Factory.New<SundryCharges>();
			Job job = new Job.Loader(sundry).TryLoadOrCreate();

			Charge charge1 = AddCharge(job, "FRT");
			Charge charge2 = AddCharge(job, "ODOC");

			ChargeWrapperCollection wrappers = new ChargeWrapperCollection(job, Factory);
			AssertEquals("wrappers.Count", 2, wrappers.Count);
			AssertEquals("wrappers[0].WrappedObject", charge1, wrappers[0].WrappedObject);
			AssertEquals("wrappers[1].WrappedObject", charge2, wrappers[1].WrappedObject);
		}

		public void TestIsGSTApplicable()
		{
			SundryCharges sundry = Factory.New<SundryCharges>();
			Job job = new Job.Loader(sundry).TryLoadOrCreate();

			Charge charge1 = AddCharge(job, "FRT");
			Charge charge2 = AddCharge(job, "ODOC");

			ChargeWrapperCollection wrappers = new ChargeWrapperCollection(job, Factory);

			Assert(!wrappers.IsGSTApplicable);

			var testObjectCreator = new TestObjectCreator(Factory);
			charge2.JR_AT_SellGSTRate = testObjectCreator.GST1.PK;
			charge2.JR_OSSellAmt = 1005m;
			charge2.JR_RX_NKSellCurrency = "AUD";
			charge2.JR_AL_ARLine = Factory.New<AccTransactionLines>().PK;
			charge2.ARLine.AL_RX_NKTransactionCurrency = charge2.JR_SellCurrency;
			charge2.ARLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			charge2.ARLine.AL_OSAmount = 2000;
			charge2.ARLine.AL_GSTVAT = 200;

			wrappers = new ChargeWrapperCollection(job, Factory);

			Assert(wrappers.IsGSTApplicable);
		}

		#region Implementation

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			SundryCharges sundry = Factory.New<SundryCharges>();
			Job job = new Job.Loader(sundry).TryLoadOrCreate();

			Charge charge = AddCharge(job, "FRT");

			return new ChargeWrapper(charge, Factory);
		}

		protected override ChargeWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new ChargeWrapperCollection(Factory);
		}

		static Charge AddCharge(Job header, string chargeCode)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(AccChargeCodeSchema.AC_Code, chargeCode);
			filter.AddToFilter(AccChargeCodeSchema.AC_GC, header.JH_GC);

			AccChargeCode ac = header.Factory.LoadTop1<AccChargeCode>(filter);

			Charge charge = header.Charges.AddNew();
			charge.JR_AC = ac.PK;

			return charge;
		}

		#endregion
	}
}
