using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

public class GroupedMissingSupportingDocument
{
	public GroupedMissingSupportingDocument(RefCusCondition condition)
	{
		this.condition = Argument.NotNull(condition, nameof(condition));
		MissingSupportingDocuments = new List<MissingSupportingDocument>();
	}
	readonly RefCusCondition condition;

	public ZString ConditionTypeDescription => condition.ConditionTypeDescription;
	public ZString ConditionTypeComment => condition.ZX1_Comment;
	public List<MissingSupportingDocument> MissingSupportingDocuments { get; }

	public ZBoolDescriptionPairList MissingSupportingDocumentsBoolDescriptionPairList => missingSupportingDocumentsBoolDescriptionPairList ?? (missingSupportingDocumentsBoolDescriptionPairList = GetMissingSupportingDocumentsPairList());
	ZBoolDescriptionPairList missingSupportingDocumentsBoolDescriptionPairList;

	ZBoolDescriptionPairList GetMissingSupportingDocumentsPairList()
	{
		var boolDescriptionPairList = new ZBoolDescriptionPairList();
		foreach (var missingSupportingDocument in MissingSupportingDocuments.OrderBy(x => x.Code))
		{
			var missingSupportingDocumentBoolDescriptionPair = new ZBoolDescriptionPair(missingSupportingDocument.CodeAndDescription, missingSupportingDocument.ShouldImport);
			missingSupportingDocumentBoolDescriptionPair.OnChanged += (s, e) => missingSupportingDocument.ShouldImport = missingSupportingDocumentBoolDescriptionPair.Value;
			boolDescriptionPairList.Add(missingSupportingDocumentBoolDescriptionPair);
		}
		return boolDescriptionPairList;
	}
}
