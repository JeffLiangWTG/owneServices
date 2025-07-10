using System;
using System.Collections.Concurrent;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine
{
	public class eDocsWebSecurity
	{
		public eDocsWebSecurity(BusinessObjectFactory factory, ZGuid? contactPK)
		{
			securityFactory = factory ?? throw new ArgumentNullException(nameof(factory));
			securityContactPK = contactPK;
			cachedRights = new ConcurrentDictionary<RefDocType, bool>();
		}

		public eDocsWebSecurity(BusinessObjectFactory factory, OrgContact contact)
			: this(factory, contact?.PK)
		{
			this.contact = contact;
		}

		public bool CanViewDocument(RefDocType docType)
		{
			if (!securityContactPK.HasValue)
			{
				return true;
			}
			if (Contact == null)
			{
				return false;
			}

			if (docType == null)
			{
				return false;
			}

			return cachedRights.GetOrAdd(docType, key => Contact.IsRightGrantedWithCheckingSecurityGroups(DocumentRights.GetSecurityRight(docType)));
		}

		DocumentWebSecurityRights DocumentRights => documentRights ??= new DocumentWebSecurityRights(securityFactory);
		OrgContact Contact => contact ??= securityContactPK.HasValue ? securityFactory.Load<OrgContact>(securityContactPK.Value) : contact;

		OrgContact contact;
		DocumentWebSecurityRights documentRights;

		readonly BusinessObjectFactory securityFactory;
		readonly ZGuid? securityContactPK;
		readonly ConcurrentDictionary<RefDocType, bool> cachedRights;
	}
}
