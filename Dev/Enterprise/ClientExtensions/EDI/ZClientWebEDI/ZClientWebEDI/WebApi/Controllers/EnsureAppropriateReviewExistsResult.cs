using System;
using System.Collections.Generic;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class EnsureAppropriateReviewExistsResult
	{
		public static EnsureAppropriateReviewExistsResult AppropriateReviewDoesExist => new EnsureAppropriateReviewExistsResult { AppropriateReviewExists = true };

		public bool AppropriateReviewExists { get; set; }
		public IReadOnlyList<Guid> IncompleteAspects { get; set; } = [];
		public IReadOnlyList<string> MissingWiseTechAcademySubjects { get; set; } = [];
		public IReadOnlyList<string> MissingWiseTechAcademyCourseUrls { get; set; } = [];
		public IReadOnlyList<string> MissingCompetencies { get; set; } = [];
	}
}
