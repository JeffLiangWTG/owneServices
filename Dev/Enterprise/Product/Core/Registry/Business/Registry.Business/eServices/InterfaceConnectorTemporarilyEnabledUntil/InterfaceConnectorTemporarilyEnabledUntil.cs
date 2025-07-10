using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class InterfaceConnectorTemporarilyEnabledUntil : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string EnabledUntil = "EnabledUntil";
		}

		#endregion

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();

			enabledUntil = ZDateTime.Empty;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new InterfaceConnectorTemporarilyEnabledUntil();
		}

		#region Enabled Until

		public ZDateTime EnabledUntil
		{
			get { return enabledUntil; }
			set
			{
				SetNonPersistentPropertyValue<ZDateTime>(EnabledUntilInfo, ref enabledUntil, value);

				if (!IsValidationSuspended)
				{
					ValidateEnabledUntil();
				}
			}
		}

		public ZPropertyInfo EnabledUntilInfo
		{
			get { return GetZPropertyInfo(Schema.EnabledUntil, "Enabled Until Date"); }
		}

		public void ValidateEnabledUntil()
		{
			EnabledUntilInfo.ClearAllNotifications();

			if (enabledUntil.IsEmpty)
			{
				return;
			}

			var now = ZDateTime.Now;
			var minimum = now.Date;
			var maximum = now.AddMonths(12).Date;

			if (enabledUntil < minimum || enabledUntil > maximum)
			{
				EnabledUntilInfo.AddError(Res.GetString("75a6b41c-8275-4e88-8e6d-46c14e0bf0d8", "The value must be between {0} and {1}.", minimum.ToShortDateString(), maximum.ToShortDateString()));
			}
		}

		ZDateTime enabledUntil;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ClearRowNotifications();

			ValidateEnabledUntil();
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.EnabledUntil, enabledUntil.ToISO8601String());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			if (!ZDateTime.TryParseISO8601Date(reader.ReadElementString(Schema.EnabledUntil), out enabledUntil))
			{
				enabledUntil = ZDateTime.Empty;
			}
		}

		#endregion

	}
}
