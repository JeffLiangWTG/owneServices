using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	internal abstract class ChargeSpecificationStrategyTest<T> : TestCaseWithFactory
			where T : IChargeSpecificationStrategy
	{
		public void TestNewStrategy()
		{
			AssertType(typeof(T), NewStrategy(new Mock<IValueObjectImportContext>().Object));
		}

		#region Implementation

		protected IChargeSpecificationStrategy NewStrategy(IValueObjectImportContext context = null, Xsd.PostedChargeHandling postedChargeHandling = Xsd.PostedChargeHandling.Abort)
		{
			return ChargeSpecificationStrategyFactory.NewStrategy(context ?? Context, Value, postedChargeHandling);
		}

		protected abstract Xsd.ChargesSpecified Value { get; }

		protected IValueObjectImportContext Context
		{
			get { return context ?? (context = new ValueObjectImportContext(Factory, Buffer)); }
		}
		IValueObjectImportContext context;

		protected NotificationBuffer Buffer
		{
			get { return buffer ?? (buffer = new NotificationBuffer()); }
		}
		NotificationBuffer buffer;

		protected AccChargeCode GetChargeCode(string code)
		{
			return GetChargeCode(GlbCompany.CurrentCompany, code);
		}

		protected AccChargeCode GetChargeCode(GlbCompany company, string code)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(AccChargeCodeSchema.AC_Code, code);
			filter.AddToFilter(AccChargeCodeSchema.AC_GC, company.PK);
			filter.IgnoreActiveFilter = true;

			AccChargeCode result = Factory.LoadTop1<AccChargeCode>(filter);

			if (result == null)
			{
				result = Factory.New<AccChargeCode>();
				result.AC_Code = code;
				result.AC_GC = company.PK;
			}

			return result;
		}

		protected GlbBranch GetBranch(ZString code)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
			filter.AddToFilter(GlbBranchSchema.GB_Code, code);

			GlbBranch result = Factory.LoadTop1<GlbBranch>(filter);

			if (result == null)
			{
				result = Factory.New<GlbBranch>();
				result.GB_GC = GlbCompany.CurrentCompany.PK;
				result.GB_Code = code;
			}

			return result;
		}

		protected GlbDepartment GetDepartment(ZString code)
		{
			GlbDepartment result = Factory.NewWithValidTestData<GlbDepartment>();
			result.GE_Code = code;
			return result;
		}

		protected Charge AddCharge(Job job, AccChargeCode accChargeCode, ZDecimal ammount, ZString currency, bool isRevenuePosted = false, bool isCostPosted = false)
		{
			var charge = job.Charges.AddNew();
			charge.JR_AC = accChargeCode.PK;
			charge.JR_OSSellAmt = ammount;
			charge.JR_RX_NKSellCurrency = currency;
			charge.JR_OSCostAmt = ammount;
			charge.JR_RX_NKCostCurrency = currency;

			if (isRevenuePosted || isCostPosted)
			{
				AccTransactionHeader header = Factory.New<AccTransactionHeader>();
				header.AH_JH = charge.JR_JH;
				header.AH_GC = charge.Branch.GB_GC;
				header.AH_GB = charge.JR_GB;
				header.AH_GE = charge.JR_GE;
				header.AH_InvoiceDate = ZDateTime.Today;

				if (isRevenuePosted)
				{
					AccTransactionLines revLine = Factory.New<AccTransactionLines>();
					revLine.AL_AH = header.PK;
					revLine.AL_GB = charge.JR_GB;
					revLine.AL_GE = charge.JR_GE;
					revLine.AL_LineType = TransactionLineTypes.Revenue;
					charge.JR_AL_ARLine = revLine.PK;
				}

				if (isCostPosted)
				{
					AccTransactionLines cstLine = Factory.New<AccTransactionLines>();
					cstLine.AL_AH = header.PK;
					cstLine.AL_GB = charge.JR_GB;
					cstLine.AL_GE = charge.JR_GE;
					cstLine.AL_LineType = TransactionLineTypes.Cost;
					charge.JR_AL_APLine = cstLine.PK;
				}

				AssertEquals(isRevenuePosted, charge.IsRevenuePosted);
				AssertEquals(isCostPosted, charge.IsCostPosted);
			}

			return charge;
		}

		protected Charge AddCharge(Job job, AccChargeCode accChargeCode, ZDecimal ammount, ZString currency, GlbBranch branch, GlbDepartment department, OrgHeader debtor, OrgHeader creditor)
		{
			Charge charge = AddCharge(job, accChargeCode, ammount, currency);
			charge.JR_GB = (branch != null) ? branch.PK : charge.JR_GB;
			charge.JR_GE = (department != null) ? department.PK : charge.JR_GE;
			charge.JR_OH_SellAccount = (debtor != null) ? debtor.PK : ZGuid.Empty;
			charge.JR_OH_CostAccount = (creditor != null) ? creditor.PK : ZGuid.Empty;

			return charge;
		}

		#endregion
	}
}
