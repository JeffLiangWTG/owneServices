using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Client.EDI.Billing.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class ProductBundleDiscount : AutoProductBundleDiscount
	{
		public static ProductBundleDiscount NewFromXml(string xml, EdiPriceHeaderDiscount parent = null)
		{
			ProductBundleDiscount result;
			if (string.IsNullOrEmpty(xml))
			{
				result = new ProductBundleDiscount();
			}
			else
			{
				var serializer = ZXmlSerializer.New(typeof(ProductBundleDiscount));
				using (var reader = new StringReader(xml))
				{
					result = (ProductBundleDiscount)serializer.Deserialize(reader);
				}
			}

			result.Parent = parent;
			return result;
		}

		public ProductBundleDiscountLineCollection Lines
		{
			get
			{
				if (lines == null)
				{
					lines = new ProductBundleDiscountLineCollection(this);
					RegisterEditableChildObject(lines);
				}
				return lines;
			}
		}
		ProductBundleDiscountLineCollection lines;

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ClearRowNotifications();
			if (Lines.Count == 0)
			{
				AddRowError("Bundle details required.");
			}
		}

		public EdiPriceHeaderDiscount Parent { get; private set; }

		#region XML Serialization

		protected override void WriteLines(XmlWriter writer)
		{
			foreach (ProductBundleDiscountLine line in Lines)
			{
				if (!line.IsDeleted)
				{
					writer.WriteStartElement("Line");
					((IXmlSerializable)line).WriteXml(writer);
					writer.WriteEndElement();
				}
			}
		}

		protected override void ReadLines(XmlReader reader)
		{
			Lines.RemoveAll();

			if (reader.IsEmptyElement)
			{
				reader.Skip();
			}
			else
			{
				reader.ReadStartElement();

				while (reader.IsStartElement("Line"))
				{
					var line = Lines.AddNew();
					((IXmlSerializable)line).ReadXml(reader);
				}

				reader.ReadEndElement();
			}
		}

		#endregion
	}

	public class ProductBundleDiscountLineCollection : NonPersistentBusinessObjectCollection<ProductBundleDiscountLine>
	{
		public ProductBundleDiscountLineCollection(ProductBundleDiscount master)
		{
			Master = master;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ProductBundleDiscountLine();
		}

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			((ProductBundleDiscountLine)child).Header = Master;
		}

		public ProductBundleDiscount Master { get; private set; }
	}

	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class ProductBundleDiscountLine : AutoProductBundleDiscountLine
	{
		public ProductBundleDiscountLine()
		{
		}

		[List("ProductCodes")]
		public override ZString ProductCode { get => base.ProductCode; set => base.ProductCode = value; }

		public ReadOnlyCodeDescriptionPairList ProductCodes
		{
			get
			{
				var codes = new ProductTypes(true, false);
				codes.RemoveCode(ProductTypes.Codes.Enterprise);
				return codes;
			}
		}

		public override void ValidateProductCode()
		{
			base.ValidateProductCode();
			MandatoryValidation.CheckEntered(ProductCodeInfo);
			ListValidation.ErrorIfInvalidCode(ProductCodeInfo);

			if (!ProductCodeInfo.HasErrors())
			{
				if (ParentCollections.Any(x => x.OfType<ProductBundleDiscountLine>()
									 .Count(y => y.ProductCode.EqualsIgnoringCase(ProductCode)) >= 2))
				{
					ProductCodeInfo.AddError("Duplicate product not allowed.");
				}
			}

			var discountVersion = Header?.Parent?.PHD_Version ?? "";
			if (!discountVersion.IsEmpty && !ProductCodeInfo.HasErrors())
			{
				var query = new ZQuery(ClientLicencePriceHeaderSchema.L6_DiscountCode, discountVersion);
				query.AddToFilter(ClientLicencePriceHeaderSchema.L6_SystemCode, ProductCode);
				var factory = Factory ?? Header?.Parent?.Factory ?? new BusinessObjectFactory();
				if (factory.ExistsInDatabase(ClientLicencePriceHeaderSchema.Constants.TableName, query))
				{
					ProductCodeInfo.AddError("System pricelist product cannot be added in this grid.");
				}
			}
		}

		public ProductBundleDiscount Header { get; set; }
	}
}

