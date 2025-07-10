using System.Collections.Generic;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.MasterData.GUI;

namespace Enterprise.Client.EDI.MasterFiles.GUI.Deduplication
{
	public sealed class EDIDeduplicationResultsViewerForm : DeduplicationResultsViewerForm
	{
		public EDIDeduplicationResultsViewerForm(object master, IEnumerable<object> targets, IEnumerable<ScoringResult> results, IEnumerable<PatternMatchingResultModel> resultModels, ZGuid selectedItemPK) : base(master, targets, results, resultModels, selectedItemPK)
		{
		}
	}
}

