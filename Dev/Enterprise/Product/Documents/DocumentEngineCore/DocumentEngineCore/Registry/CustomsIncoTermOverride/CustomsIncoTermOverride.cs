using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	[XmlSerializerAssembly("Enterprise.DocumentEngineCore.XmlSerializers")]
	public class CustomsIncoTermOverride : AutoCustomsIncoTermOverride
	{
		[List("InternationalIncoTermList")]
		public override ZString InternationalCode
		{
			get { return base.InternationalCode; }
			set { base.InternationalCode = value; }
		}

		public override void ValidateCustomsCode()
		{
			base.ValidateCustomsCode();
			MandatoryValidation.CheckEntered(CustomsCodeInfo);
			if (ParentCollections.Count > 0)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(CustomsCodeInfo);
			}
		}

		public override void ValidateInternationalCode()
		{
			base.ValidateInternationalCode();
			IncotermValidation.WarningIfExpired(InternationalCodeInfo);
			MandatoryValidation.CheckEntered(InternationalCodeInfo);
			ListValidation.ErrorIfInvalidCode(InternationalCodeInfo);
		}

		public CodeDescriptionPairList InternationalIncoTermList
		{
			get { return new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms); }
		}

		IIncotermValidation IncotermValidation
		{
			get
			{
				if (incoTermValidation == null)
				{
					incoTermValidation = ObjectFactory.Get<IIncotermValidation>();
				}
				return incoTermValidation;
			}
		}
		IIncotermValidation incoTermValidation;

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CustomsIncoTermOverride();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			CustomsIncoTermOverride incoTerm = (CustomsIncoTermOverride)clone;
			incoTerm.CustomsCode = CustomsCode;
			incoTerm.InternationalCode = InternationalCode;
		}

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.CustomsCode, CustomsCode);
			writer.WriteElementString(Schema.InternationalCode, InternationalCode);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			base.ReadElements(reader);
			CustomsCode = reader.ReadElementString(Schema.CustomsCode);
			InternationalCode = reader.ReadElementString(Schema.InternationalCode);
		}

		#endregion
	}
}
