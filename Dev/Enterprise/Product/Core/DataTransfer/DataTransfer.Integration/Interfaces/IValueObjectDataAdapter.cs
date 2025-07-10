using System;
using System.Collections;
using System.Xml.Schema;

using CargoWise.EntityFramework;

using Enterprise.DataTransfer.Xml;

namespace Enterprise.DataTransfer.Integration
{
	public interface IValueObjectDataAdapter
	{
		Type ValueObjectType { get; }
		Type BusinessObjectType { get; }

		XmlSchema Schema { get; }
		XmlSchema CollectionSchema { get; }
		string RootCollectionElementName { get; }
		string RootElementName { get; }
		string FileName { get; set; }
		bool OnlySaveDataWhenNoRecordsHaveErrorsCheckBoxVisible { get; }
		bool OnlySaveDataWhenNoRecordsHaveErrorsCheckBoxChecked { get; }
		void SetAdditionalRequirementsForDataExportEvent(Func<bool> conditions);

		BusinessObject[] FromXmlInterchange(IBusinessObjectCollection collectionForRelationshipSetup, IValueObjectImportContext context);
		IValueObject ToXmlInterchange(IList bizObjs, IValueObjectExportContext context);

		BusinessObject CreateOrUpdateFromValueObject(IValueObject value, IValueObjectImportContext context);
		void ImportFromValueObject(BusinessObject bizObj, IValueObject value, IValueObjectImportContext context);

		IValueObject ExportToValueObject(BusinessObject bizObj, IValueObjectExportContext context);
		void ExportToValueObject(BusinessObject bizObj, IValueObject constructedValueObject, IValueObjectExportContext context);

		BusinessObject NewBusinessObject(IValueObject value, IValueObjectImportContext context);
		BusinessObject FindBusinessObject(IValueObject value, IValueObjectImportContext context);
	}
}
