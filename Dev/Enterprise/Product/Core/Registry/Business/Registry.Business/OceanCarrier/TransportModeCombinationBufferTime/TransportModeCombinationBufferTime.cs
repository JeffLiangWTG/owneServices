using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class TransportModeCombinationBufferTime : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string LoadTransportMode = "LoadTransportMode";
			public const string UnloadTransportMode = "UnloadTransportMode";
			public const string BufferTimeInHours = "BufferTimeInHours";
		}

		#endregion
		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.UnloadTransportMode, UnloadTransportMode);
			writer.WriteElementString(Schema.LoadTransportMode, LoadTransportMode);
			writer.WriteElementString(Schema.BufferTimeInHours, BufferTimeInHours.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			UnloadTransportMode = reader.ReadElementString(Schema.UnloadTransportMode);
			LoadTransportMode = reader.ReadElementString(Schema.LoadTransportMode);
			BufferTimeInHours = new ZInt(reader.ReadElementString(Schema.BufferTimeInHours));
		}

		#region Properties

		#region LoadTransportMode

		[ReadOnly(true)]
		public ZString LoadTransportMode
		{
			get { return fLoadTransportMode; }
			set
			{
				SetNonPersistentPropertyValue(LoadTransportModeInfo, ref fLoadTransportMode, value);
			}
		}

		ZString fLoadTransportMode;

		public ZPropertyInfo LoadTransportModeInfo
		{
			get { return GetZPropertyInfo(Schema.LoadTransportMode); }
		}

		#endregion

		#region UnloadTransportMode

		[ReadOnly(true)]
		public ZString UnloadTransportMode
		{
			get { return fUnloadTransportMode; }
			set
			{
				SetNonPersistentPropertyValue(UnloadTransportModeInfo, ref fUnloadTransportMode, value);
			}
		}

		ZString fUnloadTransportMode;

		public ZPropertyInfo UnloadTransportModeInfo
		{
			get { return GetZPropertyInfo(Schema.UnloadTransportMode); }
		}

		#endregion

		#region BufferTimeInHours

		public ZInt BufferTimeInHours
		{
			get { return fBufferTimeInHours; }
			set
			{
				SetNonPersistentPropertyValue(BufferTimeInHoursInfo, ref fBufferTimeInHours, value);
				if (!IsValidationSuspended)
				{
					ValidateBufferTimeInHoursInfo();
				}
			}
		}

		ZInt fBufferTimeInHours;

		public ZPropertyInfo BufferTimeInHoursInfo
		{
			get { return GetZPropertyInfo(Schema.BufferTimeInHours); }
		}

		#endregion
		#endregion

		#region Validation

		public void ValidateBufferTimeInHoursInfo()
		{
			BufferTimeInHoursInfo.ClearAllNotifications();
			MandatoryValidation.CheckNotNegative(BufferTimeInHoursInfo);
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new TransportModeCombinationBufferTime();
		}

		#endregion
	}
}
