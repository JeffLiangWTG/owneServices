using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class FreeWaitingTime : RegistryBusinessObjectTemplate, IOrgFreeWaitingTime
	{
		public FreeWaitingTime()
			: base()
		{ }

		public FreeWaitingTime(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{ }

		#region Schema

		public abstract class Schema
		{
			public const string CNTType = "CNTType";
			public const string DropMode = "DropMode";
			public const string CFS = "CFS";
			public const string CTO = "CTO";
			public const string CYD = "CYD";
			public const string CNE = "CNE";
			public const string CNR = "CNR";
			public const string Other = "Other";
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new FreeWaitingTime(fallbackLevel, factory);
		}

		#region Bound Properties

		#region CNTType

		public ZGuid CNTType
		{
			get { return cntType; }
			set
			{
				SetNonPersistentPropertyValue<ZGuid>(CNTTypeInfo, ref cntType, value);
				if (!IsValidationSuspended)
				{
					ValidateContainer();
				}
			}
		}

		public ZPropertyInfo CNTTypeInfo
		{
			get { return GetZPropertyInfo(Schema.CNTType); }
		}

		ZGuid cntType;

		#endregion

		#region DropMode

		public ZString DropMode
		{
			get { return dropMode; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(DropModeInfo, ref dropMode, value);
				if (!IsValidationSuspended)
				{
					ValidateDropMode();
				}
			}
		}

		public ZPropertyInfo DropModeInfo
		{
			get { return GetZPropertyInfo(Schema.DropMode); }
		}

		ZString dropMode;

		#endregion

		#region CFS

		public ZDateTime CFS
		{
			get { return cfs; }
			set
			{
				SetNonPersistentPropertyValue<ZDateTime>(CFSInfo, ref cfs, value);
				if (!IsValidationSuspended)
				{
					ValidateTime(CFSInfo);
				}
			}
		}

		public ZPropertyInfo CFSInfo
		{
			get { return GetZPropertyInfo(Schema.CFS); }
		}

		ZDateTime cfs;

		#endregion

		#region CTO

		public ZDateTime CTO
		{
			get { return cto; }
			set
			{
				SetNonPersistentPropertyValue<ZDateTime>(CTOInfo, ref cto, value);
				if (!IsValidationSuspended)
				{
					ValidateTime(CTOInfo);
				}
			}
		}

		public ZPropertyInfo CTOInfo
		{
			get { return GetZPropertyInfo(Schema.CTO); }
		}

		ZDateTime cto;

		#endregion

		#region CYD

		public ZDateTime CYD
		{
			get { return cyd; }
			set
			{
				SetNonPersistentPropertyValue<ZDateTime>(CYDInfo, ref cyd, value);
				if (!IsValidationSuspended)
				{
					ValidateTime(CYDInfo);
				}
			}
		}

		public ZPropertyInfo CYDInfo
		{
			get { return GetZPropertyInfo(Schema.CYD); }
		}

		ZDateTime cyd;

		#endregion

		#region CNE

		public ZDateTime CNE
		{
			get { return cne; }
			set
			{
				SetNonPersistentPropertyValue<ZDateTime>(CNEInfo, ref cne, value);
				if (!IsValidationSuspended)
				{
					ValidateTime(CNEInfo);
				}
			}
		}

		public ZPropertyInfo CNEInfo
		{
			get { return GetZPropertyInfo(Schema.CNE); }
		}

		ZDateTime cne;

		#endregion

		#region CNR

		public ZDateTime CNR
		{
			get { return cnr; }
			set
			{
				SetNonPersistentPropertyValue<ZDateTime>(CNRInfo, ref cnr, value);
				if (!IsValidationSuspended)
				{
					ValidateTime(CNRInfo);
				}
			}
		}

		public ZPropertyInfo CNRInfo
		{
			get { return GetZPropertyInfo(Schema.CNR); }
		}

		ZDateTime cnr;

		#endregion

		#region Other

		public ZDateTime Other
		{
			get { return other; }
			set
			{
				SetNonPersistentPropertyValue<ZDateTime>(OtherInfo, ref other, value);
				if (!IsValidationSuspended)
				{
					ValidateTime(OtherInfo);
				}
			}
		}

		public ZPropertyInfo OtherInfo
		{
			get { return GetZPropertyInfo(Schema.Other); }
		}

		ZDateTime other;

		#endregion

		#region BindToLists

		public IRefContainerCollection ContainerTypes
		{
			get { return (IRefContainerCollection)Activator.CreateInstance(ObjectFactory.GetType<IRefContainerCollection>(), new object[] { CurrentFactory }); }
		}

		public CodeDescriptionPairList DropModes
		{
			get
			{
				// Don't have access to MasterFiles.Business otherwise return CombinedEquipmentNeededList
				var list = new CodeDescriptionPairList();
				var askClient = true;
				list.AddRangeOverwriteIfExists(new FCLEquipmentNeededList(!askClient));
				list.AddRangeOverwriteIfExists(new LCLAIREquipmentNeededList(askClient));
				return list;
			}
		}

		#endregion

		#endregion

		#region Validation

		#region ValidateDropMode

		void ValidateDropMode()
		{
			DropModeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(DropModeInfo);
			ListValidation.ErrorIfInvalidCode(DropModeInfo, DropModes);
		}

		#endregion

		#region ValidateTime

		void ValidateTime(ZPropertyInfo timeInfo)
		{
			timeInfo.ClearAllNotifications();
			if (!timeInfo.Value.IsValid && !timeInfo.Value.IsEmpty)
			{
				timeInfo.AddError(Res.GetString("47d39e7d-1ed5-4c09-9cc8-1ecf5980a064", "Please enter a valid time."));
			}
		}

		#endregion

		#region ValidateContainer

		void ValidateContainer()
		{
			CNTTypeInfo.ClearAllNotifications();
			if (!CNTTypeInfo.Value.IsEmpty)
			{
				var refContainerTypePKs = ContainerTypes.Cast<BusinessObject>().Select(bo => bo.PK);
				if (!refContainerTypePKs.Contains((ZGuid)CNTTypeInfo.Value))
				{
					CNTTypeInfo.AddError(Res.GetString("6c6a5590-c510-4b64-8aa5-2dbff310f667", "Please enter a valid Container Type."));
				}
			}
		}

		#endregion

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.CNTType, CNTType.ToString());
			writer.WriteElementString(Schema.DropMode, DropMode);
			writer.WriteElementString(Schema.CFS, CFS.ToISO8601String());
			writer.WriteElementString(Schema.CTO, CTO.ToISO8601String());
			writer.WriteElementString(Schema.CYD, CYD.ToISO8601String());
			writer.WriteElementString(Schema.CNE, CNE.ToISO8601String());
			writer.WriteElementString(Schema.CNR, CNR.ToISO8601String());
			writer.WriteElementString(Schema.Other, Other.ToISO8601String());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ZGuid.TryParse(reader.ReadElementString(Schema.CNTType), out cntType);
			dropMode = reader.ReadElementString(Schema.DropMode);
			ZDateTime.TryParseISO8601Date(reader.ReadElementString(Schema.CFS), out cfs);
			ZDateTime.TryParseISO8601Date(reader.ReadElementString(Schema.CTO), out cto);
			ZDateTime.TryParseISO8601Date(reader.ReadElementString(Schema.CYD), out cyd);
			ZDateTime.TryParseISO8601Date(reader.ReadElementString(Schema.CNE), out cne);
			ZDateTime.TryParseISO8601Date(reader.ReadElementString(Schema.CNR), out cnr);
			ZDateTime.TryParseISO8601Date(reader.ReadElementString(Schema.Other), out other);
		}

		#endregion

		#region Unimplemented interfaces

		public ZGuid Address
		{
			get { return ZGuid.Empty; }
			set { }
		}

		public ZGuid Company
		{
			get { return ZGuid.Empty; }
			set { }
		}

		#endregion
	}
}
