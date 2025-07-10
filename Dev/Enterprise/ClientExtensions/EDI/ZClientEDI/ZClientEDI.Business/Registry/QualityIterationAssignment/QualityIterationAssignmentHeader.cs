using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1035:ICollectionImplementationsHaveStronglyTypedMembers")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1039:ListsAreStronglyTyped")]
	public class QualityIterationAssignmentHeader : RegistryBusinessObjectTemplateWithChildCollection
	{
		public abstract class Schema
		{
			public const string IsDefaultOptionSelected = "IsDefaultOptionSelected";
			public const string AssignmentCollection = "AssignmentCollection";
		}

		public ZBool IsDefaultOptionSelected
		{
			get { return isDefaultOptionSelected; }
			set
			{
				SetNonPersistentPropertyValue(IsDefaultOptionSelectedInfo, ref isDefaultOptionSelected, value);
				isDefaultOptionSelected = value;
				if (!IsValidationSuspended)
				{
					ValidateIsDefaultOptionSelected();
				}
			}
		}
		ZBool isDefaultOptionSelected;

		public virtual void ValidateIsDefaultOptionSelected()
		{
			IsDefaultOptionSelectedInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo IsDefaultOptionSelectedInfo
		{
			get { return GetZPropertyInfo(Schema.IsDefaultOptionSelected); }
		}

		public QualityIterationAssignmentCollection AssignmentCollection
		{
			get
			{
				if (assignmentCollection == null)
				{
					assignmentCollection = new QualityIterationAssignmentCollection(CurrentFallbackLevel, Factory);
					RegisterEditableChildObject(assignmentCollection);
				}
				return assignmentCollection;
			}
		}
		QualityIterationAssignmentCollection assignmentCollection;

		public QualityIterationAssignmentHeader()
		{
		}

		protected QualityIterationAssignmentHeader(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected QualityIterationAssignmentHeader(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public QualityIterationAssignmentHeader(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public ZBool IsReleaseGroupSpecified(ZString releaseGroup)
		{
			return AssignmentCollection.IsReleaseGroupSpecified(releaseGroup);
		}

		public ZBool IsQiEnabledForReleaseGroup(ZString releaseGroup)
		{
			return AssignmentCollection.IsQiEnabledForReleaseGroup(releaseGroup);
		}

		public ZBool IsQiEnabledForReleaseGroupOrNotSpecifiedButDefaultEnabled(ZString releaseGroup)
		{
			return IsReleaseGroupSpecified(releaseGroup) ? IsQiEnabledForReleaseGroup(releaseGroup) : IsDefaultOptionSelected;
		}

		public QualityIterationAssignment AddNewAssignment(ZString releaseGroup, ZBool isQiEnabled)
		{
			return AssignmentCollection.AddNew(releaseGroup, isQiEnabled);
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new QualityIterationAssignmentHeader(fallbackLevel, factory)
			{
				assignmentCollection = AssignmentCollection,
				IsDefaultOptionSelected = IsDefaultOptionSelected
			};
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ZBool isDefaultOptionSelectedParsed;
			var isDefaultOptionSelectedValue = reader.ReadElementString(Schema.IsDefaultOptionSelected);

			if (!ZBool.TryParse(isDefaultOptionSelectedValue, out isDefaultOptionSelectedParsed))
			{
				throw new FormatException("The IsDefaultOptionSelected XML element does not have a properly formatted ZBool value. Value: " + isDefaultOptionSelectedValue);
			}

			IsDefaultOptionSelected = isDefaultOptionSelectedParsed;
			assignmentCollection = (QualityIterationAssignmentCollection)QualityIterationAssignmentCollectionSerialiser.Deserialize(reader);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteStartElement(Schema.IsDefaultOptionSelected);
			writer.WriteValue(IsDefaultOptionSelected);
			writer.WriteEndElement();
			QualityIterationAssignmentCollectionSerialiser.Serialize(writer, AssignmentCollection);
		}

		ZXmlSerializer QualityIterationAssignmentCollectionSerialiser
		{
			get
			{
				return qualityIterationAssignmentCollectionSerialiser ?? (qualityIterationAssignmentCollectionSerialiser = ZXmlSerializer.New(typeof(QualityIterationAssignmentCollection)));
			}
		}
		ZXmlSerializer qualityIterationAssignmentCollectionSerialiser;

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			var target = ((QualityIterationAssignmentHeader)(clone));
			base.CopyValuesToClone(target);
			target.IsDefaultOptionSelected = IsDefaultOptionSelected;
			target.assignmentCollection = AssignmentCollection;
		}

		protected override IEnumerable<RegistryBusinessObjectCollectionTemplate> ChildCollections
		{
			get { return new[] { AssignmentCollection }; }
		}
	}
}

