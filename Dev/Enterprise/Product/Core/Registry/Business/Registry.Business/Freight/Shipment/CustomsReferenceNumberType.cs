using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Integration.Freight;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CustomsReferenceNumberType : RegistryBusinessObject, ICustomsNumberTypeCodeDescription, ICanDelete
	{
		public CustomsReferenceNumberType()
		{
		}

		public CustomsReferenceNumberType(CustomsReferenceNumberTypeCollection parent)
			: this()
		{
			Parent = parent;
		}

		[XmlIgnore]
		[BusinessObjectTestExclude]
		internal CustomsReferenceNumberTypeCollection Parent { get; set; }

		#region Schema/Constants

		public new class Schema : RegistryBusinessObject.Schema
		{
			public const string IsUnique = "IsUnique";
			public const string IsAutomation = "IsAutomation";
		}

		public static class eHubInterchangeReference
		{
			public const string HIR = "HIR";
		}

		public static class CustomsPermitClearanceNumbersCodes
		{
			public const string TSN = "TSN";
			public const string ATA = "ATA";
		}

		public static class CustomsAdditionalReferenceNumbersCodes
		{
			public const string COC = "COC";
			public const string AMS = "AMS";
			public const string UBR = "UBR";
			public const string CON = "CON";
			public const string CQN = "CQN";
			public const string CLC = "CLC";
			public const string BKG = "BKG";
			public const string RLB = "RLB";
			public const string OtherAgentReference = "OAG";
			public const string UniqueConsignmentReference = "UCR";
			public const string CustomsAuthorisationReference = "CAR";
			public const string LetterOfCreditNumber = "LCR";
			public const string AdvanceCargoAdvice = "ACA";
			public const string CargoControlNumber = "CCN";
			public const string PreviousCargoControlNumber = "PCN";
			public const string ContractNamedAccount = "NAC";
			public const string CourierConsignmentReference = "COU";
			public const string BagReference = "BAG";
			public const string CarrierQuoteNumber = "CQN";
			public const string SpotReference = "SPO";
			public const string AdvanceCargoInformationReference = "ACI";
			public const string ImportSecurityFilingReference = "ISF";
			public const string DeclarationReference = "JDR";
			public const string CustomerLoadReference = "CLR";
			public const string CarrierMessageReference = "CMR";
			public const string ExporterEORINumber = "EOE";
			public const string ImporterEORINumber = "EOI";
			public const string CargoTrackingNote = "CTK";
		}

		#endregion

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			isUnique = true;
		}

		protected override int MaxDescriptionLength
		{
			get { return 256; }
		}

		#region IsUnique

		[ReadOnlyMemberAttribute(nameof(SystemDefined))]
		public ZBool IsUnique
		{
			get { return isUnique; }
			set
			{
				if (SetNonPersistentPropertyValue(IsUniqueInfo, ref isUnique, value) && !IsValidationSuspended)
				{
					ValidateIsUnique();
				}
			}
		}
		ZBool isUnique;

		public ZPropertyInfo IsUniqueInfo
		{
			get { return GetZPropertyInfo(Schema.IsUnique); }
		}

		public void ValidateIsUnique()
		{
			IsUniqueInfo.ClearAllNotifications();
		}

		#endregion

		#region IsAutomation

		[ReadOnlyMemberAttribute(nameof(SystemDefined))]
		public ZBool IsAutomation
		{
			get { return isAutomation; }
			set
			{
				if (SetNonPersistentPropertyValue(IsAutomationInfo, ref isAutomation, value) && !IsValidationSuspended)
				{
					ValidateIsAutomation();
				}
			}
		}
		ZBool isAutomation;

		public ZPropertyInfo IsAutomationInfo
		{
			get { return GetZPropertyInfo(Schema.IsAutomation); }
		}

		public void ValidateIsAutomation()
		{
			IsAutomationInfo.ClearAllNotifications();
		}

		#endregion

		#region Read Only Members

		public bool Code_ReadOnly
		{
			get { return SystemDefined; }
		}

		public bool Description_ReadOnly
		{
			get { return SystemDefined; }
		}

		public bool EnglishDescription_ReadOnly
		{
			get { return SystemDefined; }
		}

		#endregion

		#region System Defined

		public bool SystemDefined
		{
			get; set;
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateIsUnique();
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var customsReferenceNumberType = new CustomsReferenceNumberType();
			customsReferenceNumberType.Code = Code;
			customsReferenceNumberType.Description = Description;
			customsReferenceNumberType.IsUnique = IsUnique;
			customsReferenceNumberType.IsAutomation = IsAutomation;
			customsReferenceNumberType.SystemDefined = SystemDefined;

			if (customsReferenceNumberType.Code == CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CarrierMessageReference)
			{
				customsReferenceNumberType.ReadOnly = true;
			}

			return customsReferenceNumberType;
		}

		#region XML Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			writer.WriteElementString(Schema.Code, Code);
			writer.WriteElementString(Schema.Description, EnglishDescription);
			writer.WriteElementString(Schema.IsUnique, IsUnique.ToString());
			writer.WriteElementString(Schema.IsAutomation, IsAutomation.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Code = reader.ReadElementString(Schema.Code);
			EnglishDescription = reader.ReadElementString(Schema.Description);
			isUnique = new ZBool(reader.ReadElementString(Schema.IsUnique));
			isAutomation = new ZBool(reader.ReadElementString(Schema.IsAutomation));
		}

		#endregion

		#region ICanDelete members

		bool ICanDelete.CanDelete
		{
			get { return !SystemDefined; }
		}

		MultilingualString ICanDelete.ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("825bcc22-d1d8-49cb-959e-7cf77a817be1", "This is a system defined value and cannot be deleted."); }
		}

		#endregion

		#region ICustomsNumberType members

		bool ICustomsNumberTypeCodeDescription.IsUnique
		{
			get { return IsUnique; }
		}

		bool ICustomsNumberTypeCodeDescription.IsAutomation
		{
			get { return IsAutomation; }
		}

		string ICodeDescription.Code
		{
			get { return Code; }
		}

		string ICodeDescription.Description
		{
			get { return Description; }
		}

		object ICodeDescription.PK
		{
			get { return base.PK; }
		}

		#endregion
	}
}
