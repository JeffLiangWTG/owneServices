using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business
{
	public class LocalExportJobDocAddressValidation : JobDocAddressValidation
	{
		public LocalExportJobDocAddressValidation(JobDocAddress parent, JobDeclaration declaration) : base(parent)
		{
			this.declaration = declaration;
		}

		protected override void CheckE2_OA_Address()
		{
			if (LocalExportTransactionNatureCodeList.IsSea(declaration.JE_MessageSubType) &&
				Parent.DocAddressType == MasterFiles.Integration.DocAddressType.Stevedore)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.E2_OA_AddressInfo);
			}
		}

		readonly JobDeclaration declaration;
	}
}
