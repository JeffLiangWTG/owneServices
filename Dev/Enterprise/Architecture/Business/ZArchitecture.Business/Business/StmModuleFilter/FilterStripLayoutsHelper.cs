using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
	public class FilterStripLayoutsHelper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ZString CurrentUserTablePrefix
		{
			get { return GetCurrentUserTablePrefix(); }
		}

		public ZGuid CurrentUserPk
		{
			get { return GetCurrentUserPk(); }
		}

		public ZGuid CurrentOrganisationPk
		{
			get { return GetCurrentOrganisationPk(); }
		}

		public ZGuid AdditionalEntityID
		{
			get { return GetAdditionalEntityID(); }
		}

		protected virtual ZString GetCurrentUserTablePrefix()
		{
			return GlbStaffSchema.Constants.Prefix;
		}

		protected virtual ZGuid GetCurrentUserPk()
		{
			return EnvProxy.Instance.CurrentUser.PK;
		}

		protected virtual ZGuid GetCurrentOrganisationPk()
		{
			throw new NotSupportedException("CurrentOrganisationPk is not supported in FilterStripLayoutsHelper"); // Exception Message Text
		}

		protected virtual ZGuid GetAdditionalEntityID()
		{
			return ZGuid.Empty;
		}
	}
}
