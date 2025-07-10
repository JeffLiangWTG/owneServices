using Enterprise.MasterFiles.Integration;

namespace Enterprise.UniversalDataBuss.Management.Matching
{
	public class OrganizationMatchResult : LoggingMatchResult<IOrgHeader>
	{
		public new OrganizationMatchResult SetMatch(IOrgHeader matchFound)
		{
			base.SetMatch(matchFound);
			return this;
		}
	}
}
