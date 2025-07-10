using System.Collections.Generic;
using Enterprise.DbUpgrader.Transformation.DataModification.Public.Security;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Security;

public class DenyAccessUnpublishedCustomizedDocumentsAndReportsByDefault : DenyRootSecurityRightsByDefaultForGroups
{
	public override IEnumerable<string> SecurityRights
	{
		get
		{
			return new string[] { "AccessUnpublishedCustomizedDocumentsAndReports" };
		}
	}
}