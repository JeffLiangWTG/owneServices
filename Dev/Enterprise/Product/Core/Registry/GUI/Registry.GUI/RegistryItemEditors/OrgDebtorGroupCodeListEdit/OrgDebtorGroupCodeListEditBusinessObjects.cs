using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.GUI
{
	#region OrgDebtorGroupCodeListElement

	public class OrgDebtorGroupCodeListElement : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public abstract class Schema
		{
			public const string GroupGuid = "GroupGuid";
			public const string GroupDescription = "GroupDescription";
		}

		#endregion

		public OrgDebtorGroupCodeListElement(ZGuid groupGuid, OrgDebtorGroupCodeListCollection parent)
			: base(new BusinessObjectFactory())
		{
			this.groupGuid = groupGuid;
			Parent = parent;
		}

		#region Collections

		public OrgDebtorGroupCollection OrgDebtorGroupCollection
		{
			get { return Parent.OrgDebtorGroupCollection; }
		}

		protected readonly OrgDebtorGroupCodeListCollection Parent;

		#endregion

		#region Properties

		#region Group Guid

		public ZGuid GroupGuid
		{
			get { return groupGuid; }
			set
			{
				groupGuid = value;
				if (!IsValidationSuspended)
				{
					GroupGuidInfo.ClearAllNotifications();
					TypeValidation.CheckValidGuid(GroupGuidInfo);
				}
				GroupGuidInfo.RefreshBinding();
			}
		}
		protected ZGuid groupGuid;

		public ZPropertyInfo GroupGuidInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.GroupGuid); }
		}

		#endregion

		#region DebtorGroup

		public OrgDebtorGroup DebtorGroup
		{
			get { return Factory.Load<OrgDebtorGroup>(GroupGuid); }
		}

		#endregion		

		#region Group Description

		public ZString GroupDescription
		{
			get
			{
				OrgDebtorGroup group = DebtorGroup;
				return (group == null) ? ZString.Empty : group.OJ_Desc;
			}
		}

		public ZPropertyInfo GroupDescriptionInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.GroupDescription); }
		}

		#endregion

		#endregion
	}

	#endregion

	#region OrgDebtorGroupCodeListCollection

	public class OrgDebtorGroupCodeListCollection : NonPersistentBusinessObjectCollection<OrgDebtorGroupCodeListElement>	{
		public OrgDebtorGroupCodeListCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgDebtorGroupCodeListCollection(Guid[] value)
			: this(new BusinessObjectFactory())
		{
			Load(value);
		}

		public void Load(Guid[] guidList)
		{
			foreach (Guid item in guidList)
			{
				try
				{
					ZGuid newGuid = new ZGuid(item);
					if (newGuid.IsValid && !newGuid.IsEmpty)
					{
						Add(new OrgDebtorGroupCodeListElement(newGuid, this));
					}
				}
				catch (ArgumentNullException)
				{
				}
				catch (FormatException)
				{
				}
			}
		}

		public Guid[] ToGuidArray()
		{
			ArrayList guidList = new ArrayList();
			foreach (OrgDebtorGroupCodeListElement elem in this)
			{
				if (elem.GroupGuid.IsValid && !elem.GroupGuid.IsEmpty)
				{
					guidList.Add(elem.GroupGuid.ToGuid());
				}
			}

			return (Guid[])guidList.ToArray(typeof(Guid));
		}

		public override string ToString()
		{
			string result = "";
			foreach (OrgDebtorGroupCodeListElement elem in this)
			{
				if (elem.GroupGuid.IsValid && !elem.GroupGuid.IsEmpty)
				{
					if (!string.IsNullOrEmpty(result))
					{
						result += ",";
					}
					result += elem.GroupGuid.ToString();
				}
			}

			return result;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new OrgDebtorGroupCodeListElement(ZGuid.Empty, this);
		}

		#region OrgDebtorGroupCollection

		public OrgDebtorGroupCollection OrgDebtorGroupCollection
		{
			get { return orgDebtorGroupCollection ?? (orgDebtorGroupCollection = new OrgDebtorGroupCollection(Factory, GetZQuery())); }
		}

		ZQuery GetZQuery()
		{
			return new ZQuery(OrgDebtorGroupSchema.OJ_IsValid, ZBool.True);
		}

		protected OrgDebtorGroupCollection orgDebtorGroupCollection;

		#endregion

	}

	#endregion

	#region OrgDebtorGroupCodeListCollectionWrapper

	public class OrgDebtorGroupCodeListCollectionWrapper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public OrgDebtorGroupCodeListCollectionWrapper(Guid[] value)
		{
			orgDebtorGroupCodeList = new OrgDebtorGroupCodeListCollection(value);
		}

		#region OrgDebtorGroupCodeList

		public OrgDebtorGroupCodeListCollection OrgDebtorGroupCodeList
		{
			get { return orgDebtorGroupCodeList; }
		}

		readonly OrgDebtorGroupCodeListCollection orgDebtorGroupCodeList;

		#endregion
	}

	#endregion
}
