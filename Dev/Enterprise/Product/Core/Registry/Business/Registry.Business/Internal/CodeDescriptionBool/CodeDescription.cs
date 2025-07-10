using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public interface ICodeDescription<T> : ICodeDescription
		where T : IZType
	{
		T Value { get; }
	}

	[CodeProperty("Code"), DescriptionProperty("Description")]
	public abstract class CodeDescription<T> : RegistryBusinessObject, ICodeDescription<T>, ICanDelete
		where T : IZType
	{
		#region Schema

		protected new abstract class Schema : RegistryBusinessObject.Schema
		{
			public const string Value = "Value";
			public const string SystemDefined = "SystemDefined";
		}

		#endregion

		protected CodeDescription()
		{
		}

		protected CodeDescription(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		protected CodeDescription(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#region Clone

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			if (CallBaseCopy)
			{
				base.CopyValuesToClone(clone);
			}

			var codeDescriptionBool = (CodeDescription<T>)clone;
			codeDescriptionBool.SystemDefined = SystemDefined;
			codeDescriptionBool.CodeMaxLength = CodeMaxLength;
			codeDescriptionBool.Code = Code;
			codeDescriptionBool.Description = Description;
			codeDescriptionBool.CodeList = CodeList;
		}

		protected virtual bool CallBaseCopy
		{
			get { return true; }
		}

		#endregion

		#region System Defined

		public bool SystemDefined { get; set; }

		#endregion

		#region Value

		public virtual T Value
		{
			get { return fValue; }
			set
			{
				SetNonPersistentPropertyValue(ValueInfo, ref fValue, value);
				if (!IsValidationSuspended)
				{
					ValidateValue();
				}
			}
		}

		public virtual ZPropertyInfo ValueInfo
		{
			get { return GetZPropertyInfo(Schema.Value); }
		}

		public void ValidateValue()
		{
			ValueInfo.ClearAllNotifications();
			ValidateValueCore();
		}

		protected virtual void ValidateValueCore()
		{
		}

		T fValue;

		#endregion

		#region Code / Description

		protected virtual bool CodeAndDescriptionReadOnly
		{
			get { return SystemDefined; }
		}

		[ReadOnlyMember(nameof(CodeAndDescriptionReadOnly))]
		public override ZString Code
		{
			get { return base.Code; }
			set { base.Code = value; }
		}

		[ReadOnlyMember(nameof(CodeAndDescriptionReadOnly))]
		public override MultilingualString Description
		{
			get { return base.Description; }
			set { base.Description = value; }
		}

		[ReadOnlyMember(nameof(CodeAndDescriptionReadOnly))]
		public override ZString EnglishDescription
		{
			get { return base.EnglishDescription; }
			set { base.EnglishDescription = value; }
		}

		protected override int MaxDescriptionLength
		{
			get { return 256; }
		}

#if DEBUG
		public int MaxDescriptionLengthInternal => MaxDescriptionLength;
#endif
		public ZString CodeColumnType
		{
			get { return CodeList == null ? nameof(FieldType.Text) : nameof(FieldType.TextDropEdit); }
		}

		public CodeDescriptionPairList CodeList { get; set; }

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateValue();
		}

		#region Xml Serialisation

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);

			if (ReadOrWriteValueInXml)
			{
				Value = ValueFromString(reader.ReadElementString(Schema.Value));
			}

			//Subclasses of CodeDescription have their own handling for Extra Bool/System Defined that we don't want to trample over.
			if (UseDefaultSystemDefinedHandlingForXml && reader.Name == Schema.SystemDefined)
			{
				SystemDefined = bool.Parse(reader.ReadElementString(Schema.SystemDefined));
			}
		}

		protected override void WriteMoreElements(XmlWriter writer)
		{
			base.WriteMoreElements(writer);

			if (ReadOrWriteValueInXml)
			{
				writer.WriteElementString(Schema.Value, Value.ToString());
			}

			//Subclasses of CodeDescription have their own handling for Extra Bool/System Defined that we don't want to trample over.
			if (UseDefaultSystemDefinedHandlingForXml)
			{
				writer.WriteElementString(Schema.SystemDefined, SystemDefined.ToString());
			}
		}

		protected virtual bool UseDefaultSystemDefinedHandlingForXml
		{
			get { return true; }
		}

		protected virtual bool ReadOrWriteValueInXml
		{
			get { return true; }
		}

		protected abstract T ValueFromString(string value);

		#endregion

		#region ICanDelete Members

		bool ICanDelete.CanDelete
		{
			get { return !SystemDefined; }
		}

		MultilingualString ICanDelete.ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("bc2bd5a0-b96a-4456-8316-da8af68a91e0", "This is a system defined value and cannot be deleted."); }
		}

		#endregion

		#region ICodeDescription

		object ICodeDescription.PK
		{
			get { return PK; }
		}

		string ICodeDescription.Code
		{
			get { return Code; }
		}

		string ICodeDescription.Description
		{
			get { return Description; }
		}

		#endregion
	}
}
