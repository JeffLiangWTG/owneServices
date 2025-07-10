using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.DataTransfer.DataAdapters.Testing
{
	public class TestValueObjectDataAdapter : ValueObjectDataAdapter<TestImportingBizObj, TestValueObject>
	{
		public override string RootCollectionElementName
		{
			get { return "TestElements"; }
		}

		public override string RootElementName
		{
			get { return "TestElement"; }
		}

		public void SetSchema(XmlSchema schema)
		{
			fSchema = schema;
		}
		public override XmlSchema Schema
		{
			get { return fSchema; }
		}
		XmlSchema fSchema = TestXmlSchemaDefinitions.Instance.SingleTestElementSchema;

		public void SetCollectionSchema(XmlSchema collectionSchema)
		{
			fCollectionSchema = collectionSchema;
		}
		public override XmlSchema CollectionSchema
		{
			get { return fCollectionSchema; }
		}
		XmlSchema fCollectionSchema = TestXmlSchemaDefinitions.Instance.TestElementsSchema;

		protected override TestImportingBizObj FindBusinessObject(TestValueObject value, IValueObjectImportContext context)
		{
			return null;
		}

		protected override void NotifyBizObjCreatedOrUpdated(INotifications notifications, BusinessObject bizObj)
		{
			if (NotifyCreatedOrUpdated)
			{
				base.NotifyBizObjCreatedOrUpdated(notifications, bizObj);
			}
		}

		public bool NotifyCreatedOrUpdated = true;

		public new void ImportFromValueObject(TestImportingBizObj bizObj, TestValueObject value, IValueObjectImportContext context)
		{
			base.ImportFromValueObject(bizObj, value, context);
		}

		protected override void ImportFromValueObjectCore(TestImportingBizObj bizObj, TestValueObject value, IValueObjectImportContext context)
		{
			TestCase.AssertEquals("Should be importing data at this point always", true, ((ISupportDataImporting)bizObj).IsImportingData);
			bizObj.Name = "Import Name";
		}

		protected override void ExportToValueObjectCore(TestImportingBizObj bizObj, TestValueObject constructedValueObject, IValueObjectExportContext context)
		{
			constructedValueObject.Value = "splaty";
		}

		public new void AddImportEvent(TestImportingBizObj bizObj)
		{
			base.AddImportEvent(bizObj);
		}

		public new void AddExportEvent(TestValueObject value, TestImportingBizObj bizObj, IValueObjectExportContext context)
		{
			base.AddExportEvent(value, bizObj, context);
		}

		public new void AddExportEvent(TestValueObject value, TestImportingBizObj bizObj, IValueObjectExportContext context, ZString reference)
		{
			base.AddExportEvent(value, bizObj, context, reference);
		}

		public new void AddImportEvent(TestImportingBizObj bizObj, ZString reference)
		{
			base.AddImportEvent(bizObj, reference);
		}

		protected override EDIInterchange GetNewEDIInterchange(BusinessObjectFactory factory)
		{
			if (EnableEDIInterchange)
			{
				return factory.New<EDIInterchange>();
			}
			else
			{
				return null;
			}
		}

		public ZBool EnableEDIInterchange
		{
			get
			{
				return fEnableEDIInterchange;
			}
			set
			{
				fEnableEDIInterchange = value;
			}
		}

		public EDIInterchange LastPopulatedEDIInterchange
		{
			get
			{
				return fLastPopulatedEDIInterchange;
			}
		}

		protected override void PopulateEDIInterchange(EDIInterchange eDIInterchange, Enterprise.DataTransfer.Xml.XsdVersion1.XmlInterchange xmlInterchange)
		{
			base.PopulateEDIInterchange(eDIInterchange, xmlInterchange);
			fLastPopulatedEDIInterchange = eDIInterchange;
		}

		ZBool fEnableEDIInterchange;
		EDIInterchange fLastPopulatedEDIInterchange;
	}
}
