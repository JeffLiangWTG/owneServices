using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.UPE
{
	[XmlSerializerAssembly("ZClientUPE.XmlSerializers")]
	public class UPEBranchIDsRegistryObject : RegistryBusinessObjectTemplate
	{
		abstract class UPEBranchIDsSchema
		{
			public const string FirstArrivalPort = "FirstArrivalPort";
			public const string BuildingID = "BuildingID";
		}

		public UPEBranchIDsRegistryObject()
		{
		}

		#region Properties

		#region FirstArrivalPort

		public ZGuid FirstArrivalPort
		{
			get
			{
				return firstArrivalPort;
			}
			set
			{
				if (firstArrivalPort != value)
				{
					SetNonPersistentPropertyValue(FirstArrivalPortInfo, ref firstArrivalPort, value);
				}

				if (!IsValidationSuspended)
				{
					ValidateFirstArrivalPort();
				}
			}
		}

		ZGuid firstArrivalPort;

		public ZPropertyInfo FirstArrivalPortInfo
		{
			get { return GetZPropertyInfo(UPEBranchIDsSchema.FirstArrivalPort); }
		}

		public void ValidateFirstArrivalPort()
		{
			FirstArrivalPortInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(FirstArrivalPortInfo);
			ListValidation.ErrorIfInvalidPK(FirstArrivalPortInfo, UNLOCO_List);
		}

		#endregion

		#region Building ID

		[CargoWise.ComponentModel.MaxLength(7)]
		public ZString BuildingID
		{
			get
			{
				return fBuildingID;
			}
			set
			{
				if (fBuildingID != value)
				{
					CheckMaximumLength(BuildingIDInfo, value);
					SetNonPersistentPropertyValue(BuildingIDInfo, ref fBuildingID, value.ToUpper());
				}

				if (!IsValidationSuspended)
				{
					ValidateBuildingID();
				}
			}
		}

		public ZPropertyInfo BuildingIDInfo
		{
			get { return GetZPropertyInfo(UPEBranchIDsSchema.BuildingID); }
		}

		ZString fBuildingID;

		public void ValidateBuildingID()
		{
			BuildingIDInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(BuildingIDInfo);
			if (BuildingID.Length < 7)
			{
				BuildingIDInfo.AddError("Building ID should be 7 chars length");
			}
		}

		#endregion

		#endregion

		#region Overrides

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new UPEBranchIDsRegistryObject();
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(UPEBranchIDsSchema.FirstArrivalPort, FirstArrivalPort.ToString());
			writer.WriteElementString(UPEBranchIDsSchema.BuildingID, BuildingID.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			FirstArrivalPort = new ZGuid(reader.ReadElementString(UPEBranchIDsSchema.FirstArrivalPort));
			BuildingID = new ZString(reader.ReadElementString(UPEBranchIDsSchema.BuildingID));
		}

		#endregion

		#region Lists

		#region UNLOCO_List

		RefUNLOCOCollection unloco_List;
		public RefUNLOCOCollection UNLOCO_List
		{
			get
			{
				if (unloco_List == null)
				{
					unloco_List = new RefUNLOCOCollection(LocalFactory);
				}
				return unloco_List;
			}
		}

		#endregion

		#endregion

		#region LocalFactory

		BusinessObjectFactory LocalFactory
		{
			get
			{
				if (fFactory == null)
				{
					fFactory = new BusinessObjectFactory();
				}
				return fFactory;
			}
		}
		BusinessObjectFactory fFactory;

		#endregion
	}
}
