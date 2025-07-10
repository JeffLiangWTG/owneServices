using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ShipmentInspectionType : RegistryBusinessObject, ICanDelete
	{
		#region Schema

		protected new abstract class Schema : RegistryBusinessObject.Schema
		{
			public const string ShowInList = "ShowInList";
			public const string AllowedOnPassengerFlights = "AllowedOnPassengerFlights";
		}

		#endregion

		public ShipmentInspectionType()
		{
		}

		public ShipmentInspectionType(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ShipmentInspectionType();
		}

		#endregion

		#region Parent Collection

		ShipmentInspectionTypeCollection ParentCollection
		{
			get { return (ShipmentInspectionTypeCollection)GetParentCollection(this, typeof(ShipmentInspectionTypeCollection)); }
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

		#region ShowInList

		public ZBool ShowInList
		{
			get { return showInList; }
			set
			{
				SetNonPersistentPropertyValue(ShowInListInfo, ref showInList, value);

				if (!IsValidationSuspended)
				{
					ValidateAll();
				}
			}
		}

		public ZPropertyInfo ShowInListInfo
		{
			get { return GetZPropertyInfo(Schema.ShowInList); }
		}

		ZBool showInList;

		#endregion

		#region AllowedOnPassengerFlights

		public ZBool AllowedOnPassengerFlights
		{
			get { return allowedOnPassengerFlights; }
			set
			{
				SetNonPersistentPropertyValue(AllowedOnPassengerFlightsInfo, ref allowedOnPassengerFlights, value);

				if (!IsValidationSuspended)
				{
					ValidateAll();
				}
			}
		}

		public ZPropertyInfo AllowedOnPassengerFlightsInfo
		{
			get { return GetZPropertyInfo(Schema.AllowedOnPassengerFlights); }
		}

		ZBool allowedOnPassengerFlights;

		#endregion

		#region System Defined

		public ZBool SystemDefined
		{
			get { return ParentCollection != null && ParentCollection.Parent != null && ParentCollection.Parent.SystemDefinedTypes.ContainsCode(Code); }
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

		#region Overrides

		public override MultilingualString Description
		{
			get => base.Description;
			set
			{
				base.Description = value;

				if (!IsValidationSuspended)
				{
					ValidateAll();
				}
			}
		}

		protected override int MaxDescriptionLength
		{
			get { return 256; }
		}

		#endregion

		#region XML Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.AllowedOnPassengerFlights, AllowedOnPassengerFlights.ToString());
			writer.WriteElementString(Schema.ShowInList, ShowInList.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			base.ReadElements(reader);
			AllowedOnPassengerFlights = reader.ReadElementStringAsZBool(Schema.AllowedOnPassengerFlights);
			ShowInList = reader.ReadElementStringAsZBool(Schema.ShowInList);
		}

		#endregion

		#region Constants

		public const string AviationSecurity_Unknown_Code = "UNK";
		public const string AviationSecurity_Approved_Code = "APP";

		#endregion

		#region Saving

		protected override void RunPreSaveValidationCore()
		{
			ValidateAll();
			base.RunPreSaveValidationCore();
		}

		#endregion

		#region Validation

		public void ValidateAll()
		{
			ValidateCode();
			ValidateDescription();
		}

		protected override void ValidateCodeCore()
		{
			ClearRowNotifications();

			if (Code.EqualsIgnoringCase(AviationSecurity_Unknown_Code) || Code.EqualsIgnoringCase(AviationSecurity_Approved_Code))
			{
				CodeInfo.AddError(Res.GetString("d3552aed-a237-47ae-ae79-b1f2514b5f8f", "UNK and APP inspection statuses are invalid for this registry."));
			}

			if (!SystemDefined)
			{
				CodeInfo.AddWarning(Res.GetString("f07dc403-7693-455f-86a1-d63f37ae6f30", "This is a custom inspection type. Custom inspection types, if not authorized by LGA, will cause rejection by airlines and may result in cargo being re-screened, delayed or not uplifted."));
			}

			base.ValidateCodeCore();
		}

		#endregion

		public static string GetIATAExemptionCode(string cargowiseExemptionCode)
		{
			switch (cargowiseExemptionCode)
			{
				case ExemptionCodes.Codes.SmallUndersizedShipments:
					return "SMUS";
				case ExemptionCodes.Codes.Mail:
					return "MAIL";
				case ExemptionCodes.Codes.BiomedicalSamples:
					return "BIOM";
				case ExemptionCodes.Codes.DiplomaticBagsOrDiplomaticMail:
					return "DIPL";
				case ExemptionCodes.Codes.LifeSavingMaterials:
					return "LFSM";
				case ExemptionCodes.Codes.NuclearMaterial:
					return "NUCL";
				case ExemptionCodes.Codes.TransferOrTransshipment:
					return "TRNS";
				case ExemptionCodes.Codes.GovernmentApprovedReliableOrganization:
					return "623b";
				case ExemptionCodes.Codes.AdHocMovementsOfCargo:
					return "623c";

				default:
					return null;
			}
		}

		public static bool IsIATAExemptionCode(string code)
		{
			return GetIATAExemptionCodes().Contains(code);
		}

		public static string[] GetIATAExemptionCodes()
		{
			string[] iATACodes = { "SMUS", "MAIL", "BIOM", "DIPL", "LFSM", "NUCL", "TRNS", "623b", "623c" };
			return iATACodes;
		}
	}
}
