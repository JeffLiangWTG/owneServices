using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Warehouse
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class TransitReferenceMapping : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string SourceCategory = "SourceCategory";
			public const string SourceType = "SourceType";
			public const string TargetCategory = "TargetCategory";
			public const string TargetType = "TargetType";
			public const string Direction = "Direction";
		}

		#endregion

		public TransitReferenceMapping()
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new TransitReferenceMapping();
		}

		public TransitReferenceMappingCollection ParentCollection;

		#region Source Catetory

		[MaxLength(3)]
		[List("Lookups.SourceReferenceCategoryList")]
		public ZString SourceCategory
		{
			get { return sourceCategory; }
			set
			{
				CheckMaximumLength(SourceCategoryInfo, value);
				SetNonPersistentPropertyValue<ZString>(SourceCategoryInfo, ref sourceCategory, value);
				if (!IsValidationSuspended)
				{
					ValidateSourceCategory();
				}
			}
		}

		public ZPropertyInfo SourceCategoryInfo
		{
			get { return GetZPropertyInfo(Schema.SourceCategory); }
		}

		public void ValidateSourceCategory()
		{
			SourceCategoryInfo.ClearAllNotifications();

			Validation.ValidateSourceCategory();
		}

		ZString sourceCategory;

		#endregion

		#region Source Type

		[MaxLength(3)]
		[List("Lookups.SourceReferenceTypeList")]
		public ZString SourceType
		{
			get { return sourceType; }
			set
			{
				CheckMaximumLength(SourceTypeInfo, value);
				SetNonPersistentPropertyValue<ZString>(SourceTypeInfo, ref sourceType, value);
				if (!IsValidationSuspended)
				{
					ValidateSourceType();
					ValidateDirection();
				}
			}
		}

		public ZPropertyInfo SourceTypeInfo
		{
			get { return GetZPropertyInfo(Schema.SourceType); }
		}

		public void ValidateSourceType()
		{
			SourceTypeInfo.ClearAllNotifications();

			Validation.ValidateSourceType();
		}

		ZString sourceType;

		#endregion

		#region Target Catetory

		[MaxLength(3)]
		[List("Lookups.TargetReferenceCategoryList")]
		public ZString TargetCategory
		{
			get { return targetCategory; }
			set
			{
				CheckMaximumLength(TargetCategoryInfo, value);
				SetNonPersistentPropertyValue<ZString>(TargetCategoryInfo, ref targetCategory, value);
				if (!IsValidationSuspended)
				{
					ValidateTargetCategory();
				}
			}
		}

		public ZPropertyInfo TargetCategoryInfo
		{
			get { return GetZPropertyInfo(Schema.TargetCategory); }
		}

		public void ValidateTargetCategory()
		{
			TargetCategoryInfo.ClearAllNotifications();

			Validation.ValidateTargetCategory();
		}

		ZString targetCategory;

		#endregion

		#region Target Type

		[MaxLength(3)]
		[List("Lookups.TargetReferenceTypeList")]
		public ZString TargetType
		{
			get { return targetType; }
			set
			{
				CheckMaximumLength(TargetTypeInfo, value);
				SetNonPersistentPropertyValue<ZString>(TargetTypeInfo, ref targetType, value);
				if (!IsValidationSuspended)
				{
					ValidateTargetType();
				}
			}
		}

		public ZPropertyInfo TargetTypeInfo
		{
			get { return GetZPropertyInfo(Schema.TargetType); }
		}

		public void ValidateTargetType()
		{
			TargetTypeInfo.ClearAllNotifications();

			Validation.ValidateTargetType();
		}

		ZString targetType;

		#endregion

		#region Target Direction

		[MaxLength(3)]
		[List("Lookups.DirectionList")]
		public ZString Direction
		{
			get { return direction; }
			set
			{
				CheckMaximumLength(DirectionInfo, value);
				SetNonPersistentPropertyValue<ZString>(DirectionInfo, ref direction, value);
				if (!IsValidationSuspended)
				{
					ValidateDirection();
					ValidateSourceType();
				}
			}
		}

		public ZPropertyInfo DirectionInfo
		{
			get { return GetZPropertyInfo(Schema.Direction); }
		}

		public void ValidateDirection()
		{
			DirectionInfo.ClearAllNotifications();

			Validation.ValidateDirection();
		}

		ZString direction;

		#endregion

		#region Lookups

		public TransitReferenceMappingLookups Lookups
		{
			get
			{
				if (fLookups == null)
				{
					fLookups = GetNewLookups();
				}
				return fLookups;
			}
		}

		protected TransitReferenceMappingLookups GetNewLookups()
		{
			return new TransitReferenceMappingLookups(this);
		}

		TransitReferenceMappingLookups fLookups;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateSourceCategory();
			ValidateSourceType();
			ValidateTargetCategory();
			ValidateTargetType();
			ValidateDirection();
		}

		public TransitReferenceMappingValidation Validation
		{
			get { return new TransitReferenceMappingValidation(this); }
		}

		#endregion

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.SourceCategory, SourceCategory);
			writer.WriteElementString(Schema.SourceType, sourceType);
			writer.WriteElementString(Schema.TargetCategory, TargetCategory);
			writer.WriteElementString(Schema.TargetType, TargetType);
			writer.WriteElementString(Schema.Direction, Direction);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			SourceCategory = reader.ReadElementString(Schema.SourceCategory);
			SourceType = reader.ReadElementString(Schema.SourceType);
			TargetCategory = reader.ReadElementString(Schema.TargetCategory);
			TargetType = reader.ReadElementString(Schema.TargetType);
			Direction = reader.ReadElementString(Schema.Direction);
		}
	}
}
