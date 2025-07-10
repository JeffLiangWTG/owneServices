using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.GVMS
{
	public class GvmsItemReferenceValidation : CusSupportingInfoValidation
	{
		public GvmsItemReferenceValidation(GvmsItemReference parent) : base(parent)
		{
		}

		protected override void CheckCSI_ReferenceNumber2()
		{
			base.CheckCSI_ReferenceNumber2();

			var parent = Parent;
			if (!parent.CSI_ReferenceNumber2.IsEmpty && !Regex.IsMatch(parent.CSI_ReferenceNumber2, "^[A-Z0-9-]{1,200}$"))
			{
				parent.CSI_ReferenceNumber2Info.AddMessageError("If supplied, the S&S reference must be made of capital letters, numbers or hyphen only.");
			}
		}

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();

			var parent = (GvmsItemReference)Parent;
			var header = (AsycudaManifestHeader)parent.Parent;
			if (header != null)
			{
				var headerNature = header.AMA_Nature;

				if (parent.CSI_Code == GVMSCustomsReference.Codes.IndirectExportDeclarationEad &&
					(headerNature == GVMSManifestNature.Codes.Import || header.AMA_Nature == GVMSManifestNature.Codes.GBtoNI))
				{
					parent.CSI_CodeInfo.AddMessageError("EAD is not allowed for this manifest nature (direction)");
				}

				if (parent.CSI_Code == GVMSCustomsReference.Codes.SSReferenceForEmptyVehicle && header.EmptyVehicle == "")
				{
					parent.CSI_CodeInfo.AddMessageError("Type MT is only allowed for empty vehicles");
				}
			}

			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_CodeInfo);
		}
	}
}
