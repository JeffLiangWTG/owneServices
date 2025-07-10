using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class DeliveryMode : RegistryBusinessObject, ICanDelete
	{
		#region Schema

		abstract new class Schema : RegistryBusinessObject.Schema
		{
			public const string UserDefinedCode = "UserDefinedCode";
			public const string UserDefinedDescription = "UserDefinedDescription";
			public const string IsSystemDefined = "IsSystemDefined";
		}

		#endregion

		public DeliveryMode() : base()
		{
		}

		public DeliveryMode(BusinessObjectFactory factory) : base(factory)
		{
		}

		public DeliveryMode(FallbackLevel fallbackLevel) : base(fallbackLevel)
		{
		}

		public DeliveryMode(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DeliveryMode();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			((DeliveryMode)clone).IsSystemDefined = IsSystemDefined;
		}

		#region Code
		public override ZString Code
		{
			get { return base.Code; }
			set
			{
				if (base.Code != value)
				{
					base.Code = value;
					if (!IsSystemDefined)
					{
						UserDefinedCode = base.Code;
					}
				}
			}
		}

		protected bool Code_ReadOnly
		{
			get { return IsSystemDefined; }
		}
		#endregion

		#region Description

		public override MultilingualString Description
		{
			get
			{
				return base.Description;
			}
			set
			{
				if (base.Description != value)
				{
					base.Description = value;
					if (!IsSystemDefined)
					{
						UserDefinedDescription = base.Description;
					}
				}
			}
		}

		protected bool Description_ReadOnly
		{
			get { return IsSystemDefined; }
		}
		#endregion

		#region System Defined

		public ZBool IsSystemDefined
		{
			get { return isSystemDefined; }
			set
			{
				SetNonPersistentPropertyValue(IsSystemDefinedInfo, ref isSystemDefined, value);
			}
		}

		ZBool isSystemDefined;

		public virtual ZPropertyInfo IsSystemDefinedInfo
		{
			get { return GetZPropertyInfo(Schema.IsSystemDefined); }
		}
		#endregion

		#region UserDefinedCode
		public virtual ZString UserDefinedCode
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return userDefinedCode; }
			set
			{
				CheckMaximumLength(UserDefinedCodeInfo, value);
				SetNonPersistentPropertyValue(UserDefinedCodeInfo, ref userDefinedCode, value);

				if (!IsValidationSuspended)
				{
					ValidateUserDefinedCode();
				}
			}
		}

		ZString userDefinedCode;

		public virtual ZPropertyInfo UserDefinedCodeInfo
		{
			get { return GetZPropertyInfo(Schema.UserDefinedCode); }
		}

		protected bool UserDefinedCode_ReadOnly
		{
			get { return !IsSystemDefined; }
		}

		public void ValidateUserDefinedCode()
		{
			UserDefinedCodeInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(UserDefinedCodeInfo);

			if (ParentCollections.Count > 0)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(UserDefinedCodeInfo);
			}
		}

		#endregion

		#region UserDefinedDescription
		public virtual MultilingualString UserDefinedDescription
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return userDefinedDescription; }
			set
			{
				if (value == null)
				{
					value = (NoResString)"";
				}

				CheckMaximumLength(UserDefinedDescriptionInfo, value.GetUnresolvedString());
				SetNonPersistentPropertyValue(UserDefinedDescriptionInfo, ref userDefinedDescription, value);
			}
		}

		MultilingualString userDefinedDescription;

		public virtual ZPropertyInfo UserDefinedDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.UserDefinedDescription); }
		}

		public int UserDefinedDescription_MaxLength
		{
			get { return MaxDescriptionLength; }
		}

		protected bool UserDefinedDescription_ReadOnly
		{
			get { return !IsSystemDefined; }
		}
		#endregion
		#region ICanDelete Members

		public override bool CanDelete
		{
			get { return !IsSystemDefined; }
		}

		MultilingualString ICanDelete.ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("9960d19d-5a1a-421c-9ecf-366175787573", "This is a system defined value and cannot be deleted."); }
		}

		#endregion

		protected override int CodeMaxLengthDefaultValue => 7;

		protected override int MaxDescriptionLength => 256;

		protected override void ReadMoreElements(XmlReader reader)
		{
			UserDefinedCode = reader.ReadElementString(Schema.UserDefinedCode);
			UserDefinedDescription = (NoResString)reader.ReadElementString(Schema.UserDefinedDescription);
			IsSystemDefined = ((XmlReaderWrapper)reader).ReadElementStringAsZBool(Schema.IsSystemDefined);
		}

		protected override void WriteMoreElements(XmlWriter writer)
		{
			writer.WriteElementString(Schema.UserDefinedCode, UserDefinedCode);
			writer.WriteElementString(Schema.UserDefinedDescription, UserDefinedDescription?.GetUnresolvedString());
			writer.WriteElementString(Schema.IsSystemDefined, IsSystemDefined.ToString());
		}
	}
}
