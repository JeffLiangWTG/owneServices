using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class OrgBarcodeMask : RegistryBusinessObjectTemplate
	{
		public OrgBarcodeMask()
			: base()
		{
			parent = new OrgBarcodeMaskCollection();
		}

		public OrgBarcodeMask(BusinessObjectFactory factory, BusinessObjectCollection parent)
			: base(factory)
		{
			this.parent = parent;
		}

		public OrgBarcodeMask(FallbackLevel fallback, BusinessObjectFactory factory, BusinessObjectCollection parent)
			: base(fallback, factory)
		{
			this.parent = parent;
		}

		#region Schema

		abstract class Schema
		{
			public const string Organisation = "Org";
			public const string Priority = "Priority";
			public const string Mask = "Mask";
		}

		#endregion

		#region Organisation

		[List("OrganisationList")]
		public ZGuid Org
		{
			get { return org; }
			set
			{
				SetNonPersistentPropertyValue<ZGuid>(OrgInfo, ref org, value);
				orgHeader = null;
				if (!IsValidationSuspended)
				{
					ValidateOrganisation();
				}
			}
		}

		public ZPropertyInfo OrgInfo
		{
			get { return GetZPropertyInfo(Schema.Organisation); }
		}

		public void ValidateOrganisation()
		{
			OrgInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(OrgInfo);
			TypeValidation.CheckValidGuid(OrgInfo);
			ListValidation.ErrorIfInvalidPK(OrgInfo, OrganisationList);
			if (OrgHeader != null)
			{
				DepotAddressColorSoundValidation.ValidateOrgHasDepot(OrgInfo, OrgHeader);
			}
		}

		ZGuid org;

		#endregion

		#region Priority

		public ZInt Priority
		{
			get { return priority; }
			set
			{
				SetNonPersistentPropertyValue<ZInt>(PriorityInfo, ref priority, value);
				if (!IsValidationSuspended)
				{
					ValidatePriority();
				}
			}
		}

		public ZPropertyInfo PriorityInfo
		{
			get { return GetZPropertyInfo(Schema.Priority); }
		}

		public void ValidatePriority()
		{
			PriorityInfo.ClearAllNotifications();
			CompareValidation.CheckWithinRange(PriorityInfo, 1, 99);
			List<OrgBarcodeMask> sameOrgList = new List<OrgBarcodeMask>();

			sameOrgList.Add(this);
			foreach (OrgBarcodeMask child in Parent)
			{
				if (child.Org.Equals(Org))
				{
					sameOrgList.Add(child);
				}
			}

			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(PriorityInfo, sameOrgList, Res.GetString("5596c96b-4f43-4d56-a8bf-e1e3d6da72fc", "Priorities must be unique per organization"));
		}

		ZInt priority;

		#endregion

		#region Mask

		[MaxLength(100)]
		public ZString Mask
		{
			get { return mask; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(MaskInfo, ref mask, value);
				if (!IsValidationSuspended)
				{
					ValidateMask();
				}
			}
		}

		public ZPropertyInfo MaskInfo
		{
			get { return GetZPropertyInfo(Schema.Mask); }
		}

		public void ValidateMask()
		{
			MaskInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(MaskInfo);
			ValidateRegex(MaskInfo);
		}

		void ValidateRegex(ZPropertyInfo info)
		{
			ZString value = (ZString)info.Value;
			try
			{
				var regex = new Regex(value);
				if (regex.GetGroupNumbers().Length > 2)
				{
					info.AddError(Res.GetString("50ace722-8e8c-4dcb-bd64-c5611a427551", "Regular expression can only contain one group"));
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				info.AddError(Res.GetString("6d21b34e-5a83-4e37-bf32-586ccfa95e79", "Mask is not a valid regular expression"));
			}
		}

		ZString mask;

		#endregion

		#region Lookups

		public BusinessObjectCollection OrganisationList
		{
			get
			{
				if (orgList == null)
				{
					orgList = (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.MasterFiles.Integration.IOrgHeaderCollection>(), new object[] { CurrentFactory });
					orgList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Secondary Type", "Property", (ZString)OrgConstants.FilterControl.SecondaryOrgType.Depot, false));
				}
				return orgList;
			}
		}

		BusinessObjectCollection orgList;

		IOrgHeader OrgHeader
		{
			get
			{
				return orgHeader ?? (orgHeader = (Org != ZGuid.Empty) ? CurrentFactory.LoadTop1<IOrgHeader>(new ZQuery(OrgHeaderSchema.PK, Org)) : null);
			}
		}

		IOrgHeader orgHeader;

		BusinessObjectCollection Parent
		{
			get { return parent; }
		}

		readonly BusinessObjectCollection parent;
		#endregion

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new OrgBarcodeMask(factory, new OrgBarcodeMaskCollection());
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateOrganisation();
			ValidatePriority();
			ValidateMask();
		}

		#endregion

		#region XML Serialization

		protected override void WriteElements(XmlWriter writer)
		{
			writer.WriteElementString(Schema.Organisation, Org.ToString());
			writer.WriteElementString(Schema.Priority, Priority.ToString());
			writer.WriteElementString(Schema.Mask, Mask);
			base.WriteElements(writer);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			XmlReader readerObject = reader.Reader;
			while (readerObject.NodeType != XmlNodeType.EndElement)
			{
				switch (readerObject.LocalName)
				{
					case Schema.Organisation:
						ZGuid orgGuid;
						if (ZGuid.TryParse(readerObject.ReadElementString(), out orgGuid))
						{
							Org = orgGuid;
						}
						break;
					case Schema.Priority:
						ZInt priorityInt;
						if (ZInt.TryParse(readerObject.ReadElementString(), out priorityInt))
						{
							Priority = priorityInt;
						}
						break;
					case Schema.Mask:
						ZString maskString = readerObject.ReadElementString();
						if (!maskString.IsEmpty)
						{
							Mask = maskString;
						}
						break;
				}
			}
		}

		#endregion
	}
}
