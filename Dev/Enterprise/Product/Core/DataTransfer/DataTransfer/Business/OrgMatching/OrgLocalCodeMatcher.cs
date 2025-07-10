using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Business
{
	public interface IOrgLocalCodeMatcher
	{
		OrgHeader Match(ZString orgCode, IOrgHeaderForMatching temporaryOrganisation);
	}

	public class OrgLocalCodeMatcher : IOrgLocalCodeMatcher
	{
		public OrgLocalCodeMatcher(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}
		readonly BusinessObjectFactory factory;

		public OrgHeader Match(ZString orgCode, IOrgHeaderForMatching temporaryOrganisation)
		{
			if (orgCode.IsEmpty)
			{
				return null;
			}

			var query = new ZQuery(OrgHeaderSchema.OH_Code, orgCode);
			IEnumerable<OrgHeader> matches = (OrgHeader[])factory.Load(typeof(OrgHeader), query);
			if (temporaryOrganisation != null)
			{
				matches = matches.Where(m => m.PK != temporaryOrganisation.PK);
			}

			return matches.Count() != 1 ? null : matches.First();
		}
	}
}

