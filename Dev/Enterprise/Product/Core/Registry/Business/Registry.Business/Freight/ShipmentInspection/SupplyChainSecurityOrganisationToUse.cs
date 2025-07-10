using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class SupplyChainSecurityOrganisationToUse : RegistryBusinessObject
	{
		#region Schema

		protected new abstract class Schema : RegistryBusinessObject.Schema
		{
			public const string ValidationCode = "ValidationCode";
		}

		#endregion

		public SupplyChainSecurityOrganisationToUse()
		{
		}

		public SupplyChainSecurityOrganisationToUse(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new SupplyChainSecurityOrganisationToUse();
		}

		#endregion

		#region Parent Collection

		SupplyChainSecurityOrganisationToUseCollection ParentCollection
		{
			get { return (SupplyChainSecurityOrganisationToUseCollection)GetParentCollection(this, typeof(SupplyChainSecurityOrganisationToUseCollection)); }
		}

		#endregion

		#region Read Only Members

		public bool Code_ReadOnly
		{
			get { return true; }
		}

		public bool Description_ReadOnly
		{
			get { return true; }
		}

		public bool EnglishDescription_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region ValidationCode

		[List("ValidationCodeList")]
		public ZString ValidationCode
		{
			get { return validationCode; }
			set
			{
				SetNonPersistentPropertyValue(ValidationCodeInfo, ref validationCode, value);
				if (!IsValidationSuspended)
				{
					ValidateValidationCode();
				}
			}
		}

		public ZPropertyInfo ValidationCodeInfo
		{
			get { return GetZPropertyInfo(Schema.ValidationCode); }
		}

		ZString validationCode;

		public void ValidateValidationCode()
		{
			ValidationCodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ValidationCodeInfo);
			ListValidation.ErrorIfInvalidCode(ValidationCodeInfo);

			ValidateLocalClientConsignor();
		}

		void ValidateLocalClientConsignor()
		{
			if (ParentCollection != null)
			{
				if (Code == SupplyChainSecurityOrganisationTypes.Consignor || Code == SupplyChainSecurityOrganisationTypes.LocalClient)
				{
					var countLocalClientConsignorIsNo = ParentCollection.Cast<SupplyChainSecurityOrganisationToUse>()
						.Count(x => (x.Code == SupplyChainSecurityOrganisationTypes.Consignor || x.Code == SupplyChainSecurityOrganisationTypes.LocalClient) && x.validationCode == ValidationCodes.No);

					if (countLocalClientConsignorIsNo > 1)
					{
						ValidationCodeInfo.AddError(Res.GetString("016a27be-531e-4f8a-8db8-b52404ab9d10", "Either Consignor or Local Client must be set to Yes/Warning."));
					}
				}
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateValidationCode();
		}

		#endregion

		#region Lookups

		public CodeDescriptionPairList ValidationCodeList
		{
			get
			{
				if (validationCodeList == null)
				{
					validationCodeList = GetValidationCodeList();
				}

				return validationCodeList;
			}
		}
		CodeDescriptionPairList validationCodeList;

		protected virtual CodeDescriptionPairList GetValidationCodeList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(ValidationCodes.Yes, ResString.GetMultilingualString("aaa64b56-d1fe-4a75-8dbb-9acb4491d30f", "Yes"));
			result.AddPair(ValidationCodes.No, ResString.GetMultilingualString("f6e957ac-164c-43f6-97fe-75074993e1e2", "No"));
			result.AddPair(ValidationCodes.Warning, ResString.GetMultilingualString("353cf87b-affb-41a8-9a93-d2bc31d1a536", "Warning"));

			return result;
		}

		public static class ValidationCodes
		{
			public const string Yes = "YES";
			public const string No = "NO";
			public const string Warning = "WARN";
		}

		#endregion

		#region Overrides

		protected override int MaxDescriptionLength
		{
			get { return 256; }
		}

		#endregion

		#region XML Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.ValidationCode, ValidationCode.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			base.ReadElements(reader);
			ValidationCode = reader.ReadElementString(Schema.ValidationCode);
		}

		#endregion
	}
}
