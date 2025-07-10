using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	public class OrgHeaderTCPMessageWrapper : NonPersistentBusinessObject
	{
		protected OrgHeaderTCPMessageWrapper(OrgHeader organisation)
			: base(organisation.Factory)
		{
			this.organisation = organisation;
		}

		#region static New

		public static OrgHeaderTCPMessageWrapper New(OrgHeader organisation)
		{
			OrgHeaderTCPMessageWrapper result = null;

			if (organisation != null)
			{
				result = organisation.Factory.GetCachedValue(organisation.PK.ToStringKey(), delegate
				{
					return new OrgHeaderTCPMessageWrapper(organisation);
				});
			}

			return result;
		}

		#endregion

		public OrgHeaderWrapperTCPMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = new OrgHeaderWrapperTCPMessageCollection(organisation);
					fMessages.Load();
					RegisterEditableChildObject(fMessages);
				}
				return fMessages;
			}
		}
		OrgHeaderWrapperTCPMessageCollection fMessages;

		public readonly OrgHeader organisation;
	}
}
