using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class DeltaIEJobComInvoiceLineLookups : JobComInvoiceLineLookups
	{
		public DeltaIEJobComInvoiceLineLookups(JobComInvoiceLine parent) : base(parent)
		{
		}

		public override RefCusProcedureCollection CPCList
		{
			get
			{
				var result = new RefCusProcedureCollection(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, GetDateOfValuation(), Parent.EntryInstruction?.CEI_Style ?? ZString.Empty, Parent.Declaration?.JE_MessageType ?? ZString.Empty);
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(RefCusProcedureCollection.FilterConstants.ProcedureCode, "Property", Parent.EntryInstruction?.CEI_Procedure, false));

				return result;
			}
		}
	}
}
