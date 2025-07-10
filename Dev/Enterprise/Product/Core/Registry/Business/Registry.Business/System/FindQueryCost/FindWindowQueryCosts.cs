using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class FindWindowQueryCosts : RegistryBusinessObjectTemplate
	{
		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new FindWindowQueryCosts();
		}

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			AllowedCost = 50;
			MaximalCost = 100;
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			AllowedCost = reader.ReadElementStringAsZInt(Schema.AllowedCost);
			MaximalCost = reader.ReadElementStringAsZInt(Schema.MaximalCost);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.AllowedCost, AllowedCost.ToString());
			writer.WriteElementString(Schema.MaximalCost, MaximalCost.ToString());
		}

		ZInt allowedCost;
		ZInt maximalCost;

		#region SuppressResourceStringsCheckRegion

		public static class Schema
		{
			public const string AllowedCost = "AllowedCost";
			public const string MaximalCost = "MaximalCost";
		}

		#endregion

		#region Properties

		public ZInt AllowedCost
		{
			get { return allowedCost; }
			set
			{
				SetNonPersistentPropertyValue(AllowedCostInfo, ref allowedCost, value);
				if (!IsValidationSuspended)
				{
					ValidateAllowedCost();
					ValidateMaximalCost();
				}
			}
		}

		void ValidateAllowedCost()
		{
			AllowedCostInfo.ClearAllNotifications();
			CompareValidation.CheckWithinRange(AllowedCostInfo, 1, 600);
			CompareValidation.CheckLessThanOrEqualTo(AllowedCostInfo, MaximalCost);
		}

		public ZPropertyInfo AllowedCostInfo => GetZPropertyInfo(Schema.AllowedCost);

		public ZInt MaximalCost
		{
			get { return maximalCost; }
			set
			{
				SetNonPersistentPropertyValue(MaximalCostInfo, ref maximalCost, value);
				if (!IsValidationSuspended)
				{
					ValidateAllowedCost();
					ValidateMaximalCost();
				}
			}
		}

		void ValidateMaximalCost()
		{
			MaximalCostInfo.ClearAllNotifications();
			CompareValidation.CheckWithinRange(MaximalCostInfo, 1, 3600);
			CompareValidation.CheckGreaterThanOrEqualTo(MaximalCostInfo, AllowedCost);
		}

		public ZPropertyInfo MaximalCostInfo => GetZPropertyInfo(Schema.MaximalCost);

		#endregion
	}
}
