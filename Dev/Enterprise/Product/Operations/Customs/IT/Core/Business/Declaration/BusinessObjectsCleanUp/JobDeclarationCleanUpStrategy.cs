using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

public sealed class JobDeclarationCleanUpStrategy : ICleanUpStrategy
{
	public JobDeclarationCleanUpStrategy(JobDeclaration declaration)
	{
		this.declaration = Argument.NotNull(declaration, nameof(declaration));
	}

	readonly JobDeclaration declaration;

	void ICleanUpStrategy.CleanUp()
	{
		declaration.ZG_SpecificCircumstanceIndicator = ZString.Empty;

		if (declaration.IsUCC6)
		{
			declaration.JE_GS_NKCusAgent = ZString.Empty;
			CleanupPreviousDocuments();
			CleanupSupportingDocuments();

			CleanupDataNotApplicableForUcc6AndIfExport();
			CleanupDataNotApplicableForUcc6AndIfImport();
		}
		else
		{
			declaration.DeleteGoodsLocation();
			declaration.ZG_IsSecurityDeclaration = false;
			declaration.JE_TransportMeans = ZString.Empty;
		}

		if (!declaration.IsUCC6AndIsExport)
		{
			declaration.ZG_BorderTransportMeans = ZString.Empty;
		}
	}

	#region Implementation

	void CleanupDataNotApplicableForUcc6AndIfExport()
	{
		if (!declaration.IsExport)
		{
			return;
		}

		CleanUpAuthorisationNumber();

		declaration.ZG_CTStatusID = ZString.Empty;
		declaration.JE_SubLocationOfGoods = ZString.Empty;
		declaration.ZG_Box18TransportID = ZString.Empty;
		declaration.ZG_Box18TransportNationality = ZString.Empty;
	}

	void CleanupPreviousDocuments()
	{
		declaration.PreviousDocuments.RemoveAndDeleteAll();
	}

	void CleanupSupportingDocuments()
	{
		declaration.SupportingDocuments.RemoveAndDeleteAll();
	}

	void CleanUpAuthorisationNumber()
	{
		declaration.ZG_AuthorisationNumber = ZString.Empty;
	}

	void CleanupDataNotApplicableForUcc6AndIfImport()
	{
		if (!declaration.IsImport)
		{
			return;
		}
		declaration.JE_VesselName = ZString.Empty;
		declaration.JE_VoyageFlightNo = ZString.Empty;
	}

	#endregion
}
