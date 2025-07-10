using CargoWise.EntityFramework;
using Enterprise.MasterData.Common;

namespace Enterprise.AuditDataServices.MDM.Subscribers
{
	public static class PatternMatchingSubscriberUtilitiesFactory<T> where T : BusinessObject, IPatternMatchingBusinessObjects
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
		public static IPatternMatchingSubscriberUtilities Provider(PatternMasterType masterType)
		{
			IPatternMatchingSubscriberUtilities patternMatchingSubscriberUtilities;

			if (masterType == PatternMasterType.GlbPerson)
			{
				patternMatchingSubscriberUtilities = new PersonPatternMatchingSubscriberUtilities<T>();
			}
			else
			{
				patternMatchingSubscriberUtilities = new OrgPatternMatchingSubscriberUtilities<T>();
			}

			return patternMatchingSubscriberUtilities;
		}
	}
}
