using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.WorkflowManager;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	[CodeProperty("Code"), DescriptionProperty("Description")]
	public class CategorisedWorkflowIterationReasons : RegistryBusinessObject, ICategorisedRegistryBusinessObjectCollection
	{
		#region Schema

		protected new abstract class Schema : RegistryBusinessObject.Schema
		{
			public const string IterationReasonValidation = "IterationReasonValidation";
		}

		#endregion

		public void SetIterationReasons(WorkflowIterationReasonCollection iterationReasonsToSet)
		{
			UnRegisterEditableChildObject(IterationReasons);
			iterationReasons = iterationReasonsToSet;
			RegisterEditableChildObject(IterationReasons);
		}

		[ChildEditable]
		public WorkflowIterationReasonCollection IterationReasons
		{
			get
			{
				if (iterationReasons == null)
				{
					iterationReasons = new WorkflowIterationReasonCollection();
					RegisterEditableChildObject(iterationReasons);
				}
				return iterationReasons;
			}
		}

		WorkflowIterationReasonCollection iterationReasons;

		[List("IterationReasonValidationList")]
		public ZString IterationReasonValidation
		{
			get { return iterationReasonValidation; }
			set
			{
				CheckMaximumLength(IterationReasonValidationInfo, value);
				SetNonPersistentPropertyValue(IterationReasonValidationInfo, ref iterationReasonValidation, value);

				if (!IsValidationSuspended)
				{
					ValidateIterationReasonValidation();
				}
			}
		}
		ZString iterationReasonValidation;

		public virtual ZPropertyInfo IterationReasonValidationInfo
		{
			get { return GetZPropertyInfo(Schema.IterationReasonValidation); }
		}

		public int IterationReasonValidation_MaxLength
		{
			get { return CodeMaxLength; }
		}

		public void ValidateIterationReasonValidation()
		{
			IterationReasonValidationInfo.ClearAllNotifications();
			if (IterationReasonValidation.IsEmpty)
			{
				MandatoryValidation.CheckEntered(IterationReasonValidationInfo);
			}
			else if (!IterationReasonValidationList.ContainsCode(IterationReasonValidation))
			{
				IterationReasonValidationInfo.AddError(Res.GetString("7cd43fbd-dbef-4870-97be-bc5abbe5a467", "Please enter a valid value."));
			}
		}

		public IterationReasonValidationList IterationReasonValidationList
		{
			get { return new IterationReasonValidationList(); }
		}

		protected override int MaxDescriptionLength => 256;

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CategorisedWorkflowIterationReasons();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);

			((CategorisedWorkflowIterationReasons)clone).IterationReasonValidation = IterationReasonValidation;
		}

		protected override void CopyCollectionsToClone(RegistryBusinessObjectTemplate clone, FallbackLevel currentFallbackLevel,
			BusinessObjectFactory factory)
		{
			base.CopyCollectionsToClone(clone, currentFallbackLevel, factory);

			var iteration = (CategorisedWorkflowIterationReasons)clone;

			iteration.IterationReasons.RemoveAll();
			iteration.IterationReasons.AddRange((BusinessObjectCollection)IterationReasons.Clone(currentFallbackLevel, factory));
		}

		#endregion

		#region Xml Serialization

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);
			SetIterationReasons((WorkflowIterationReasonCollection)CollectionSerialiser.Deserialize(reader));
			IterationReasonValidation = reader.ReadElementString(Schema.IterationReasonValidation);
		}

		protected override void WriteMoreElements(XmlWriter writer)
		{
			base.WriteMoreElements(writer);
			CollectionSerialiser.Serialize(writer, IterationReasons);
			writer.WriteElementString(Schema.IterationReasonValidation, IterationReasonValidation);
		}

		ZXmlSerializer CollectionSerialiser
		{
			get { return collectionSerialiser ?? (collectionSerialiser = ZXmlSerializer.New(typeof(WorkflowIterationReasonCollection))); }
		}

		ZXmlSerializer collectionSerialiser;

		#endregion

		#region ICollectionOfCodeDescriptionBoolCollections

		RegistryBusinessObjectCollection ICategorisedRegistryBusinessObjectCollection.InnerCollection => IterationReasons;

		#endregion

	}
}
