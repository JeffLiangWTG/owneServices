using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

public sealed class JobDeclarationTransportModeInlandCleanUpStrategy : ICleanUpStrategy
{
	public JobDeclarationTransportModeInlandCleanUpStrategy(JobDeclaration declaration)
	{
		this.declaration = Argument.NotNull(declaration, nameof(declaration));
	}

	readonly JobDeclaration declaration;

	void ICleanUpStrategy.CleanUp()
	{
		declaration.ZG_SpecificCircumstanceIndicator = ZString.Empty;

		if (declaration.IsUCC6AndIsExport)
		{
			CleanUpAircraftRegistrationInlandIfNotApplicable();
			CleanUpTrailer1RegNoIfNotApplicable();
			CleanUpTrailer2RegNoIfNotApplicable();
			CleanUpTrailer1NationalityIfNotApplicable();
			CleanUpTrailer2NationalityIfNotApplicable();
		}
	}

	#region Implementation

	void CleanUpAircraftRegistrationInlandIfNotApplicable()
	{
		if (!declaration.IsAirInland)
		{
			declaration.JE_AircraftRegistrationInland = ZString.Empty;
		}
	}

	void CleanUpTrailer1RegNoIfNotApplicable()
	{
		if (!declaration.IsRailInland && !declaration.IsRoadInland)
		{
			declaration.JE_Trailer1RegNo = ZString.Empty;
		}
	}

	void CleanUpTrailer2RegNoIfNotApplicable()
	{
		if (!declaration.IsRoadInland)
		{
			declaration.JE_Trailer2RegNo = ZString.Empty;
		}
	}

	void CleanUpTrailer1NationalityIfNotApplicable()
	{
		if (!declaration.IsAirInland && !declaration.IsRailInland && !declaration.IsRoadInland)
		{
			declaration.JE_RN_NKTrailer1Nationality = ZString.Empty;
		}
	}

	void CleanUpTrailer2NationalityIfNotApplicable()
	{
		if (!declaration.IsRoadInland)
		{
			declaration.JE_RN_NKTrailer2Nationality = ZString.Empty;
		}
	}

	#endregion
}
