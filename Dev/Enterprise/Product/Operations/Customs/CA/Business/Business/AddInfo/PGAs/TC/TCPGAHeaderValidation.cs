using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class TCPGAHeaderValidation : CusAddInfoValidation
	{
		public TCPGAHeaderValidation(AutoCusAddInfo parent)
			: base(parent)
		{
		}

		TCPGAHeader PGAHeader => (TCPGAHeader)Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateManufacturerLetterAttached();
			ValidateStatementLabelAttached();
			ValidateIsZZImporterDeclared();
			ValidateIsUSImporterDeclared();
			ValidateIsVPRImporterDeclared();
		}

		public void ValidateManufacturerLetterAttached()
		{
			ValidateCalculatedProperty(PGAHeader.ManufacturerLetterAttachedInfo);
		}

		public void ValidateStatementLabelAttached()
		{
			ValidateCalculatedProperty(PGAHeader.StatementLabelAttachedInfo);
		}

		protected void CheckManufacturerLetterAttached()
		{
			if (PGAHeader.CA_VPRProgramInd == YesNoList.Codes.Yes
				&& !PGAHeader.ManufacturerLetterAttached
				&& BothShowStatementLabelAndManufacturerLetter()
				&& !PGAHeader.StatementLabelAttached)
			{
				PGAHeader.ManufacturerLetterAttachedInfo.AddMessageError(NeedCriteriaConformanceMessageError);
			}
		}

		protected void CheckStatementLabelAttached()
		{
			if (PGAHeader.CA_VPRProgramInd == YesNoList.Codes.Yes
				&& !PGAHeader.StatementLabelAttached)
			{
				if (PGAHeader.CA_SubProgram == TCPGAVehicleProgramCodes.Codes.PIL)
				{
					PGAHeader.StatementLabelAttachedInfo.AddMessageError(Res.GetString("0c7ba2d1-4ec9-44b6-906b-2dae559798c4", "Statement of Compliance Label should be provided."));
				}
				else if (BothShowStatementLabelAndManufacturerLetter() && !PGAHeader.ManufacturerLetterAttached)
				{
					PGAHeader.StatementLabelAttachedInfo.AddMessageError(NeedCriteriaConformanceMessageError);
				}
			}
		}

		public void ValidateIsZZImporterDeclared()
		{
			ValidateCalculatedProperty(PGAHeader.IsZZImporterDeclaredInfo);
		}

		protected void CheckIsZZImporterDeclared()
		{
			if (!PGAHeader.IsZZImporterDeclared
				&& PGAHeader.CA_TPRProgramInd == YesNoList.Codes.Yes
				&& PGAHeader.ParentCountryOfOrigin != Core.Constants.CountryCodes.UnitedStates
				&& PGAHeader.ZZImporterDeclarationVisibility)
			{
				PGAHeader.IsZZImporterDeclaredInfo.AddMessageError(Res.GetString("83caa0b3-86db-4e4f-a61b-1ec8ab6552c3", "Importer Declaration should be provided."));
			}
			else
			{
				CheckUSImporterDeclaration(PGAHeader.IsZZImporterDeclaredInfo);
			}
		}

		public void ValidateIsVPRImporterDeclared()
		{
			ValidateCalculatedProperty(PGAHeader.IsVPRImporterDeclaredInfo);
		}

		protected void CheckIsVPRImporterDeclared()
		{
			if (!PGAHeader.IsVPRImporterDeclared && PGAHeader.VPRImporterShouldProvided)
			{
				PGAHeader.IsVPRImporterDeclaredInfo.AddMessageError(Res.GetString("62f5059b-c943-456a-9f9c-87e55260943e", "Importer Statement should be provided."));
			}
		}

		public void ValidateIsUSImporterDeclared()
		{
			ValidateCalculatedProperty(PGAHeader.IsUSImporterDeclaredInfo);
		}

		protected void CheckIsUSImporterDeclared()
		{
			CheckUSImporterDeclaration(PGAHeader.IsUSImporterDeclaredInfo);
		}

		void CheckUSImporterDeclaration(ZPropertyInfo info)
		{
			if (!PGAHeader.IsZZImporterDeclared
				&& !PGAHeader.IsUSImporterDeclared
				&& PGAHeader.CA_TPRProgramInd == YesNoList.Codes.Yes
				&& PGAHeader.ParentCountryOfOrigin == Core.Constants.CountryCodes.UnitedStates
				&& PGAHeader.USImporterDeclarationVisibility)
			{
				info.AddMessageError(Res.GetString("0d1101da-c6cc-4ec5-90ef-0018a902bd73", "Either Importer Declaration for All Countries or Importer Declaration for US should be provided."));
			}
		}

		bool BothShowStatementLabelAndManufacturerLetter()
		{
			return PGAHeader.CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VCC
					|| PGAHeader.CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VFS
					|| PGAHeader.CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VAE
					|| PGAHeader.CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VCR
					|| PGAHeader.CA_SubProgram == TCPGAVehicleProgramCodes.Codes.VFC;
		}

		static string NeedCriteriaConformanceMessageError => Res.GetString("d12b7461-937c-45f7-ba53-36bdf2c6254d", "Either Statement of Compliance Label or Manufacturer Letter of Compliance Label should be provided.");
	}
}
