using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class GroupInvoiceCharge
		: EU.Business.Declaration.GroupInvoiceCharge
		, Integration.Customs.GB.IGroupInvoiceCharge
	{
		public GroupInvoiceCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString CDSChargeCode => this.GetCDSChargeCode();

		public new JobComInvoiceGroupHeader GroupInvoice => (JobComInvoiceGroupHeader)base.GroupInvoice;

		protected override JobComInvHeaderChargeLookups GetNewLookups() => new GroupInvoiceChargeLookups(this);

		protected override JobComInvHeaderChargeValidation GetNewValidation() => new GroupInvoiceChargeValidation(this);

		public override ZString J7_ChargeType
		{
			get => base.J7_ChargeType;
			set
			{
				base.J7_ChargeType = value;
				UpdateDistributeBy();
			}
		}

		public void UpdateDistributeBy()
		{
			var apportionByWeight = JobDeclaration?.ZG_ApportionByWeight ?? ZBool.False;

			J7_DistributeBy = apportionByWeight && ChargesProvider.ChargesValidToBeApportionedByWeight.Contains(J7_ChargeType.ToString()) ? ChargeDistributeByList.Codes.Weight : ChargeDistributeByList.Codes.Value;
		}

		JobDeclaration JobDeclaration => GroupInvoice?.JobDeclaration;
	}
}
