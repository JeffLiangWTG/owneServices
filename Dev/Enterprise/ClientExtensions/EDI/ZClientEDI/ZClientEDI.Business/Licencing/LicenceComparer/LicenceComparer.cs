using System.Collections.Generic;
using System.Text;
using Enterprise.Client.EDI.Licencing.Business;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class LicenceComparer
	{
		public LicenceComparer(LegacyLicence clientLicence, LegacyLicence ediLicence, bool includeObsoleteModules)
		{
			ParseAndCompareLicenceData(clientLicence, ediLicence, includeObsoleteModules);
		}

		public bool AreLicencesTheSame
		{
			get { return CheckpointDifferences.Count == 0; }
		}

		public string GetDescriptionOfDifferences()
		{
			StringBuilder result = new StringBuilder();
			foreach (LicenceCheckpointDifference diff in CheckpointDifferences)
			{
				result.AppendLine(diff.GetTextualDifference());
			}

			return result.ToString();
		}

		void ParseAndCompareLicenceData(LegacyLicence clientLicence, LegacyLicence ediLicence, bool includeObsoleteModules)
		{
			ParseAndCompareCheckpointData(clientLicence, ediLicence, includeObsoleteModules);
		}

		void ParseAndCompareCheckpointData(LegacyLicence clientLicence, LegacyLicence ediLicence, bool includeObsoleteModules)
		{
			foreach (var clientCheckpoint in clientLicence.GetAllCheckpoints())
			{
				if (includeObsoleteModules || (!includeObsoleteModules && !clientCheckpoint.DisplayName.Contains("OBSOLETE")))
				{
					var ediCheckPoint = ediLicence.GetCheckpointFromCode(clientCheckpoint.Name);
					LicenceCheckpointDifference diff = new LicenceCheckpointDifference(clientCheckpoint, ediCheckPoint);
					if (diff.ArePartsDifferent)
					{
						CheckpointDifferences.Add(diff);
					}
				}
			}
		}

		List<LicenceCheckpointDifference> CheckpointDifferences
		{
			get
			{
				if (checkPointDifferences == null)
				{
					checkPointDifferences = new List<LicenceCheckpointDifference>();
				}

				return checkPointDifferences;
			}
		}
		List<LicenceCheckpointDifference> checkPointDifferences;
	}
}

