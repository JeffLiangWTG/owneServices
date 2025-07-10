using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CreditTemporaryIncreaseAuthorisationSettings : AmountOrPercentageBasedThreeLevelAuthorisationRequirement, IObsoleteValidation
	{
		const int MaxDaysToExpirtyAllowed = 365;

		public new class Schema : AmountOrPercentageBasedThreeLevelAuthorisationRequirement.Schema
		{
			public const string DaysToExpiry = "DaysToExpiry";
		}

		#region DaysToExpiry

		public virtual ZInt DaysToExpiry
		{
			get { return daysToExpiry; }
			set
			{
				SetNonPersistentPropertyValue<ZInt>(DaysToExpiryInfo, ref daysToExpiry, value);
				if (!IsValidationSuspended)
				{
					ValidateDaysToExpiry();
				}
			}
		}

		public virtual ZPropertyInfo DaysToExpiryInfo
		{
			get { return GetZPropertyInfo(Schema.DaysToExpiry); }
		}

		public void ValidateDaysToExpiry()
		{
			ValidateDaysToExpiryCore();
		}

		protected virtual void ValidateDaysToExpiryCore()
		{
			DaysToExpiryInfo.ClearAllNotifications();
			CompareValidation.CheckNumberGreaterThanZero(DaysToExpiryInfo);

			if (DaysToExpiry > MaxDaysToExpirtyAllowed)
			{
				DaysToExpiryInfo.AddError(Res.GetString("cc13739e-4f6d-46ad-8b48-aa2664eb2705", "Please enter a value up to {0}", MaxDaysToExpirtyAllowed));
			}
		}

		ZInt daysToExpiry;

		#endregion

		#region Validation Overrides

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateDaysToExpiry();
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.DaysToExpiry, DaysToExpiry.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			base.ReadElements(reader);
			DaysToExpiry = ZInt.Parse(reader.ReadElementString(Schema.DaysToExpiry));
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CreditTemporaryIncreaseAuthorisationSettings();
		}
	}
}
