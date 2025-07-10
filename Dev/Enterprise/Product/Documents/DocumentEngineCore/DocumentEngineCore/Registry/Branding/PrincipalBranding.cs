using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	[XmlSerializerAssembly("Enterprise.DocumentEngineCore.XmlSerializers")]
	public class PrincipalBranding : DocumentBrandingBusinessObject
	{
		protected new class Schema : ClientAndAgentBrandingBusinessObject.Schema
		{
			public const string PrincipalPK = "PrincipalPK";
		}

		public PrincipalBranding()
		{
		}

		public PrincipalBranding(BusinessObjectFactory factory, FallbackLevel fallbackLevel)
			: base(fallbackLevel, factory)
		{
		}

		#region Overrides

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PrincipalBranding(factory, fallbackLevel);
		}

		protected override void WriteMoreElements(XmlWriter writer)
		{
			base.WriteMoreElements(writer);
			writer.WriteElementString(Schema.PrincipalPK, PrincipalPK.ToString());
		}

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);
			PrincipalPK = new ZGuid(reader.ReadElementString(Schema.PrincipalPK));
		}

		protected override CodeDescriptionPairList GetNewCodeList()
		{
			return new CodeDescriptionPairList();
		}

		protected override IRegistryItem BrandingOptionRegistryItem
		{
			get { return null; }
		}

		protected override string BrandingOptionTitle
		{
			get { return ""; }
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("9488742a-0232-4727-a350-dc2d496e39eb", "Principal Branding"); }
		}

		#endregion

		#region Properties

		#region Code

		protected override void ValidateCodeCore()
		{
		}

		#endregion

		#region PrincipalPK

		public ZGuid PrincipalPK
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return principalPK; }
			set
			{
				principalPK = value;
				if (!IsValidationSuspended)
				{
					ValidatePrincipalPK();
				}
				PrincipalPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo PrincipalPKInfo
		{
			get { return GetZPropertyInfo(Schema.PrincipalPK); }
		}

		public void ValidatePrincipalPK()
		{
			PrincipalPKInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(PrincipalPKInfo, Res.GetString("87b1f08b-90fe-42f2-a3a5-a61bdcf47b8f", "Principal"));
			ListValidation.ErrorIfInvalidPK(PrincipalPKInfo, Principals, ResString.GetMultilingualString("39bc41eb-718d-4d72-80e6-2a85581fe607", "Enter a valid principal."));

			if (ParentCollections.Count > 0)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(PrincipalPKInfo, Res.GetString("88e9dc4a-5e65-48c4-828f-4bf1127d5bff", "A principal may only appear once in this list."));
			}
		}

		ZGuid principalPK;

		#endregion

		#endregion

		#region Principals

		public BusinessObjectCollection Principals
		{
			get
			{
				if (principals == null)
				{
					principals = (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<MasterFiles.Integration.IShipsAgencyPrincipalCollection>(), CurrentFactory);
				}

				return principals;
			}
		}

		BusinessObjectCollection principals;

		#endregion
	}
}
