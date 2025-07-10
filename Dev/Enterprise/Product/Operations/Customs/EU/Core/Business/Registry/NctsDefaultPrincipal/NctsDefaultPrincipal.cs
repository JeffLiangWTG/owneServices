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
	public class NctsDefaultPrincipal : AutoNctsDefaultPrincipal
	{
		public NctsDefaultPrincipal() : base()
		{
		}

		public NctsDefaultPrincipal(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
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
					WipeOutPrincipalIfNeeded();
				}
			}
		}

		void WipeOutPrincipalIfNeeded()
		{
			if (LeaveBlank)
			{
				Principal = ZGuid.Empty;
			}
		}

		#endregion

		#region Principal

		[List(nameof(PrincipalCollection))]
		[RelatedBusinessObject(nameof(PrincipalOrganization))]
		public override ZGuid Principal { get => base.Principal; set => base.Principal = value; }

		protected override bool Principal_ReadOnly => LeaveBlank;

		public OrgHeader PrincipalOrganization => CurrentFactory.Load<OrgHeader>(Principal);

		public OrgHeaderCollection PrincipalCollection => new OrgHeaderCollection(CurrentFactory);

		public override void ValidatePrincipal()
		{
			base.ValidatePrincipal();
			if (!LeaveBlank)
			{
				var principalInfo = PrincipalInfo;
				MandatoryValidation.CheckEntered(principalInfo);
				ListValidation.ErrorIfInvalidPK(principalInfo);
			}
		}

		#endregion

		#region Cloning, XML Reading and Writing

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new NctsDefaultPrincipal(fallbackLevel, factory);

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			LeaveBlank = reader.ReadElementStringAsZBool(Schema.LeaveBlank);
			Principal = ZGuid.TryParse(reader.ReadElementString(Schema.Principal), out var principalPK) ? principalPK : ZGuid.Empty;
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.LeaveBlank, LeaveBlank.ToString());
			writer.WriteElementString(Schema.Principal, Principal.ToString());
		}

		#endregion
	}
}
