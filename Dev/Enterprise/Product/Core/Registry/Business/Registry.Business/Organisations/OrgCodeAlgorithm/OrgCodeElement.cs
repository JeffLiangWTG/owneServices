using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Organizations.CodeGeneration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class OrgCodeElement : RegistryBusinessObjectTemplate, IOrgCodeElement
	{
		ZString description;
		ZByte length;
		ZByte order;

		public OrgCodeElement()
		{
		}

		public OrgCodeElement(ZString description)
			: this(description, ZByte.Zero)
		{
		}

		public OrgCodeElement(ZString description, ZByte length)
		{
			this.description = description;
			this.length = length;
		}

		public ZString Description
		{
			get { return description; }
		}

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.Description); }
		}

		public bool Empty
		{
			get { return Length.IsEmpty || Order.IsEmpty; }
		}

		public ZByte Length
		{
			get { return length; }
			set
			{
				SetNonPersistentPropertyValue(LengthInfo, ref length, value);
				RefreshCurrentOrgCodeLength();
				if (!IsValidationSuspended)
				{
					ValidateLength();
				}
			}
		}

		public ZPropertyInfo LengthInfo
		{
			get { return GetZPropertyInfo(Schema.Length); }
		}

		protected bool Length_ReadOnly
		{
			get
			{
				switch (Description)
				{
					case OrgCodeElementDescription.CountryCode:
					case OrgCodeElementDescription.UnlocoCode:
					case OrgCodeElementDescription.IataCode:
						return true;
					default:
						return false;
				}
			}
		}

		public ZByte Order
		{
			get { return order; }
			set
			{
				SetNonPersistentPropertyValue(OrderInfo, ref order, value);
				RefreshCurrentOrgCodeLength();
				if (!IsValidationSuspended)
				{
					ValidateOrder();
				}
			}
		}

		public ZPropertyInfo OrderInfo
		{
			get { return GetZPropertyInfo(Schema.Order); }
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			((OrgCodeElement)clone).description = Description;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new OrgCodeElement();
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			description = reader.ReadElementString(Schema.Description);
			length = ZByte.ParseSafe(reader.ReadElementString(Schema.Length), ZByte.Zero);
			order = ZByte.ParseSafe(reader.ReadElementString(Schema.Order), ZByte.Zero);
		}

		void RefreshCurrentOrgCodeLength()
		{
			if (ParentCollections.Count > 0)
			{
				OrgCodeAlgorithm parentAlgorithm = ((OrgCodeElementCollection)ParentCollections.First()).Parent;
				if (parentAlgorithm != null)
				{
					parentAlgorithm.RefreshCurrentOrgCodeLength();
				}
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateLength();
			ValidateOrder();
		}

		public void ValidateLength()
		{
			LengthInfo.ClearAllNotifications();
			CompareValidation.CheckLessThanOrEqualTo(LengthInfo, 9);
		}

		public void ValidateOrder()
		{
			OrderInfo.ClearAllNotifications();
			CompareValidation.CheckLessThanOrEqualTo(OrderInfo, 7);
			if (!Order.IsEmpty && !OrderInfo.HasErrors() && ParentCollections.Count > 0)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(OrderInfo);
				if (!OrderInfo.HasErrors())
				{
					bool isCodeSpecificUniqueNumber = (Description == OrgCodeElementDescription.CodeSpecificUniqueNumber);
					if (isCodeSpecificUniqueNumber)
					{
						OrgCodeElement otherElement = ((OrgCodeElementCollection)ParentCollections.First())[OrgCodeElementDescription.GloballyUniqueNumber];
						if (!otherElement.Order.IsEmpty)
						{
							OrderInfo.AddError(Res.GetString("2afa4ebf-73d1-4a30-8b0b-68c2bbdb11da", "The {0} cannot be included in the algorithm because the {1} is already included.", Description, otherElement.Description));
						}
					}
					if (!OrderInfo.HasErrors() && (isCodeSpecificUniqueNumber || (Description == OrgCodeElementDescription.GloballyUniqueNumber)))
					{
						foreach (OrgCodeElement otherElement in ParentCollections.First())
						{
							if ((otherElement != this) && (otherElement.Order > Order))
							{
								OrderInfo.AddError(Res.GetString("27905bd8-2013-4158-b3ca-f070b37616b5", "The {0} must be the last element in the algorithm.", Description));
							}
						}
					}
				}
			}
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Description, Description);
			writer.WriteElementString(Schema.Length, Length.ToString());
			writer.WriteElementString(Schema.Order, Order.ToString());
		}

		#region IOrgCodeElement Members

		string IOrgCodeElement.Description
		{
			get { return Description; }
		}

		int IOrgCodeElement.Length
		{
			get { return Length; }
		}

		int IOrgCodeElement.Order
		{
			get { return Order; }
		}

		#endregion

		#region Schema

		public static class Schema
		{
			public const string Code = "Code";
			public const string Description = "Description";
			public const string Length = "Length";
			public const string Order = "Order";
		}

		#endregion
	}
}
