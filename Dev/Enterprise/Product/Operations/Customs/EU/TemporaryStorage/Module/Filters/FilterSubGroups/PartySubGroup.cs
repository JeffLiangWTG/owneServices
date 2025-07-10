using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Module;

sealed class PartySubGroup : FilterSubGroup
{
	readonly ZString _partyType;

	public PartySubGroup(string partyType)
	{
		_partyType = partyType;
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Use of Filter Constants")]
	public override ZQuery GetSubQuery(ZQuery filter)
	{
		var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
		orgAddressQuery.AddToFilter(filter);

		var partySchemaColumn = _partyType.ToString() switch
		{
			"Consignee" => AsycudaBillSchema.ABL_OA_Consignee,
			"Consignor" => AsycudaBillSchema.ABL_OA_Shipper,
			"Notify" => AsycudaBillSchema.ABL_OA_NotifyParty,
			_ => HandleInvalidPartyType(),
		};

		if (partySchemaColumn is null)
		{
			return filter;
		}

		return GetAsycudaMainQuery(partySchemaColumn, orgAddressQuery);
	}

	SchemaGuidColumn HandleInvalidPartyType()
	{
		var message = $"Invalid Party Type: {_partyType}";
		ExceptionReporter.Instance.ReportDeveloperException("e5ea126e-242c-4a36-9994-ac52255befb4", message, new InvalidOperationException(message));
		return null;
	}
}
