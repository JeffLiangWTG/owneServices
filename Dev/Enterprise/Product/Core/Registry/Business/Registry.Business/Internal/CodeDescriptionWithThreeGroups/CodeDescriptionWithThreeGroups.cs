using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	#region ICodeDescriptionWithThreeGroups Interface

	public interface ICodeDescriptionWithThreeGroups : ICodeDescriptionWithGroup, IMultilingualDescription
	{
		ZString MainDescription { get; }
		ZString Group2 { get; }
		ZString Group3 { get; }
		ZString ExtraDescription { get; }
	}

	#endregion

	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	[CodeProperty("Code"), DescriptionProperty("Description")]
	public class CodeDescriptionWithThreeGroups : CodeDescriptionWithGroup, ICodeDescriptionWithThreeGroups, ICanDelete
	{
		#region Schema

		protected new abstract class Schema : CodeDescriptionWithGroup.Schema
		{
			public const string Group2 = "Group2";
			public const string Group3 = "Group3";
		}

		#endregion

		public CodeDescriptionWithThreeGroups()
		{
		}

		public CodeDescriptionWithThreeGroups(CodeDescriptionWithThreeGroups clone)
		{
			clone.CopyValuesToClone(this);
		}

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new CodeDescriptionWithThreeGroups();

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			var codeDescriptionWithThreeGroups = (CodeDescriptionWithThreeGroups)clone;
			codeDescriptionWithThreeGroups.SystemDefined = SystemDefined;
			codeDescriptionWithThreeGroups.CodeMaxLength = CodeMaxLength;
			codeDescriptionWithThreeGroups.CodeList = CodeList;
		}

		#endregion

		#region MainDescription

		public ZString MainDescription => !Group.IsEmpty ? GroupLookup.GetDescriptionFromCode(Group) : !Group2.IsEmpty ? Group2Lookup.GetDescriptionFromCode(Group2) : string.Empty;

		#endregion

		#region Group

		[List(nameof(GroupLookup))]
		public override ZString Group
		{
			get => group;
			set
			{
				SetNonPersistentPropertyValue(GroupInfo, ref group, value);
				if (!IsValidationSuspended)
				{
					ValidateGroup();
				}
			}
		}
		ZString group;

		#endregion

		#region Group2

		[List(nameof(Group2Lookup))]
		public virtual ZString Group2
		{
			get => group2;
			set
			{
				SetNonPersistentPropertyValue(Group2Info, ref group2, value);
				if (!IsValidationSuspended)
				{
					ValidateGroup2();
				}
			}
		}
		ZString group2;

		public virtual ZPropertyInfo Group2Info => GetZPropertyInfo(Schema.Group2);

		#endregion

		#region Group3

		[List(nameof(Group3Lookup))]
		public virtual ZString Group3
		{
			get => group3;
			set
			{
				SetNonPersistentPropertyValue(Group3Info, ref group3, value);
				if (!IsValidationSuspended)
				{
					ValidateGroup3();
				}
			}
		}
		ZString group3;

		public virtual ZPropertyInfo Group3Info => GetZPropertyInfo(Schema.Group3);

		#endregion

		#region ExtraDescription

		public ZString ExtraDescription { get => Group3Lookup.GetDescriptionFromCode(Group3); }

		#endregion

		#region Xml Serialisation

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);

			Group2 = new ZString(reader.ReadElementString(Schema.Group2));
			Group3 = new ZString(reader.ReadElementString(Schema.Group3));
		}

		protected override void WriteMoreElements(XmlWriter writer)
		{
			base.WriteMoreElements(writer);

			writer.WriteElementString(Schema.Group2, Group2);
			writer.WriteElementString(Schema.Group3, Group3);
		}

		#endregion

		#region ICanDelete Members

		bool ICanDelete.CanDelete => !SystemDefined;

		MultilingualString ICanDelete.ReasonForNotAbleToDelete => ResString.GetMultilingualString("bc2bd5a0-b96a-4456-8316-da8af68a91e0", "This is a system defined value and cannot be deleted.");

		#endregion

		#region PreSaveValidation

		protected override void ValidateGroupCore()
		{
			base.ValidateGroupCore();
			ValidateGroup2();
			ValidateGroup3();
		}

		void ValidateGroup2()
		{
			Group2Info.ClearAllNotifications();
			MandatoryValidation.CheckEntered(Group2Info);
			ListValidation.ErrorIfInvalidCode(Group2Info);
		}

		void ValidateGroup3()
		{
			Group3Info.ClearAllNotifications();
			MandatoryValidation.CheckEntered(Group3Info);
			ListValidation.ErrorIfInvalidCode(Group3Info);
		}

		#endregion

		#region GroupLookup

		public ReadOnlyCodeDescriptionPairList Group2Lookup => ParentCollection?.Group2Lookup ?? new ReadOnlyCodeDescriptionPairList();
		public ReadOnlyCodeDescriptionPairList Group3Lookup => ParentCollection?.Group3Lookup ?? new ReadOnlyCodeDescriptionPairList();

		CodeDescriptionWithThreeGroupsCollection ParentCollection =>
			GetParentCollection(this, typeof(CodeDescriptionWithThreeGroupsCollection)) as CodeDescriptionWithThreeGroupsCollection;

		#endregion
	}
}
