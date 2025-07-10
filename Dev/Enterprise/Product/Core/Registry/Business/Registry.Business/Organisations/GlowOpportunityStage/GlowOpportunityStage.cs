using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class GlowOpportunityStage : CodeDescriptionBool
	{
		#region Schema

		new abstract class Schema : CodeDescriptionBool.Schema
		{
			public const string WinProbability = "WinProbability";
		}

		#endregion

		#region Properties

		#region Win Probability

		public ZInt WinProbability
		{
			get => winProbability;
			set
			{
				SetNonPersistentPropertyValue(WinProbabilityInfo, ref winProbability, value);
				if (!IsValidationSuspended)
				{
					ValidateWinProbability();
				}
			}
		}
		ZInt winProbability;

		public ZPropertyInfo WinProbabilityInfo => GetZPropertyInfo(Schema.WinProbability);

		void ValidateWinProbability()
		{
			WinProbabilityInfo.ClearAllNotifications();

			if (winProbability < 0 || winProbability > 100)
			{
				WinProbabilityInfo.AddError(Res.GetString("69df1319-0bd2-4d80-9373-76c1557478bd", "Win Probability must be an integer from 0 to 100"));
			}
		}

		#endregion

		#endregion

		protected override bool IsDescriptionMandatory => true;

		protected override RegistryBusinessObjectTemplate GetClone(ZArchitecture.Environment.FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new GlowOpportunityStage();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			var oppStage = clone as GlowOpportunityStage;
			oppStage.WinProbability = WinProbability;
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateWinProbability();
		}

		#region XML Serialisation

		protected override void WriteMoreElements(XmlWriter writer)
		{
			base.WriteMoreElements(writer);
			writer.WriteElementString(Schema.WinProbability, WinProbability.ToString());
		}

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);
			if (int.TryParse(reader.ReadElementString(Schema.WinProbability), out var value))
			{
				WinProbability = value;
			}
		}

		#endregion
	}
}
