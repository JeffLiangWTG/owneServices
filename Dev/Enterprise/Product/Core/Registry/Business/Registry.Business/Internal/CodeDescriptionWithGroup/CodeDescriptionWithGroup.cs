using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	#region ICodeDescriptionBool Interface

	public interface ICodeDescriptionWithGroup : ICodeDescription, IMultilingualDescription
	{
		ZString Group { get; }
	}

	#endregion

	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	[CodeProperty("Code"), DescriptionProperty("Description")]
	public class CodeDescriptionWithGroup : RegistryBusinessObject, ICodeDescriptionWithGroup, ICanDelete
	{
		#region Schema

		protected new abstract class Schema : RegistryBusinessObject.Schema
		{
			public const string Group = "Group";
			public const string SystemDefined = "SystemDefined";
		}

		#endregion

		public CodeDescriptionWithGroup()
		{
		}

		public CodeDescriptionWithGroup(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public CodeDescriptionWithGroup(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new CodeDescriptionWithGroup();

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);

			var codeDescriptionWithGroup = (CodeDescriptionWithGroup)clone;
			codeDescriptionWithGroup.SystemDefined = SystemDefined;
			codeDescriptionWithGroup.CodeMaxLength = CodeMaxLength;
			codeDescriptionWithGroup.CodeList = CodeList;
		}

		#endregion

		#region System Defined

		public bool SystemDefined { get; set; }

		#endregion

		#region Group

		[List(nameof(GroupLookup))]
		public virtual ZString Group { get => group; set => SetNonPersistentPropertyValue(GroupInfo, ref group, value); }
		ZString group;

		public virtual ZPropertyInfo GroupInfo => GetZPropertyInfo(Schema.Group);

		#endregion

		#region Code / Description

		protected virtual bool CodeAndDescriptionReadOnly => SystemDefined;

		[ReadOnlyMember(nameof(CodeAndDescriptionReadOnly))]
		public override ZString Code { get => base.Code; set => base.Code = value; }

		[ReadOnlyMember(nameof(CodeAndDescriptionReadOnly))]
		public override MultilingualString Description { get => base.Description; set => base.Description = value; }

		[ReadOnlyMember(nameof(CodeAndDescriptionReadOnly))]
		public override ZString EnglishDescription { get => base.EnglishDescription; set => base.EnglishDescription = value; }

		[ReadOnlyMember(nameof(CodeAndDescriptionReadOnly))]
		MultilingualString IMultilingualDescription.MultilingualDescription => Description;

		protected override int MaxDescriptionLength => 256;

#if DEBUG
		public int MaxDescriptionLengthInternal => MaxDescriptionLength;
#endif

		public ZString CodeColumnType => CodeList == null ? nameof(FieldType.Text) : nameof(FieldType.TextDropEdit);

		public CodeDescriptionPairList CodeList { get; set; }

		#endregion

		#region Xml Serialisation

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);

			if (ReadOrWriteGroupInXml)
			{
				Group = new ZString(reader.ReadElementString(Schema.Group));
			}
			//Subclasses of CodeDescriptionBool have their own handling for Extra Bool/System Defined that we don't want to trample over.
			if (UseDefaultSystemDefinedHandlingForXml && reader.Name == Schema.SystemDefined)
			{
				SystemDefined = bool.Parse(reader.ReadElementString(Schema.SystemDefined));
			}
		}

		protected override void WriteMoreElements(XmlWriter writer)
		{
			base.WriteMoreElements(writer);

			if (ReadOrWriteGroupInXml)
			{
				writer.WriteElementString(Schema.Group, Group);
			}
			//Subclasses of CodeDescriptionBool have their own handling for Extra Bool/System Defined that we don't want to trample over.
			if (UseDefaultSystemDefinedHandlingForXml)
			{
				writer.WriteElementString(Schema.SystemDefined, SystemDefined.ToString());
			}
		}

		protected virtual bool UseDefaultSystemDefinedHandlingForXml => true;

		protected virtual bool ReadOrWriteGroupInXml => true;

		#endregion

		#region ICanDelete Members

		public override bool CanDelete => !SystemDefined;

		public override MultilingualString ReasonForNotAbleToDelete => ResString.GetMultilingualString("bc2bd5a0-b96a-4456-8316-da8af68a91e0", "This is a system defined value and cannot be deleted.");

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

		#region PreSaveValidation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateGroup();
		}

		public void ValidateGroup()
		{
			ValidateGroupCore();
		}

		protected virtual void ValidateGroupCore()
		{
			GroupInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(GroupInfo);
			ListValidation.ErrorIfInvalidCode(GroupInfo, GroupLookup);
		}

		#endregion

		#region GroupLookup

		public ReadOnlyCodeDescriptionPairList GroupLookup => ParentCollection?.GroupLookup ?? new ReadOnlyCodeDescriptionPairList();

		CodeDescriptionWithGroupCollection ParentCollection => GetParentCollection(this, typeof(CodeDescriptionWithGroupCollection)) as CodeDescriptionWithGroupCollection;

		#endregion
	}
}
