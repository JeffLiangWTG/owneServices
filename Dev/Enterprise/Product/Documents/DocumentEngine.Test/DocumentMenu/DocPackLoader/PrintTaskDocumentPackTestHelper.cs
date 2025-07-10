using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.DocumentMenu.Testing
{
	sealed class PrintTaskDocumentPackTestHelper
	{
		internal PrintTaskDocumentPackTestHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		internal DocumentCommand CreateDocCommand(string name)
		{
			return CreateDocCommand(name, BusinessContext.Shipment, Core.Constants.DataContext.Shipment, null, 0);
		}

		internal DocumentCommand CreateDocCommand(string name, BusinessContext? businessContext, Core.Constants.DataContext? dataContext, ContactType contactType, short index)
		{
			DocumentCommand result = factory.New<DocumentCommand>();
			result.SU_BusinessContext = businessContext.HasValue ? businessContext.ToString() : string.Empty;
			result.SU_IsPublished = true;
			result.SU_IsSystemDefined = true;
			result.SU_MenuName = name;
			result.SU_MenuIndex = index;
			result.SU_MenuDataContext = dataContext.HasValue ? dataContext.ToString() : string.Empty;
			result.SU_ContactType = (contactType != null) ? contactType.Code : string.Empty;
			result.SU_IsDocPack = false;
			result.Factory.Save();
			return result;
		}

		internal DummyConsolBusinessObject CreateDummyConsol(string code)
		{
			DummyConsolBusinessObject result = factory.New<DummyConsolBusinessObject>();
			result.Z0_Code = code;
			return result;
		}

		internal DummyShipmentBusinessObject CreateDummyShipment(string code, string fkCode, string filterField)
		{
			DummyShipmentBusinessObject result = factory.New<DummyShipmentBusinessObject>();
			result.Z0_Code = code;
			result.Z0_FK_Code = fkCode;
			if (filterField != null)
			{
				DocumentNote note = DocumentNote.LoadNote(result);
				((FilterFieldValueSerialisable)note.UserDefinedFieldList["I'm on alpha shipment"]).ValueAsStringForSerialisation = filterField;
			}
			return result;
		}

		internal StmMenuMenuPivot CreateMenuMenuPivot(DocumentCommand inward, DocumentCommand outward)
		{
			StmMenuMenuPivot result = inward.ChildMenus.AddNew();
			result.SF_SU_Inward = inward.PK;
			result.SF_SU_Outward = outward.PK;
			result.SF_IsSystemDefined = true;
			return result;
		}

		internal StmMenuMenuPivot CreateMenuMenuPivotWithFilter(DocumentCommand inward, DocumentCommand outward, ZString filter)
		{
			var result = CreateMenuMenuPivot(inward, outward);
			result.SF_Filter = filter;
			return result;
		}

		internal StmMenuTemplatePivotBase CreateMenuTemplatePivot(string title, StmTemplateBase template, DocumentCommand command)
		{
			return CreateMenuTemplatePivot(title, template, command, 0);
		}

		internal StmMenuTemplatePivotBase CreateMenuTemplatePivot(string title, StmTemplateBase template, DocumentCommand command, byte index)
		{
			StmMenuTemplatePivotBase result = command.Documents.AddNew();
			result.SI_SO = template.PK;
			result.SI_SU = command.PK;
			result.SI_DocumentTitle = title;
			result.SI_Index = index;
			return result;
		}

		internal StmTemplateBase CreateTemplate(Core.Constants.DataContext dataContext, string name)
		{
			return CreateTemplate(dataContext.ToString(), name);
		}

		internal StmTemplateBase CreateTemplate(string dataContext, string name)
		{
			StmTemplateBase result = new BusinessObjectFactory().New<StmTemplateBase>();
			result.SO_DataContext = dataContext;
			result.SO_IsSystemDefined = true;
			result.SO_Name = name;
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("UDF with tabs3.xls", TestFilesSubFolder.ReportTestFiles);
			result.SO_Template = excelTemplate.GetAsByteArray();
			result.Factory.Save();
			return factory.Load<StmTemplateBase>(result.PK);
		}

		internal PrintTask CreateLoadedPrintTask(DocumentCommand command, UserControlProviderList userFieldList)
		{
			PrintTask result = new PrintTask();
			PrintTaskDocumentPackLoader loader = new PrintTaskDocumentPackLoader(result, command, userFieldList);
			loader.LoadAll();
			return result;
		}
	}
}
