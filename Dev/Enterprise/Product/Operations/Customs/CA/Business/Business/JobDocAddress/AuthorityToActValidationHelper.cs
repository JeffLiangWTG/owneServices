using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	class AuthorityToActValidationHelper
	{
		#region CheckIOROrgPK

		public static void Validate(ZPropertyInfo info, JobDeclaration dec, Func<OrgHeader> orgGetter, ZString direction, ZString port)
		{
			new AuthorityToActValidator().Validate(dec, orgGetter(), info,
				Core.Constants.RefDocTypes.PowerOfAttorney,
				Core.Constants.RefDocTypes.PowerOfAttorneyCustoms,
				ZString.Empty, ExtraMatchingConditionForDirectionAndPortOfEntry(direction, port));
		}

		public static List<(Predicate<JobRequiredDocument>, string)> ExtraMatchingConditionForDirectionAndPortOfEntry(string direction, string portOfEntry)
		{
			return new List<(Predicate<JobRequiredDocument>, string)>()
			{
				AuthorityToActValidator.HasNoDirectionAttributeOrHasMatchedDirectionAttribute(direction),
				AuthorityToActValidator.HasNoPortOfEntryAttributeOrHasMatchedPortOfEntryAttribute(portOfEntry)
			};
		}

		#endregion
	}
}
