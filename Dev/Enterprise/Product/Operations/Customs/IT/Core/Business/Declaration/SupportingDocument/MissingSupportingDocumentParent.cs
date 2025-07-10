#if NETFRAMEWORK
using CargoWise.Common;
#elif NET
using Argument = CargoWise.Common.Argument;
#endif
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IT.Business.Declaration;

public class MissingSupportingDocumentParent : NonPersistentBusinessObject
{
	MissingSupportingDocumentParent(JobComInvoiceLine invoiceLine)
		: base(invoiceLine.Factory)
	{
		this.invoiceLine = invoiceLine;
		Argument.NotNull(invoiceLine.Declaration, nameof(invoiceLine.Declaration));
	}
	readonly JobComInvoiceLine invoiceLine;

	public static MissingSupportingDocumentParent LoadNew(JobComInvoiceLine invoiceLine)
	{
		Argument.NotNull(invoiceLine, nameof(invoiceLine));
		var missingSupportingDocumentParent = new MissingSupportingDocumentParent(invoiceLine);
		missingSupportingDocumentParent.LoadMissingDocuments();
		return missingSupportingDocumentParent;
	}

	public IEnumerable<GroupedMissingSupportingDocument> MissingSupportingDocumentGrouped => missedSupportingDocumentDictionary.Values;

	void LoadMissingDocuments()
	{
		missedSupportingDocumentDictionary.Clear();

		var universalTariff = invoiceLine.UniversalTariff;

		if (universalTariff != null && Declaration is JobDeclaration declaration && (declaration.IsImport || declaration.IsExport))
		{
			var alreadyEnteredSupportingDocuments = invoiceLine.ActualSupportingDocumentCodeCollection.Select(x => x.CSI_Code).Distinct();

			var conditions = ConditionChecker.GetApplicableConditions(Factory, universalTariff, invoiceLine.ConditionSelectionCriterias);
			conditions.ToList().ForEach(condition => condition.FetchForLoadChildEditableObjectsIfNeeded());

			foreach (var condition in conditions.Where(x => x.ShouldStop(invoiceLine.EvaluateConditionValue, null)).OrderBy(x => x.CusConditionType.ZX2_Description).ThenBy(x => x.ZX1_Comment))
			{
				var orderedConditionValuesForMissingDocuments = GetSupAndSnrConditionValuesOrderedByValue(condition);

				foreach (var conditionValue in orderedConditionValuesForMissingDocuments)
				{
					var missedSupportingDocumentDictionaryKey = GetMissedSupportingDocumentDictionaryKey(condition);
					if (alreadyEnteredSupportingDocuments.Any(supportingDocumentCode => supportingDocumentCode == conditionValue.ZX3_Value))
					{
						missedSupportingDocumentDictionary.Remove(missedSupportingDocumentDictionaryKey);
						break;
					}
					else
					{
						AddMissingSupportingDocumentIfIsNotDuplicated(missedSupportingDocumentDictionaryKey, conditionValue);
					}
				}
			}
		}
	}

	public IEnumerable<MissingSupportingDocument> GetMissingSupportingDocumentsToImport() => MissingSupportingDocumentGrouped.SelectMany(x => x.MissingSupportingDocuments).DistinctBy(x => x.Code).Where(x => x.ShouldImport);

	public ZString TariffCode => invoiceLine.JI_Tariff;

	public ZBool ShouldImportDocumentsToAllInvoiceLinesWithSameTariff { get; set; }

	#region Implementation

	JobDeclaration Declaration => invoiceLine.Declaration;

	void AddMissingSupportingDocumentIfIsNotDuplicated(string missedSupportingDocumentDictionaryKey, RefCusConditionValue conditionValue)
	{
		var condition = conditionValue.Condition;

		var missingSupportingDocument = new MissingSupportingDocument(conditionValue, invoiceLine);

		if (!missedSupportingDocumentDictionary.ContainsKey(missedSupportingDocumentDictionaryKey))
		{
			missedSupportingDocumentDictionary.Add(missedSupportingDocumentDictionaryKey, new GroupedMissingSupportingDocument(condition));
		}

		var missingSupportingDocuments = missedSupportingDocumentDictionary[missedSupportingDocumentDictionaryKey].MissingSupportingDocuments;
		if (!missingSupportingDocuments.Any(x => x.Code == conditionValue.ZX3_Value))
		{
			missingSupportingDocuments.Add(missingSupportingDocument);
		}
	}

	IEnumerable<RefCusConditionValue> GetSupAndSnrConditionValuesOrderedByValue(RefCusCondition condition)
	{
		return condition.ConditionValues
			.Where(x => x.ConditionValueType != null && missingDocumentConditionalValueTypes.Contains(x.ConditionValueType.ZX4_ValueType))
			.OrderBy(x => x.ZX3_Value);
	}

	string GetMissedSupportingDocumentDictionaryKey(RefCusCondition condition) => FormattableString.Invariant($"{condition.CusConditionType?.ZX2_Description ?? ZString.Empty}_{condition.ZX1_Comment}");

	readonly Dictionary<string, GroupedMissingSupportingDocument> missedSupportingDocumentDictionary = new Dictionary<string, GroupedMissingSupportingDocument>();
	readonly ImmutableArray<string> missingDocumentConditionalValueTypes = new string[]
	{
		 Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument,
		 Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber
	}.ToImmutableArray();

	#endregion
}

