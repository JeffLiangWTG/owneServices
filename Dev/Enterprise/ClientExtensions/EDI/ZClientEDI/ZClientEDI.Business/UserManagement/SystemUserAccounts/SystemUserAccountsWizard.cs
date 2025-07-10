using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	public class SystemUserAccountsWizard : NonPersistentBusinessObject
	{
		public SystemUserAccountsWizard(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public EdiCustomerUserAccountCollection EdiCustomerUserAccountCollection
		{
			get
			{
				if (ediCustomerUserAccountCollection == null)
				{
					var query = new ZQuery();
					query.MaximumRows = SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.Value;
					ediCustomerUserAccountCollection = new EdiCustomerUserAccountCollection(Factory, query);
					RegisterEditableChildObject(ediCustomerUserAccountCollection);
				}

				return ediCustomerUserAccountCollection;
			}
		}
		EdiCustomerUserAccountCollection ediCustomerUserAccountCollection;

		public OrgContactCollection OrgContactCollection
		{
			get
			{
				if (orgContactCollection == null)
				{
					orgContactCollection = new OrgContactCollection(Factory);
					RegisterEditableChildObject(orgContactCollection);
				}

				return orgContactCollection;
			}
		}
		OrgContactCollection orgContactCollection;

		public void RefreshCollection()
		{
			var query = new ZQuery();
			query.MaximumRows = SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.Value;
			var localEdiCustomerUserAccountCollection = new EdiCustomerUserAccountCollection(Factory, query);
			localEdiCustomerUserAccountCollection.Load();
			ediCustomerUserAccountCollection = localEdiCustomerUserAccountCollection;
			RegisterEditableChildObject(ediCustomerUserAccountCollection);
		}

		public EdiCustomerUserAccountCollection UserAccountLinkedToContact(EDIOrgContact orgContact)
		{
			var personPk = orgContact != null ? orgContact.OC_PER : ZGuid.Empty;
			var contactSubQuery = new ZDBOnlySubQuery(typeof(OrgContact), OrgContactSchema.PK);
			contactSubQuery.AddToFilter(OrgContactSchema.OC_PER, personPk);

			var query = new ZDBOnlyQuery(typeof(EdiCustomerUserAccount));
			query.AddSubQuery(EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact, contactSubQuery, JoinCondition.And);
			query.MaximumRows = SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.Value;
			return new EdiCustomerUserAccountCollection(Factory, query);
		}
	}
}
