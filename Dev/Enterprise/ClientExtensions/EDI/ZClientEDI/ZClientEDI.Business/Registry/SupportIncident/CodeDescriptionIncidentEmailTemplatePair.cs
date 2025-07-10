using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class CodeDescriptionIncidentEmailTemplatePair : RegistryBusinessObject
	{
		#region Schema

		protected new abstract class Schema : RegistryBusinessObject.Schema
		{
			public const string Product = "Product";
			public const string NeedUpgrade = "NeedUpgrade";
		}

		#endregion

		public CodeDescriptionIncidentEmailTemplatePair() { }

		public CodeDescriptionIncidentEmailTemplatePair(Type docSourceType) : base()
		{
			this.docSourceType = docSourceType;
		}

		public CodeDescriptionIncidentEmailTemplatePair(Type docSourceType, NotificationEmailTemplate legacyAndERequestV1EmailTemplate, NotificationEmailTemplate eRequestV2EmailTemplate)
			: this(docSourceType)
		{
			emailTemplates = new IncidentEmailTemplatePair(docSourceType, legacyAndERequestV1EmailTemplate, eRequestV2EmailTemplate);
		}

		protected override bool IsCodeUniqueInCollection
		{
			get { return false; }
		}

		#region Code

		protected override void ValidateCodeCore()
		{
			base.ValidateCodeCore();
			var collection = GetParentCollection(this, typeof(CodeDescriptionIncidentEmailTemplatePairCollection));
			if (collection != null && collection.Cast<CodeDescriptionIncidentEmailTemplatePair>().Any(pair => pair != this && pair.Code == Code && pair.Product == Product && pair.NeedUpgrade == NeedUpgrade))
			{
				CodeInfo.AddError("The code and product combination have been duplicated and must be unique.");
			}
		}

		#endregion

		#region DocSourceType

		Type docSourceType;
		public Type DocSourceType
		{
			get { return docSourceType; }
			set
			{
				docSourceType = value;
				EmailTemplates.DocSourceType = value;
			}
		}

		IDocumentFieldDefinitionCollection documentFields;
		public IDocumentFieldDefinitionCollection DocumentFields
		{
			get { return documentFields ?? (documentFields = new DocumentFieldAttributeFinder().FindProperties(DocSourceType)); }
		}

		#endregion

		#region Product

		[List("ProductList")]
		[MaxLength(3)]
		public ZString Product
		{
			get { return product; }
			set
			{
				if (value != product)
				{
					CheckMaximumLength(ProductInfo, value);
					SetNonPersistentPropertyValue(ProductInfo, ref product, value);

					if (!IsValidationSuspended)
					{
						ValidateProduct();
						ValidateCode();
						ValidateNeedUpgrade();
					}
				}
			}
		}
		ZString product;

		public ZPropertyInfo ProductInfo
		{
			get { return GetZPropertyInfo(Schema.Product); }
		}

		public CodeDescriptionPairList ProductList
		{
			get
			{
				return new ProductTypes();
			}
		}

		void ValidateProduct()
		{
			ProductInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(ProductInfo);
		}

		#endregion

		#region NeedUpgrade

		public ZBool NeedUpgrade
		{
			get { return needUpgrade; }
			set
			{
				if (value != needUpgrade)
				{
					SetNonPersistentPropertyValue(NeedUpgradeInfo, ref needUpgrade, value);
					if (!IsValidationSuspended)
					{
						ValidateNeedUpgrade();
					}
				}
			}
		}

		ZBool needUpgrade;

		public ZPropertyInfo NeedUpgradeInfo => GetZPropertyInfo(Schema.NeedUpgrade);

		void ValidateNeedUpgrade()
		{
			NeedUpgradeInfo.ClearAllNotifications();
			if (NeedUpgrade && Product != ProductTypes.Codes.Enterprise)
			{
				NeedUpgradeInfo.AddError($"Need Upgrade option is only valid for {ProductTypes.Codes.Enterprise}.");
			}
		}

		#endregion

		#region ReadOnly

		public override bool ReadOnly
		{
			get { return base.ReadOnly; }
			set
			{
				base.ReadOnly = value;

				EmailTemplates.ReadOnly = value;
			}
		}

		#endregion

		#region MaxDescriptionLength

		protected override int MaxDescriptionLength
		{
			get { return 256; }
		}

		#endregion

		#region Email Template

		IncidentEmailTemplatePair emailTemplates;
		public IncidentEmailTemplatePair EmailTemplates
		{
			get { return emailTemplates ?? (emailTemplates = new IncidentEmailTemplatePair(docSourceType)); }
			set { emailTemplates = value; }
		}

		#endregion

		#region XML Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Product, Product.ToString());
			writer.WriteElementString(Schema.NeedUpgrade, NeedUpgrade.ToString());
			EmailTemplateSerialiser.Serialize(writer, EmailTemplates);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			base.ReadElements(reader);
			Product = reader.Reader.IsStartElement(Schema.Product)
				? new ZString(reader.ReadElementString(Schema.Product))
				: ZString.Empty;
			needUpgrade = reader.Reader.IsStartElement(Schema.NeedUpgrade)
				? reader.ReadElementStringAsZBool(Schema.NeedUpgrade)
				: ZBool.False;
			emailTemplates = (IncidentEmailTemplatePair)EmailTemplateSerialiser.Deserialize(reader);
		}

		ZXmlSerializer emailTemplateSerialiser;
		ZXmlSerializer EmailTemplateSerialiser
		{
			get { return emailTemplateSerialiser ?? (emailTemplateSerialiser = ZXmlSerializer.New(typeof(IncidentEmailTemplatePair))); }
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var result = new CodeDescriptionIncidentEmailTemplatePair(DocSourceType, EmailTemplates.LegacyAndERequestV1EmailTemplate, EmailTemplates.ERequestV2EmailTemplate);
			result.Code = Code;
			result.Description = Description;
			result.Product = Product;
			result.NeedUpgrade = NeedUpgrade;
			return result;
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateProduct();
			ValidateNeedUpgrade();
		}
	}
}

