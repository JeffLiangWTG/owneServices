using System.Linq;

namespace Enterprise.Customs.GB.GVMS
{
	public class GvmsCustomsReferenceValidation : GvmsItemReferenceValidation
	{
		public GvmsCustomsReferenceValidation(GvmsItemReference parent) : base(parent)
		{
		}

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();

			var parent = (GvmsItemReference)Parent;
			if (parent.CSI_Code == GVMSCustomsReference.Codes.SSReferenceForEmptyVehicle || parent.CSI_Code == GVMSCustomsReference.Codes.ImportControlSystemEntrySummaryDeclaration)
			{
				var header = (AsycudaManifestHeader)parent.Parent;
				if (header != null)
				{
					var referenceTypeExists = header.GvmsCustomsReferenceCollection.Cast<GvmsItemReference>().Any(x => x.CSI_Code == parent.CSI_Code && x.PK != parent.PK);
					if (referenceTypeExists)
					{
						parent.CSI_CodeInfo.AddMessageError(string.Format("Only one reference of type {0} is allowed", parent.CSI_Code));
					}
				}
			}
		}
	}
}
