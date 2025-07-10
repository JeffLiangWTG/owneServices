using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Registry.GUI
{
	#region OrgHeaderCodeListElement

	public class OrgHeaderCodeListElement : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public abstract class Schema
		{
			public const string ClientGuid = "ClientGuid";
			public const string ClientName = "ClientName";
		}

		#endregion

		public OrgHeaderCodeListElement(ZGuid clientGuid, OrgHeaderCodeListCollection parent) : base(new BusinessObjectFactory())
		{
			fClientGuid = clientGuid;
			this.Parent = parent;
		}

		#region Collections

		public OrgHeaderCollection OrgHeaderCollection
		{
			get { return Parent.OrgHeaderCollection; }
		}

		protected readonly OrgHeaderCodeListCollection Parent;

		#endregion

		#region Properties

		#region Client Code

		public ZGuid ClientGuid
		{
			get { return fClientGuid; }
			set
			{
				fClientGuid = value;
				if (!IsValidationSuspended)
				{
					ClientGuidInfo.ClearAllNotifications();
					TypeValidation.CheckValidGuid(ClientGuidInfo);
				}
				ClientGuidInfo.RefreshBinding();
			}
		}

		protected ZGuid fClientGuid;

		public ZPropertyInfo ClientGuidInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.ClientGuid); }
		}

		#endregion

		#region Org Client

		public OrgHeader OrgClient
		{
			get { return (OrgHeader)Factory.Load(typeof(OrgHeader), ClientGuid); }
		}

		#endregion

		#region Client Name

		public ZString ClientName
		{
			get
			{
				OrgHeader org = OrgClient;
				return (org == null) ? ZString.Empty : org.OH_FullNameTruncated;
			}
		}

		public ZPropertyInfo ClientNameInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.ClientName); }
		}

		#endregion

		#endregion
	}

	#endregion

	#region OrgHeaderCodeListCollection

	public class OrgHeaderCodeListCollection : NonPersistentBusinessObjectCollection<OrgHeaderCodeListElement>	{
		public OrgHeaderCodeListCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public OrgHeaderCodeListCollection(Guid[] value) : this(new BusinessObjectFactory())
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
						Add(new OrgHeaderCodeListElement(newGuid, this));
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
			foreach (OrgHeaderCodeListElement elem in this)
			{
				if (elem.ClientGuid.IsValid && !elem.ClientGuid.IsEmpty)
				{
					guidList.Add(elem.ClientGuid.ToGuid());
				}
			}

			return (Guid[])guidList.ToArray(typeof(Guid));
		}

		public override string ToString()
		{
			string result = "";
			foreach (OrgHeaderCodeListElement elem in this)
			{
				if (elem.ClientGuid.IsValid && !elem.ClientGuid.IsEmpty)
				{
					if (!string.IsNullOrEmpty(result))
					{
						result += ",";
					}
					result += elem.ClientGuid.ToString();
				}
			}

			return result;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new OrgHeaderCodeListElement(ZGuid.Empty, this);
		}

		#region OrgHeaderCollection

		public OrgHeaderCollection OrgHeaderCollection
		{
			get
			{
				if (fOrgHeaderCollection == null)
				{
					fOrgHeaderCollection = new OrgHeaderCollection(Factory);
				}
				return fOrgHeaderCollection;
			}
		}

		protected OrgHeaderCollection fOrgHeaderCollection;

		#endregion
	}

	#endregion

	#region OrgHeaderCodeListCollectionWrapper

	public class OrgHeaderCodeListCollectionWrapper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public OrgHeaderCodeListCollectionWrapper(Guid[] value)
		{
			fOrgHeaderCodeList = new OrgHeaderCodeListCollection(value);
		}

		#region OrgHeaderCodeList

		public OrgHeaderCodeListCollection OrgHeaderCodeList
		{
			get { return fOrgHeaderCodeList; }
		}

		readonly OrgHeaderCodeListCollection fOrgHeaderCodeList;

		#endregion
	}

	#endregion
}
