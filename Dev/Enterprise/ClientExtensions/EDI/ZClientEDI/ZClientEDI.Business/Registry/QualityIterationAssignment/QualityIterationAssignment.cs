using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1035:ICollectionImplementationsHaveStronglyTypedMembers")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1039:ListsAreStronglyTyped")]
	public class QualityIterationAssignment : RegistryBusinessObjectTemplate
	{
		public abstract class Schema
		{
			public const string ReleaseGroup = "ReleaseGroup";
			public const string IsQiEnabled = "IsQiEnabled";
			public const int ReleaseGroupMaxLength = 15;
		}

		[List("Lookups.ReleaseGroupList")]
		[MaxLength(Schema.ReleaseGroupMaxLength)]
		public ZString ReleaseGroup
		{
			get { return releaseGroup; }
			set
			{
				SetNonPersistentPropertyValue(ReleaseGroupInfo, ref releaseGroup, value);
				if (!IsValidationSuspended)
				{
					ValidateReleaseGroup();
				}
			}
		}
		ZString releaseGroup;

		public virtual void ValidateReleaseGroup()
		{
			ReleaseGroupInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ReleaseGroupInfo);
			ListValidation.ErrorIfInvalidCode(ReleaseGroupInfo);
			if (ParentCollections.Any())
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(ReleaseGroupInfo);
			}
		}

		public virtual ZPropertyInfo ReleaseGroupInfo
		{
			get { return GetZPropertyInfo(Schema.ReleaseGroup); }
		}

		public ZBool IsQiEnabled
		{
			get { return isQiEnabled; }
			set
			{
				SetNonPersistentPropertyValue(IsQiEnabledInfo, ref isQiEnabled, value);
				if (!IsValidationSuspended)
				{
					ValidateIsQiEnabled();
				}
			}
		}
		ZBool isQiEnabled;

		public virtual void ValidateIsQiEnabled()
		{
			IsQiEnabledInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo IsQiEnabledInfo
		{
			get { return GetZPropertyInfo(Schema.IsQiEnabled); }
		}

		public QualityIterationAssignment()
		{
		}

		protected QualityIterationAssignment(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected QualityIterationAssignment(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public QualityIterationAssignment(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new QualityIterationAssignment(fallbackLevel, factory)
			{
				releaseGroup = releaseGroup,
				isQiEnabled = isQiEnabled
			};
		}

		protected override void WriteElements(XmlWriter writer)
		{
			writer.WriteStartElement(Schema.ReleaseGroup);
			writer.WriteValue(ReleaseGroup);
			writer.WriteEndElement();
			writer.WriteStartElement(Schema.IsQiEnabled);
			writer.WriteValue(IsQiEnabled);
			writer.WriteEndElement();
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ReleaseGroup = reader.ReadElementString(Schema.ReleaseGroup);
			ZBool isQiEnabledParsed;
			var isQiEnabledStringValue = reader.ReadElementString(Schema.IsQiEnabled);

			if (!ZBool.TryParse(isQiEnabledStringValue, out isQiEnabledParsed))
			{
				throw new FormatException("The IsQiEnabled XML element does not have a properly formatted ZBool value. Value: " + isQiEnabledStringValue);
			}

			IsQiEnabled = isQiEnabledParsed;
		}

		public QualityIterationAssignmentLookups Lookups
		{
			get { return lookups ?? (lookups = new QualityIterationAssignmentLookups(this)); }
		}
		QualityIterationAssignmentLookups lookups;

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			var target = ((QualityIterationAssignment)(clone));
			base.CopyValuesToClone(target);
			target.ReleaseGroup = ReleaseGroup;
			target.IsQiEnabled = IsQiEnabled;
		}
	}
}

