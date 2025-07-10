using System.Xml.Schema;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters
{
	public class ContainerTypeValueObjectDataAdapter : ValueObjectDataAdapter<RefContainer, Xsd.ContainerType>
	{
		public override string RootCollectionElementName
		{
			get { return "ContainerTypes"; }
		}

		public override string RootElementName
		{
			get { return "ContainerType"; }
		}

		public override XmlSchema Schema
		{
			get { return FreightXmlSchemaDefinitions.Instance.SingleContainerType; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return null; }
		}

		protected override void ImportFromValueObjectCore(RefContainer container, Xsd.ContainerType containerTypeValue, IValueObjectImportContext context)
		{
			Xsd.XmlInterchange interchange = context.Interchange as Xsd.XmlInterchange;

			context.SetPropertyInfoValue(container.RC_ISOTypeInfo, containerTypeValue.ISOCode, containerTypeValue.ISOCodeSpecified, Res.GetString("c57ae866-4516-4599-973d-2325501b203d", "Job Declaration Container ISO Type"));
			context.SetPropertyInfoValue(container.RC_CodeInfo, containerTypeValue.ISOCode, containerTypeValue.ISOCodeSpecified, Res.GetString("31a2f6b4-71b8-485f-aee5-950abc964301", "Job Declaration Container Code"));
			context.SetPropertyInfoValue(container.RC_WidthInfo, containerTypeValue.Width.ToString(), containerTypeValue.WidthSpecified, Res.GetString("d1aea245-1a60-44f6-8367-00294be74f58", "Job Declaration Container Width"));
			context.SetPropertyInfoValue(container.RC_LengthInfo, containerTypeValue.Length.ToString(), containerTypeValue.LengthSpecified, Res.GetString("7ad5651e-7713-4848-8668-15e5733e07cb", "Job Declaration Container Length"));
			context.SetPropertyInfoValue(container.RC_HeightInfo, containerTypeValue.Height.ToString(), containerTypeValue.HeightSpecified, Res.GetString("a87aac7d-95cb-48f2-85c2-aad6c4daae7c", "Job Declaration Container Height"));
			context.SetPropertyInfoValue(container.RC_USContainerCodeInfo, containerTypeValue.USContainerCode, containerTypeValue.USContainerCodeSpecified, Res.GetString("9350b308-3fe5-42d7-9a62-8ee26549da24", "Job Declaration Container US Code"));
			container.SetCountrySpecificContainerCode(containerTypeValue.USContainerCode, Core.Constants.CountryCodes.UnitedStates);
		}

		protected override void ExportToValueObjectCore(RefContainer containerType, Xsd.ContainerType result, IValueObjectExportContext context)
		{
			if (!containerType.RC_Code.IsEmpty)
			{
				var usContainerCode = containerType.GetCountrySpecificContainerCode(Enterprise.Core.Constants.CountryCodes.UnitedStates);
				if (usContainerCode.IsEmpty)
				{
					usContainerCode = containerType.RC_USContainerCode;
				}
				result.USContainerCode = usContainerCode;
				result.Length = containerType.RC_Length;
				result.LengthSpecified = true;
				result.Width = containerType.RC_Width;
				result.WidthSpecified = true;
				result.Height = containerType.RC_Height;
				result.HeightSpecified = true;
				result.ISOCode = containerType.RC_ISOType;
				result.ContainerCode = containerType.RC_Code;
			}
		}
	}
}
