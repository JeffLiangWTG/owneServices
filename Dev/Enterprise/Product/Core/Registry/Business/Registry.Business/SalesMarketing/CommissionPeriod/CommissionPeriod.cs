using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CommissionPeriod : RegistryBusinessObject
	{
		const int maxEntitlementPeriod = 240;

		#region Schema

		public new abstract class Schema : RegistryBusinessObject.Schema
		{
			public const string Start = "Start";
			public const string End = "End";
			public const string IsEnabled = "IsEnabled";
		}

		#endregion

		#region Properties

		#region Code

		protected override int CodeMaxLengthDefaultValue
		{
			get { return OrgCommissionAgreementRecipientRateSchema.CAT_CommissionPeriod.MaxLength; }
		}

		#endregion

		#region Start

		public ZInt Start
		{
			get { return start; }
			set
			{
				SetNonPersistentPropertyValue(StartInfo, ref start, value);

				if (!IsValidationSuspended)
				{
					ValidateStart();
				}
			}
		}
		ZInt start;

		public ZPropertyInfo StartInfo
		{
			get { return GetZPropertyInfo(Schema.Start); }
		}

		#endregion

		#region End

		public ZInt End
		{
			get { return end; }
			set
			{
				SetNonPersistentPropertyValue(EndInfo, ref end, value);

				if (!IsValidationSuspended)
				{
					ValidateEnd();
				}
			}
		}
		ZInt end;

		public ZPropertyInfo EndInfo
		{
			get { return GetZPropertyInfo(Schema.End); }
		}

		#endregion

		#region IsEnabled

		public ZBool IsEnabled
		{
			get { return isEnabled; }
			set
			{
				SetNonPersistentPropertyValue(IsEnabledInfo, ref isEnabled, value);
			}
		}
		ZBool isEnabled;

		public ZPropertyInfo IsEnabledInfo
		{
			get { return GetZPropertyInfo(Schema.IsEnabled); }
		}

		#endregion

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CommissionPeriod();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);

			var period = (CommissionPeriod)clone;
			period.Start = Start;
			period.End = End;
			period.IsEnabled = IsEnabled;
		}

		#endregion

		#region Validation

		public void ValidateStart()
		{
			StartInfo.ClearAllNotifications();

			if (Start < 0 || Start > maxEntitlementPeriod)
			{
				StartInfo.AddError(ResString.GetMultilingualString("2d14a0d2-97dc-4254-9114-5fea0b6016db", "Must be between 0 and {0} inclusive.", maxEntitlementPeriod));
			}
			else if (Start > End && End != 0)
			{
				StartInfo.AddError(ResString.GetMultilingualString("64b04304-4bf9-4b24-be21-01d88aa44581", "Must be smaller than End."));
			}
		}

		public void ValidateEnd()
		{
			EndInfo.ClearAllNotifications();

			if (End < 0 || End > maxEntitlementPeriod)
			{
				EndInfo.AddError(ResString.GetMultilingualString("2d14a0d2-97dc-4254-9114-5fea0b6016db", "Must be between 0 and {0} inclusive.", maxEntitlementPeriod));
			}
			else if (End < Start && End != 0)
			{
				EndInfo.AddError(ResString.GetMultilingualString("c82f4ca3-c08a-4291-996d-0c8354bc766b", "Must be larger than Start."));
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateStart();
			ValidateEnd();
		}

		#endregion

		#region XML Serialisation

		protected override void WriteMoreElements(XmlWriter writer)
		{
			base.WriteMoreElements(writer);
			writer.WriteElementString(Schema.Start, Start.ToString());
			writer.WriteElementString(Schema.End, End.ToString());
			writer.WriteElementString(Schema.IsEnabled, IsEnabled.ToString());
		}

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);
			Start = new ZInt(reader.ReadElementString(Schema.Start));
			End = new ZInt(reader.ReadElementString(Schema.End));
			IsEnabled = new ZBool(reader.ReadElementString(Schema.IsEnabled));
		}

		#endregion
	}
}
