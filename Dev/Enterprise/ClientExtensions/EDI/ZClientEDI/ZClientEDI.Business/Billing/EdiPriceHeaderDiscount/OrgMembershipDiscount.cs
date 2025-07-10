using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Client.EDI.Billing.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class OrgMembershipDiscount : AutoOrgMembershipDiscount
	{
		public static OrgMembershipDiscount NewFromXml(string xml)
		{
			OrgMembershipDiscount result;
			if (string.IsNullOrEmpty(xml))
			{
				result = new OrgMembershipDiscount();
			}
			else
			{
				var serializer = ZXmlSerializer.New(typeof(OrgMembershipDiscount));
				using (var reader = new StringReader(xml))
				{
					result = (OrgMembershipDiscount)serializer.Deserialize(reader);
				}
			}
			return result;
		}

		public OrgMembershipDiscountLineCollection Lines
		{
			get
			{
				if (lines == null)
				{
					lines = new OrgMembershipDiscountLineCollection();
					RegisterEditableChildObject(lines);
				}
				return lines;
			}
		}
		OrgMembershipDiscountLineCollection lines;

		#region XML Serialization

		protected override void WriteLines(XmlWriter writer)
		{
			foreach (OrgMembershipDiscountLine line in Lines)
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

	public class OrgMembershipDiscountLineCollection : NonPersistentBusinessObjectCollection<OrgMembershipDiscountLine>
	{
		public OrgMembershipDiscountLineCollection()
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new OrgMembershipDiscountLine();
		}
	}

	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class OrgMembershipDiscountLine : AutoOrgMembershipDiscountLine
	{
		public OrgMembershipDiscountLine()
		{
		}

		[List("MembershipTypes")]
		public override ZString MembershipType { get => base.MembershipType; set => base.MembershipType = value; }

		public CodeDescriptionBoolCollection MembershipTypes => EDIDataRegistry.Instance.OrgMembershipTypes.Value;

		public override void ValidateMembershipType()
		{
			base.ValidateMembershipType();
			MandatoryValidation.CheckEntered(MembershipTypeInfo);
			ListValidation.ErrorIfInvalidCode(MembershipTypeInfo);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(MembershipTypeInfo);
		}
	}
}

