using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	class DeclarationJobDocAddressValidation : JobDocAddressValidation
	{
		public DeclarationJobDocAddressValidation(JobDocAddress address, JobDeclaration declaration)
			: base(address)
		{
			this.declaration = declaration;
		}

		readonly JobDeclaration declaration;

		protected override void CheckE2_GovRegNum()
		{
			base.CheckE2_GovRegNum();

			var parent = Parent;
			if (declaration.IsExport && parent.E2_AddressType == DocAddressTypes.Codes.ClearanceLocalInvolvedParty && parent.E2_AddressOverride && !parent.E2_GovRegNum.IsEmpty)
			{
				if (!CNPJValidator.ValidateCNPJ(parent.E2_GovRegNum))
				{
					parent.E2_GovRegNumInfo.AddMessageError(Res.GetString("302fc5ef-43d0-4d32-96c1-2698cea6024c", "The entered CNPJ is not valid."));
				}
			}
		}

		protected override void CheckE2_GeoLocation()
		{
			base.CheckE2_GeoLocation();

			ValidateCalculatedProperty(Parent.E2_LatitudeInfo);
			ValidateCalculatedProperty(Parent.E2_LongitudeInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Method is used via reflection (see InvokeValidationMethod() in ZValidation)")]
		void CheckE2_Latitude()
		{
			if (declaration.IsExport && Parent.E2_AddressType == DocAddressTypes.Codes.ClearanceLocalInvolvedParty && Parent.E2_AddressOverride)
			{
				if (Parent.E2_Latitude == 0)
				{
					Parent.E2_LatitudeInfo.AddMessageError(Res.GetString("E121C9D2-C4F6-476D-8686-54BB2D93E416", "You have not entered a valid Latitude."));
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Method is used via reflection (see InvokeValidationMethod() in ZValidation)")]
		void CheckE2_Longitude()
		{
			if (declaration.IsExport && Parent.E2_AddressType == DocAddressTypes.Codes.ClearanceLocalInvolvedParty && Parent.E2_AddressOverride)
			{
				if (Parent.E2_Longitude == 0)
				{
					Parent.E2_LongitudeInfo.AddMessageError(Res.GetString("07F960E5-ED28-4C50-8E60-C68B66CF5E9E", "You have not entered a valid Longitude."));
				}
			}
		}

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();

			var parent = Parent;
			var targetInfo = parent.OrganisationPKInfo;

			if (declaration.IsExport && parent.E2_AddressType == DocAddressTypes.Codes.ClearanceLocalInvolvedParty && !parent.E2_AddressOverride)
			{
				if (parent.HasRealOrganisation)
				{
					if (parent.E2_Longitude == 0)
					{
						targetInfo.AddMessageError(Res.GetString("20E762F2-E014-4587-8A4D-9A3F8D0F34B9", "Clearance Local Address does not have a valid GPS> Longitude."));
					}

					if (parent.E2_Latitude == 0)
					{
						targetInfo.AddMessageError(Res.GetString("89780BBB-7093-4390-B381-0B7130B96039", "Clearance Local Address does not have a valid GPS> Latitude."));
					}
				}

				if (!declaration.ClearanceOfficeIsCustomsEnclosure)
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo, DocAddressTypes.Descriptions.ClearanceLocalInvolvedParty);
				}
			}
		}
	}
}
