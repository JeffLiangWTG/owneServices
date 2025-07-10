using System;
using System.Xml.Schema;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// Base Class for Air/Sea Data Adapters
	/// </summary>
	public abstract class BaseCargoXmlDataAdapter<TBusinessObject, TValueObject> : ValueObjectDataAdapter<TBusinessObject, TValueObject>
		where TBusinessObject : BusinessObject
		where TValueObject : Xsd.Consol
	{
		#region Overrides Abstract Members

		public override string RootCollectionElementName
		{
			get { return "Consols"; }
		}

		public override string RootElementName
		{
			get { return "Consol"; }
		}

		public override XmlSchema Schema
		{
			get { return FreightXmlSchemaDefinitions.Instance.SingleConsolSchema; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return FreightXmlSchemaDefinitions.Instance.ConsolsSchema; }
		}

		protected override void ExportToValueObjectCore(TBusinessObject bizObj, TValueObject constructedValueObject, IValueObjectExportContext context)
		{
			throw new NotSupportedException();
		}

		#endregion

		#region PopulateAddress2SuburbDetails

		internal void PopulateAddress2SuburbDetails(ZPropertyInfo address2Property, ZPropertyInfo citySuburbProperty, ZString addressLine2, ZString cityOrSuburb, IValueObjectImportContext context)
		{
			int citySuburbLength = cityOrSuburb.Length;
			int address2Length = addressLine2.Length;
			int maxCitySubLength = citySuburbProperty.MaxLength;
			int maxAddressLine2Length = address2Property.MaxLength;
			if ((citySuburbLength > maxCitySubLength) && ((address2Length + citySuburbLength) <= (maxCitySubLength + maxAddressLine2Length - 1)))
			{
				int index = citySuburbLength - maxCitySubLength - 1;
				ZString token1 = "";
				ZString token2 = cityOrSuburb.Substring(index);
				int firstWhiteSpace = token2.IndexOf(" ");
				if (firstWhiteSpace == -1 || (index + firstWhiteSpace) > (maxAddressLine2Length - address2Length))
				{
					token2 = cityOrSuburb.Substring(index + 1);
					token1 = cityOrSuburb.Substring(0, index + 1);
				}
				else
				{
					token2 = token2.Substring(firstWhiteSpace + 1);
					token1 = cityOrSuburb.Substring(0, cityOrSuburb.Length - token2.Length - 1);
				}
				addressLine2 = addressLine2 + " " + token1;
				cityOrSuburb = token2;
			}
			context.SetPropertyInfoValueIfValueNotEmpty(address2Property, addressLine2);
			context.SetPropertyInfoValueIfValueNotEmpty(citySuburbProperty, cityOrSuburb);
		}
		#endregion

		#region Notes

		protected void PopulateNotes(Notes notes, Xsd.NotesNoteCollection notesValue, IValueObjectImportContext context)
		{
			NoteValueObjectDataAdapter adapter = new NoteValueObjectDataAdapter();
			adapter.ImportNotesAndAttachToBusinessObjectNotes(notes, notesValue, context);
		}

		#endregion
	}
}
