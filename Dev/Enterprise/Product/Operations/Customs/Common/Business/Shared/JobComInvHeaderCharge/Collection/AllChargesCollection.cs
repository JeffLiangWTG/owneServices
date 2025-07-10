using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Common
{
	/// <summary>
	/// This collection is used to load charges + apportioned charges together. However it does not get synchronised when Charges or ApportionCharges has a new element.
	/// </summary>
	public class AllChargesCollection : DependentBusinessObjectCollection<JobComInvCharge, BusinessObject>
	{
		public AllChargesCollection(ICommonInvoice commonInvoice) : base(commonInvoice as BusinessObject)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return JobComInvHeaderChargeSchema.J7_ParentID; }
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}
