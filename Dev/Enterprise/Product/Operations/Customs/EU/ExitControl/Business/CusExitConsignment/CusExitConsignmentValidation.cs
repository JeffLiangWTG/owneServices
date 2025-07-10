using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.ExitControlBase.Business;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class CusExitConsignmentValidation : ExitControlBase.Business.CusExitConsignmentValidation
	{
		public CusExitConsignmentValidation(AutoCusExitConsignment parent)
			: base(parent)
		{
		}

		protected new CusExitConsignment Parent => (CusExitConsignment)base.Parent;

		protected override void CheckCXC_MovementReference()
		{
			base.CheckCXC_MovementReference();

			if (Parent.CXC_MovementReference.IsEmpty)
			{
				CheckCXC_MovementReference_Mandatory();
			}
			else
			{
				var parentMRN = Parent.CXC_MovementReference;
				var parentPK = Parent.PK;
				var existsInHeader = Parent.Header?.CusExitConsignments.Any(c => c.PK != parentPK && c.CXC_MovementReference.EqualsIgnoringCase(parentMRN)) ?? false;
				if (existsInHeader)
				{
					var error = Res.GetString("46B0B7CD-8C0B-45AB-A6D8-AF97C5A28C32", "This MRN has already been entered.");
					Parent.CXC_MovementReferenceInfo.AddError(error);
				}
			}
		}

		protected virtual void CheckCXC_MovementReference_Mandatory()
		{
			MandatoryValidation.CheckEntered(Parent.CXC_MovementReferenceInfo);
		}

		protected override void CheckCXC_LocalReference()
		{
			base.CheckCXC_LocalReference();
			if (!Parent.CXC_LocalReference.IsEmpty)
			{
				var header = Parent.Header;
				var parentPK = Parent.PK;
				var parentLRN = Parent.CXC_LocalReference;
				var isInHeader = header.CusExitConsignments.Any(x => x.PK != parentPK && x.CXC_LocalReference.EqualsIgnoringCase(parentLRN));

				if (isInHeader)
				{
					var error = Res.GetString("0F47D997-466D-4ABD-AF17-17A5704DDC37", "This LRN has already been entered");
					Parent.CXC_LocalReferenceInfo.AddError(error);
				}
			}
		}

		protected override void CheckCXC_Status()
		{
			base.CheckCXC_Status();

			var status = Parent.CXC_Status;
			if (!status.IsEmpty && status.Length != 3)
			{
				Parent.CXC_StatusInfo.AddError(Res.GetString("B23D6580-27FD-4CC6-A4A4-A986D3CFC742", "Status must have 3 characters"));
			}
		}
	}
}
