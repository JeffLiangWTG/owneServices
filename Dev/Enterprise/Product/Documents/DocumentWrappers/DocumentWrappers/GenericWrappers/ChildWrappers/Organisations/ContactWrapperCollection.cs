using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DocumentEngineCore.DocWrappers.CustomIndexerList(typeof(ContactTypeList))]
	public class ContactWrapperCollection : GenericWrapperCollection<ContactWrapper>
	{
		public ContactWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ContactWrapperCollection(OrgHeader organisation, BusinessObjectFactory factory)
			: base(factory)
		{
			if (organisation != null)
			{
				Organisation = organisation;
				Load(organisation.Contacts);
			}
		}
		readonly OrgHeader Organisation;

		protected override DocumentEngineCore.DocWrappers.DocumentWrapper WrapObject(object objectToWrap)
		{
			return new ContactWrapper((OrgContact)objectToWrap, Factory);
		}

		protected override DocumentEngineCore.DocWrappers.IBODocDataProvider GetRow(ZString index)
		{
			if (Organisation != null)
			{
				ContactType contactType = new ContactTypeList().GetContactType(index);
				if (contactType != null)
				{
					ContactWrapper result = null;
					if (!ContactWrapperCache.TryGetValue(contactType, out result))
					{
						result = GetContactWrapper(contactType);
						ContactWrapperCache[contactType] = result;
					}
					return result;
				}
			}
			return base.GetRow(index);
		}

		Dictionary<ContactType, ContactWrapper> ContactWrapperCache
		{
			get
			{
				if (fContactWrapperCache == null)
				{
					fContactWrapperCache = new Dictionary<ContactType, ContactWrapper>();
				}
				return fContactWrapperCache;
			}
		}
		Dictionary<ContactType, ContactWrapper> fContactWrapperCache;

		ContactWrapper GetContactWrapper(ContactType contactType)
		{
			OrgContact contactBO = new DefaultContactFinder(Organisation, true).DefaultContact(contactType);
			foreach (ContactWrapper wrapper in this)
			{
				if (contactBO == wrapper.WrappedObject)
				{
					return wrapper;
				}
			}
			return new ContactWrapper(contactBO, Organisation, Factory);
		}
	}
}
