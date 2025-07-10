using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

sealed class CustomsProcedureWrapper : ICustomsProcedure
{
	public CustomsProcedureWrapper(
		ZString fullProcedureCode,
		ZString[] additionalProcedureCodes,
		bool addDefaultAdditionalProcedureIfNone)
	{
		Argument.NotNull(additionalProcedureCodes, nameof(additionalProcedureCodes));

		lazyProcedure = new Lazy<string>(GetProcedure(fullProcedureCode));
		lazyPreviousProcedure = new Lazy<string>(GetPreviousProcedure(fullProcedureCode));
		lazyAdditionalProcedures = new Lazy<IReadOnlyCollection<string>>(GetAdditionalProcedures(fullProcedureCode, additionalProcedureCodes, addDefaultAdditionalProcedureIfNone));
	}

	string ICustomsProcedure.Procedure => lazyProcedure.Value;
	readonly Lazy<string> lazyProcedure;

	string ICustomsProcedure.PreviousProcedure => lazyPreviousProcedure.Value;
	readonly Lazy<string> lazyPreviousProcedure;

	IReadOnlyCollection<string> ICustomsProcedure.AdditionalProcedures => lazyAdditionalProcedures.Value;
	readonly Lazy<IReadOnlyCollection<string>> lazyAdditionalProcedures;

	Func<string> GetProcedure(ZString fullProcedureCode) => () => fullProcedureCode.SubstringSafe(0, ProcedureLength);

	Func<string> GetPreviousProcedure(ZString fullProcedureCode) => () => fullProcedureCode.SubstringSafe(ProcedureLength, PreviousProcedureLength);

	Func<IReadOnlyCollection<string>> GetAdditionalProcedures(ZString fullProcedureCode, ZString[] additionalProcedureCodes, bool addDefaultAdditionalProcedureIfNone)
	{
		return () =>
		{
			string ExtractAdditionalProcedure(ZString procedureCode) => procedureCode.SubstringSafe(ProcedureLength + PreviousProcedureLength, AdditionalProcedureLength).ToString();

			var extractedAdditionalProcedures = new List<string>();
			extractedAdditionalProcedures.Add(ExtractAdditionalProcedure(fullProcedureCode));
			extractedAdditionalProcedures.AddRange(additionalProcedureCodes.Select(ExtractAdditionalProcedure));

			extractedAdditionalProcedures = extractedAdditionalProcedures.WhereNotNullOrEmpty().ToList();
			if (addDefaultAdditionalProcedureIfNone && extractedAdditionalProcedures.Count == 0)
			{
				extractedAdditionalProcedures.Add(EmptyAdditionalProcedure);
			}
			return extractedAdditionalProcedures;
		};
	}

	const int ProcedureLength = 2;
	const int PreviousProcedureLength = 2;
	const int AdditionalProcedureLength = 3;
	const string EmptyAdditionalProcedure = "1NN";
}
