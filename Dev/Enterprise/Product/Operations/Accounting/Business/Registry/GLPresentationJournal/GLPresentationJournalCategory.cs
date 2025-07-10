using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using ResString = Enterprise.Accounting.Business.ResString;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	[XmlRoot("GLPresentationJournalCategory")]
	public class GLPresentationJournalCategory : CodeDescriptionBoolWithExtraBool, ICanDelete
	{
		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new GLPresentationJournalCategory();
		}

		#region ICanDelete

		public override bool CanDelete
		{
			get { return base.CanDelete && !(IsUsed || IsUsedByParentCode || Bool2); }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				if (Bool2)
				{
					return ResString.GetMultilingualString("c214f346-8020-477d-8a45-5da03b980518", "Cannot delete Elimination Category.");
				}
				if (IsUsed)
				{
					return ResString.GetMultilingualString("59d72254-9314-4c89-97a6-8bf40e73823d", "Cannot delete Category already used in aggregation. Make inactive instead.");
				}
				if (IsUsedByParentCode)
				{
					var collection = ParentCollection.ToArray<GLPresentationJournalCategory>().Where(x => x.ParentCode == Code);
					var codeMsg = new ZStringBuilder();
					collection.ForEach(x => codeMsg.Append(string.Format(CultureInfo.CurrentCulture, "'{0}',", x.Code)));
					return ResString.GetMultilingualString("8C451E40-1EC5-4205-A90D-5B0109A6DF2C", "Code '{0}' is the parent of {1} and cannot be deleted. ", Code, codeMsg.ToString().Trim(','));
				}
				return base.ReasonForNotAbleToDelete;
			}
		}

		#endregion

		bool IsUsed
		{
			get { return ParentCollection != null && ParentCollection.CategoriesInUse.Contains<string>(Code.ToUpper()); }
		}

		bool IsUsedByParentCode
		{
			get
			{
				return ParentCollection != null && !Code.IsEmpty && ParentCollection.ToArray<GLPresentationJournalCategory>().Any(x => x.Code != Code && x.ParentCode == Code);
			}
		}

		#region Properties

		#region Code

		[ReadOnlyMember(nameof(Code_ReadOnly))]
		public override ZString Code
		{
			get { return base.Code; }
			set { base.Code = value; }
		}

		protected bool Code_ReadOnly
		{
			get { return CodeAndDescriptionReadOnly || IsUsed; }
		}

		protected override void ValidateCodeCore()
		{
			base.ValidateCodeCore();
			Validation.ValidateCode();
		}

		#endregion

		#region Description

		protected override void ValidateDescriptionCore()
		{
			base.ValidateDescriptionCore();
			Validation.ValidateDescription();
		}

		#endregion

		#region Bool == IsActive

		[ReadOnlyMember(nameof(Bool_ReadOnly))]
		public override ZBool Bool
		{
			get { return base.Bool; }
			set
			{
				base.Bool = value;
				if (!IsValidationSuspended)
				{
					ValidateIsActive();
				}
			}
		}

		protected bool Bool_ReadOnly
		{
			get { return Bool2_ReadOnly && Bool; }
		}

		protected void ValidateIsActive()
		{
			BoolInfo.ClearAllNotifications();
			Validation.ValidateIsActive();
		}

		#endregion

		#region Bool2 == UseForElimination

		[ReadOnlyMember(nameof(Bool2_ReadOnly))]
		public override ZBool Bool2
		{
			get { return base.Bool2; }
			set
			{
				base.Bool2 = value;
				if (!IsValidationSuspended)
				{
					ValidateUseForElimination();
				}
			}
		}

		protected bool Bool2_ReadOnly
		{
			get { return IsUsed && Bool2 && !Bool2Info.HasErrors(); }
		}

		protected void ValidateUseForElimination()
		{
			Bool2Info.ClearAllNotifications();
			Validation.ValidateUseForElimination();
		}

		#endregion

		#region Bool3 == UseForClosing

		ZBool fBool3;

		public virtual ZBool Bool3
		{
			get { return fBool3; }
			set
			{
				if (fBool3 != value)
				{
					SetNonPersistentPropertyValue(Bool3Info, ref fBool3, value);
				}
				if (!IsValidationSuspended)
				{
					ValidateUseForClosing();
				}
			}
		}

		public ZPropertyInfo Bool3Info
		{
			get { return GetZPropertyInfo(nameof(Bool3)); }
		}

		protected void ValidateUseForClosing()
		{
			Bool3Info.ClearAllNotifications();
			Validation.ValidateUseForClosing();
		}

		#endregion

		#region Bool4 == UseForOpening

		ZBool fBool4;

		public virtual ZBool Bool4
		{
			get { return fBool4; }
			set
			{
				if (fBool4 != value)
				{
					SetNonPersistentPropertyValue(Bool4Info, ref fBool4, value);
				}
				if (!IsValidationSuspended)
				{
					ValidateUseForOpening();
				}
			}
		}

		public ZPropertyInfo Bool4Info
		{
			get { return GetZPropertyInfo(nameof(Bool4)); }
		}

		protected void ValidateUseForOpening()
		{
			Bool4Info.ClearAllNotifications();
			Validation.ValidateUseForOpening();
		}

		#endregion

		#region Parent Code

		ZString fParentCode;

		public virtual ZString ParentCode
		{
			get { return fParentCode; }
			set
			{
				if (fParentCode != value)
				{
					SetNonPersistentPropertyValue(ParentCodeInfo, ref fParentCode, value);
				}
				if (!IsValidationSuspended)
				{
					ValidateParentCode();
				}
			}
		}

		public ZPropertyInfo ParentCodeInfo
		{
			get { return GetZPropertyInfo(nameof(ParentCode)); }
		}

		void ValidateParentCode()
		{
			ParentCodeInfo.ClearAllNotifications();
			Validation.ValidateParentCode();
		}

		#endregion

		#endregion

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);
			if (reader.NodeType == XmlNodeType.Element && reader.Name == "Bool3")
			{
				Bool3 = new ZBool(reader.ReadElementString("Bool3"));
			}
			if (reader.NodeType == XmlNodeType.Element && reader.Name == "Bool4")
			{
				Bool4 = new ZBool(reader.ReadElementString("Bool4"));
			}
			if (reader.NodeType == XmlNodeType.Element && reader.Name == "ParentCode")
			{
				ParentCode = new ZString(reader.ReadElementString("ParentCode"));
			}
		}

		protected override void WriteMoreElements(XmlWriter writer)
		{
			base.WriteMoreElements(writer);
			writer.WriteElementString("Bool3", Bool3.ToString());
			writer.WriteElementString("Bool4", Bool4.ToString());
			writer.WriteElementString("ParentCode", ParentCode.ToString());
		}

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateIsActive();
			ValidateUseForElimination();
			ValidateUseForClosing();
			ValidateUseForOpening();
			ValidateParentCode();
		}

		public GLPresentationJournalCategoryValidation Validation
		{
			get { return new GLPresentationJournalCategoryValidation(this); }
		}

		#endregion

		#region ParentCollection

		internal GLPresentationJournalCategoryCollection ParentCollection
		{
			get { return (GLPresentationJournalCategoryCollection)GetParentCollection(this, typeof(GLPresentationJournalCategoryCollection)); }
		}

		#endregion
	}
}
