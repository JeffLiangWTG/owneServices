using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.EU.Registry
{
	[XmlSerializerAssembly("Enterprise.Customs.EU.Business.XmlSerializers")]
	public class NctsDefaultTraderAtDestination : AutoNctsDefaultTraderAtDestination
	{
		public NctsDefaultTraderAtDestination() : base()
		{
		}

		public NctsDefaultTraderAtDestination(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{
		}

		#region LeaveBlank

		public override ZBool LeaveBlank
		{
			get => base.LeaveBlank;
			set
			{
				var oldValue = LeaveBlank;
				base.LeaveBlank = value;
				if (oldValue != value)
				{
					WipeOutTraderAtDestinationIfNeeded();
				}
			}
		}

		void WipeOutTraderAtDestinationIfNeeded()
		{
			if (LeaveBlank)
			{
				TraderAtDestination = ZGuid.Empty;
			}
		}

		#endregion

		#region TraderAtDestination

		[List(nameof(TraderAtDestinationCollection))]
		[RelatedBusinessObject(nameof(TraderAtDestinationOrganization))]
		public override ZGuid TraderAtDestination { get => base.TraderAtDestination; set => base.TraderAtDestination = value; }

		protected override bool TraderAtDestination_ReadOnly => LeaveBlank;

		public OrgHeader TraderAtDestinationOrganization => CurrentFactory.Load<OrgHeader>(TraderAtDestination);

		public OrgHeaderCollection TraderAtDestinationCollection => new OrgHeaderCollection(CurrentFactory);

		public override void ValidateTraderAtDestination()
		{
			base.ValidateTraderAtDestination();
			if (!LeaveBlank)
			{
				var principalInfo = TraderAtDestinationInfo;
				MandatoryValidation.CheckEntered(principalInfo);
				ListValidation.ErrorIfInvalidPK(principalInfo);
			}
		}

		#endregion

		#region Cloning, XML Reading and Writing

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new NctsDefaultTraderAtDestination(fallbackLevel, factory);

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			LeaveBlank = reader.ReadElementStringAsZBool(Schema.LeaveBlank);
			TraderAtDestination = ZGuid.TryParse(reader.ReadElementString(Schema.TraderAtDestination), out var traderPK) ? traderPK : ZGuid.Empty;
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.LeaveBlank, LeaveBlank.ToString());
			writer.WriteElementString(Schema.TraderAtDestination, TraderAtDestination.ToString());
		}

		#endregion
	}
}
