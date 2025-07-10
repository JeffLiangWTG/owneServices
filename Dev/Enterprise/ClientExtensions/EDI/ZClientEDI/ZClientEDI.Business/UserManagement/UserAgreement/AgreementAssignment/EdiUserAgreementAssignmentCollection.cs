using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	public class EdiUserAgreementAssignmentCollection : ActiveBusinessObjectCollection<EdiUserAgreementAssignment>
	{
		public EdiUserAgreementAssignmentCollection(BusinessObject parent) : base(parent.Factory)
		{
			Argument.NotNull(parent, "parent");

			this.parent = parent;
		}

		public EdiUserAgreementAssignmentCollection(BusinessObject parent, ZQuery filter) : base(parent.Factory, filter)
		{
			Argument.NotNull(parent, "parent");
			this.parent = parent;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = new ZQuery();

			if (parent is LicenceEnterprise parentEnterprise)
			{
				query.AddToFilter(EdiUserAgreementAssignmentSchema.EAE_ParentTableCode, new string[] { LicenceEnterpriseSchema.Constants.Prefix, LicenceDatabaseSchema.Constants.Prefix });
				var parentPKs = parentEnterprise.Databases.GetPKs();
				parentPKs.Add(parentEnterprise.PK);
				parentPKs.Add(ZGuid.Empty);
				parentPKs.Add(ZGuid.Missing);
				query.AddToFilter(EdiUserAgreementAssignmentSchema.EAE_ParentID, parentPKs);
			}
			else
			{
				query.AddToFilter(EdiUserAgreementAssignmentSchema.EAE_ParentTableCode, parent.TablePrefix);
				query.AddToFilter(EdiUserAgreementAssignmentSchema.EAE_ParentID, parent.PK);
			}

			return query;
		}

		protected override void SetDefaultsForNewElementCore(EdiUserAgreementAssignment newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			if (Parent is LicenceEnterprise enterprise)
			{
				newElement.EnterpriseParent = (LicenceEnterprise)Parent;
			}

			newElement.Parent = parent;
			newElement.EAE_ParentTableCodeInfo.ValueChanged += SetParentIfEnterprise;
		}

		void SetParentIfEnterprise(object sender, EventArgs e)
		{
			var assignment = ((EdiUserAgreementAssignment)sender);
			if (Parent is LicenceEnterprise enterprise && assignment.EAE_ParentTableCode == LicenceEnterpriseSchema.Constants.Prefix)
			{
				assignment.EAE_ParentID = enterprise.PK;
			}
		}

		protected override void OnLoadedIntoCollectionCore(EdiUserAgreementAssignment loadedObject)
		{
			base.OnLoadedIntoCollectionCore(loadedObject);

			if (Parent is LicenceEnterprise enterprise)
			{
				loadedObject.EnterpriseParent = (LicenceEnterprise)Parent;
			}

			loadedObject.EAE_ParentTableCodeInfo.ValueChanged += SetParentIfEnterprise;
		}

		readonly BusinessObject parent;

		public BusinessObject Parent => parent;
	}
}
