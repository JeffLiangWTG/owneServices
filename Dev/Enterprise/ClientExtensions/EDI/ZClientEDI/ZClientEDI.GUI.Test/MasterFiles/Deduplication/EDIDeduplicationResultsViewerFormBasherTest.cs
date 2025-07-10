using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Glow.Model.Interfaces;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.MasterData.Business;
using Enterprise.MasterData.GUI.Tests;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.GUI.Deduplication.Testing
{
	[TestedType(typeof(EDIDeduplicationResultsViewerForm))]
	public class EDIDeduplicationResultsViewerFormBasherTest : TestDeduplicationResultsViewerForm
	{
		protected override Form GetFormToBashCore()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var deduplicationOrgHeader = new DeduplicationOrgHeader(org2);
			var targetList = new List<IOrgHeader>() { deduplicationOrgHeader };
			org1.OH_Code = "XYZ";
			Factory.Save();

			var scoringResults = new List<ScoringResult>();
			var score = TargetScorerController.Score(
				new DeduplicationOrgHeader(org1),
				new DeduplicationOrgHeader(org2),
				true);
			scoringResults.Add(score);

			return new EDIDeduplicationResultsViewerForm(org1, targetList, scoringResults, new List<PatternMatchingResultModel>(), ZGuid.Empty);
		}
	}
}
