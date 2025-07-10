using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class ActivitySubtypeAssignment : AutoActivitySubtypeAssignment
	{
		public ActivitySubtypeAssignment() { }

		public ActivitySubtypeAssignment(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory) { }

		public ActivitySubtypeAssignmentLookups Lookups
		{
			get { return lookups ?? (lookups = new ActivitySubtypeAssignmentLookups(this)); }
		}
		ActivitySubtypeAssignmentLookups lookups;

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ActivitySubtypeAssignment(fallbackLevel, factory);
		}

		#region Properties

		[List("Lookups.ActivitySubtypeList")]
		public override ZString ActivitySubtype
		{
			get { return base.ActivitySubtype; }
			set { base.ActivitySubtype = value; }
		}

		#endregion

		#region Validation

		public override void ValidateActivitySubtype()
		{
			base.ValidateActivitySubtype();
			MandatoryValidation.CheckEntered(ActivitySubtypeInfo);
			ListValidation.ErrorIfInvalidCode(ActivitySubtypeInfo);
			if (ParentCollections.Count > 0)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(ActivitySubtypeInfo);
			}
		}

		public override void ValidateDescription()
		{
			base.ValidateDescription();
			MandatoryValidation.CheckEntered(DescriptionInfo);
		}

		public override void ValidateCapitalization()
		{
			base.ValidateCapitalization();
			MandatoryValidation.CheckEntered(CapitalizationInfo);
		}

		#endregion
	}
}
