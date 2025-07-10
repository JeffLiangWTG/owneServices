using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CN.Business
{
	[SingleObjectAroundARow]
	[DependentBusinessObject(typeof(JobComInvoiceHeader), "ContractNumbers")]
	public class JobComInvoiceHeaderContract : JobComInvoiceHeaderRefs, Integration.Customs.CN.IJobComInvoiceHeaderContract
	{
		public JobComInvoiceHeaderContract(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[MaxLength(32)]
		[ResourceStringData("1e052af5-7ffb-4d56-a6a6-cb1d541b4e1a", Caption = "Contract Number", MediumCaption = "Contract No.", ShortCaption = "CTR No.")]
		public override ZString J2_ReferenceNumber { get => base.J2_ReferenceNumber; set => base.J2_ReferenceNumber = value; }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			J2_ReferenceType = Constants.CTR;
		}

		protected override JobComInvoiceHeaderRefsValidation GetNewValidation() => new JobComInvoiceHeaderContractValidation(this);
	}
}
