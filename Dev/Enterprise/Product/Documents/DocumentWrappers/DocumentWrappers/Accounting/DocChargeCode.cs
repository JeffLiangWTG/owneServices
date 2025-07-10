using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocChargeCode : DocBaseWrapper
	{
		DocChargeCode(AccChargeCode accChargeCode, BusinessObjectFactory factoryToWrap)
			: base(accChargeCode, factoryToWrap)
		{
		}

		public static DocChargeCode New(BusinessObjectFactory factory, ZGuid pK)
		{
			return New(factory.Load<AccChargeCode>(pK), factory);
		}

		public static DocChargeCode New(AccChargeCode accChargeCode, BusinessObjectFactory factoryToWrap)
		{
			if (accChargeCode == null)
			{
				return null;
			}
			else
			{
				return factoryToWrap.GetCachedValue(accChargeCode.PK.ToStringKey(), () => new DocChargeCode(accChargeCode, factoryToWrap), CacheStalenessPolicy.StaleOnFactorySave);
			}
		}

		internal AccChargeCode AccChargeCode
		{
			get { return (AccChargeCode)WrappedObject; }
		}

		public ZGuid ChargeCodePK
		{
			get { return AccChargeCode.PK; }
		}

		public override string ToString()
		{
			return Code;
		}

		public ZString RateCalculatorDesc
		{
			get { return AccChargeCode.AC_RateCalculatorDesc; }
		}

		public DocGLAccount AccrualGLAccount
		{
			get { return DocGLAccount.New(AccChargeCode.AccrualAccount, Factory); }
		}

		public DocGLAccount CostGLAccount
		{
			get { return DocGLAccount.New(AccChargeCode.CostAccount, Factory); }
		}

		public DocGLAccount RevenueGLAccount
		{
			get { return DocGLAccount.New(AccChargeCode.RevenueAccount, Factory); }
		}

		public DocGLAccount WIPGLAccount
		{
			get { return DocGLAccount.New(AccChargeCode.WIPAccount, Factory); }
		}

		public DocGroup ExpenseGroup
		{
			get { return DocGroup.New(AccChargeCode.ExpenseGroup, Factory); }
		}

		public DocGroup SalesGroup
		{
			get { return DocGroup.New(AccChargeCode.SalesGroup, Factory); }
		}

		public DocTaxRate TaxRate
		{
			get { return DocTaxRate.New(AccChargeCode.GSTRate, Factory); }
		}

		public DocWithholdingTaxRate WithholdingTaxRate
		{
			get { return DocWithholdingTaxRate.New(AccChargeCode.WithholdingTaxRate, Factory); }
		}

		public ZString ChargeType
		{
			get { return AccChargeCode.AC_ChargeType; }
		}

		public ZString Code
		{
			get { return AccChargeCode.AC_Code; }
		}

		public ZShort Sequence
		{
			get { return AccChargeCode.AC_PrintSequence; }
		}

		public ZString DepartmentFilterList
		{
			get { return AccChargeCode.AC_DepartmentFilterList; }
		}

		public ZString Desc
		{
			get { return AccChargeCode.AC_DescMultilingual; }
		}

		public DocCompany Company
		{
			get { return DocCompany.New(AccChargeCode.Company, Factory); }
		}

		public ZBool IsActive
		{
			get { return AccChargeCode.AC_IsActive; }
		}

		public ZDecimal MarginPercentage
		{
			get { return AccChargeCode.AC_MarginPercentage; }
		}

		public ZString RateCalculator
		{
			get { return AccChargeCode.AC_RateCalculator; }
		}

		public ZString ChargeGroup
		{
			get { return AccChargeCode.AC_ChargeGroup; }
		}

		protected override ZString DocManagerUniqueID
		{
			get { return Code; }
		}
	}
}
