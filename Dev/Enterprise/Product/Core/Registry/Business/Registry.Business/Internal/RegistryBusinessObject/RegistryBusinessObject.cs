using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public abstract class RegistryBusinessObject : RegistryBusinessObjectTemplate, IXmlSerializable
	{
		#region Schema

		public abstract class Schema
		{
			public const string CodeMaxLength = "CodeMaxLength";
			public const string Code = "Code";
			public const string Description = "Description";
		}

		#endregion

		public RegistryBusinessObject()
		{
		}

		public RegistryBusinessObject(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public RegistryBusinessObject(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public RegistryBusinessObject(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#region Method Overrides

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateCode();
			ValidateDescription();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			((RegistryBusinessObject)clone).CodeMaxLength = CodeMaxLength;
			base.CopyValuesToClone(clone);
		}

		#endregion

		#region CodeMaxLength

		public ZInt CodeMaxLength
		{
			get
			{
				if (fCodeMaxLength == 0)
				{
					fCodeMaxLength = CodeMaxLengthDefaultValue;
				}
				return fCodeMaxLength;
			}
			set
			{
				fCodeMaxLength = value;
			}
		}
		int fCodeMaxLength;

		#endregion

		#region Error Messages

		protected string EnterValidSelectionErrorMessage
		{
			get { return Res.GetString("92510606-9e7b-4c91-99b3-13fdd8eb7748", "Please enter a valid selection from the drop down list."); }
		}

		#endregion

		#region Bound Properties

		#region Code

		public virtual ZString Code
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return fCode; }
			set
			{
				CheckMaximumLength(CodeInfo, value);
				SetNonPersistentPropertyValue(CodeInfo, ref fCode, value);

				if (!IsValidationSuspended)
				{
					ValidateCode();
				}
			}
		}

		public virtual ZPropertyInfo CodeInfo
		{
			get { return GetZPropertyInfo(Schema.Code); }
		}

		public int Code_MaxLength
		{
			get { return CodeMaxLength; }
		}

		public void ValidateCode()
		{
			CodeInfo.ClearAllNotifications();

			if (IsCodeMandatory)
			{
				MandatoryValidation.CheckEntered(CodeInfo, CodeDisplayName);
			}

			if (IsCodeUniqueInCollection && ParentCollections.Count > 0)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(CodeInfo);
			}

			ValidateCodeCore();
		}

		protected virtual string CodeDisplayName
		{
			get { return (NoResString)"Code"; }
		}

		protected virtual int CodeMaxLengthDefaultValue
		{
			get { return 3; }
		}

		protected virtual bool IsCodeUniqueInCollection
		{
			get { return true; }
		}

		protected virtual bool IsCodeMandatory
		{
			get { return true; }
		}

		protected virtual void ValidateCodeCore()
		{
		}

		ZString fCode;

		#endregion

		#region Description

		public virtual MultilingualString Description
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return description ?? (NoResString)""; }
			set
			{
				if (value == null)
				{
					value = (NoResString)"";
				}
				CheckMaximumLength(DescriptionInfo, value.GetUnresolvedString());
				SetNonPersistentPropertyValue(DescriptionInfo, ref description, value, false);
				EnglishDescriptionInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					ValidateDescription();
				}
			}
		}

		public virtual ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.Description); }
		}

		public int Description_MaxLength
		{
			get { return MaxDescriptionLength; }
		}

		public virtual ZString EnglishDescription
		{
			get { return Description.GetUnresolvedString(); }
			set
			{
				if (value != EnglishDescription)
				{
					Description = (NoResString)value;
				}
			}
		}

		public virtual ZPropertyInfo EnglishDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(EnglishDescription)); }
		}

		public int EnglishDescription_MaxLength
		{
			get { return MaxDescriptionLength; }
		}

		public void ValidateDescription()
		{
			DescriptionInfo.ClearAllNotifications();

			if (IsDescriptionMandatory)
			{
				MandatoryValidation.CheckEntered(DescriptionInfo, DescriptionDisplayName);
			}

			ValidateDescriptionCore();
		}

		protected virtual string DescriptionDisplayName
		{
			get { return (NoResString)"Description"; }
		}

		protected virtual int MaxDescriptionLength
		{
			get { return 35; }
		}

		protected virtual bool IsDescriptionMandatory
		{
			get { return false; }
		}

		protected virtual void ValidateDescriptionCore()
		{
		}

		MultilingualString description;

		#endregion

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.CodeMaxLength, CodeMaxLength.ToString());
			writer.WriteElementString(Schema.Code, Code);
			writer.WriteElementString(Schema.Description, Description.GetUnresolvedString());

			WriteMoreElements(writer);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			CodeMaxLength = reader.ReadElementStringAsZInt(Schema.CodeMaxLength);
			var readCode = reader.ReadElementString(Schema.Code);
			var readDescription = (ZString)reader.ReadElementString(Schema.Description);

			var isInvalidCode = (CodeMaxLength > 0) && (readCode.Length > CodeMaxLength);
			if (isInvalidCode)
			{
				Code = ZString.Empty;
				Description = (NoResString)ZString.Empty;
			}
			else
			{
				Code = readCode;
				Description = (NoResString)readDescription.Left(Description_MaxLength);
			}

			ReadMoreElements(reader);
		}

		protected virtual void WriteMoreElements(XmlWriter writer)
		{
		}

		protected virtual void ReadMoreElements(XmlReader reader)
		{
		}

		#endregion
	}
}
