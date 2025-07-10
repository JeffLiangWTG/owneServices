using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Business.Testing
{
	[TestedType(typeof(StmMenuTemplatePivotBase))]
	sealed class StmMenuTemplatePivotBaseTest : EnterpriseBusinessObjectTestCase
	{
		public static string[] GetNonReadOnlyPropertyNames(StmMenuTemplatePivotBase instance)
		{
			if (instance == null)
			{
				throw new ArgumentException("StmMenuTemplatePivotBase cannot be null");
			}

			var nonReadonlyPropertyNames = new List<string>();
			foreach (ZPropertyInfo info in instance.ZPropertyInfoHash)
			{
				if (!info.ReadOnly)
				{
					nonReadonlyPropertyNames.Add(info.Name);
				}
			}

			return nonReadonlyPropertyNames.ToArray();
		}

		StmMenuTemplatePivotBase Pivot
		{
			get { return fPivot ?? (fPivot = (StmMenuTemplatePivotBase)GetNewBusinessObject()); }
		}
		StmMenuTemplatePivotBase fPivot;

		public void TestDelete()
		{
			StmMenuDocumentConfig docConfig1 = Pivot.DocConfigs.AddNew();
			StmMenuDocumentConfig docConfig2 = Pivot.DocConfigs.AddNew();
			Pivot.Delete();
			AssertEquals("docConfig1.IsDeleted", true, docConfig1.IsDeleted);
			AssertEquals("docConfig2.IsDeleted", true, docConfig2.IsDeleted);
		}

		public void TestDocConfigs()
		{
			Pivot.ReadOnly = true;
			AssertEquals("DocConfigs.Master", Pivot, Pivot.DocConfigs.Master);
			AssertEquals("DocConfigs.ReadOnly", false, Pivot.DocConfigs.ReadOnly);
			AssertEquals("DocConfigs should be registered as an editable child object.", true, Pivot.IsRegisteredEditableChildObject(Pivot.DocConfigs));
		}

		public void TestEditingMode()
		{
			AssertEquals("EditingMode", MenuEditingMode.NotAllowEditingOfSystemOrClientMenus, Pivot.EditingMode);

			// User Defined
			Pivot.EditingMode = MenuEditingMode.AllowAll;
			AssertEquals("ReadOnly", false, Pivot.ReadOnly);
			AssertEquals("CanDelete", true, Pivot.CanDelete);
			AssertEquals("SI_IsSystemDefinedInfo.ReadOnly", false, Pivot.SI_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SI_IsClientSpecificInfo.ReadOnly", false, Pivot.SI_IsClientSpecificInfo.ReadOnly);
			AssertEquals("SI_DocumentTitleInfo.ReadOnly", false, Pivot.SI_DocumentTitleInfo.ReadOnly);
			AssertEquals("SI_IndexInfo.ReadOnly", false, Pivot.SI_IndexInfo.ReadOnly);
			AssertEquals("SI_RT_DocTypeInfo.ReadOnly", false, Pivot.SI_RT_DocTypeInfo.ReadOnly);
			AssertEquals("SI_PrintCopyTypeInfo.ReadOnly", false, Pivot.SI_PrintCopyTypeInfo.ReadOnly);
			AssertEquals("SI_MenuTemplateFilterInfo.ReadOnly", false, Pivot.SI_MenuTemplateFilterInfo.ReadOnly);
			AssertEquals("SI_PrintByDefaultInfo.ReadOnly", false, Pivot.SI_PrintByDefaultInfo.ReadOnly);
			AssertEquals("SI_IsPasswordProtectedInfo.ReadOnly", false, Pivot.SI_IsPasswordProtectedInfo.ReadOnly);
			AssertEquals("SI_IsPasswordProtectedForOpeningInfo.ReadOnly", false, Pivot.SI_IsPasswordProtectedForOpeningInfo.ReadOnly);

			Pivot.EditingMode = MenuEditingMode.AllowEditingOfClientSpecificOnly;
			AssertEquals("ReadOnly", false, Pivot.ReadOnly);
			AssertEquals("CanDelete", true, Pivot.CanDelete);
			AssertEquals("SI_IsSystemDefinedInfo.ReadOnly", true, Pivot.SI_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SI_IsClientSpecificInfo.ReadOnly", false, Pivot.SI_IsClientSpecificInfo.ReadOnly);
			AssertEquals("SI_DocumentTitleInfo.ReadOnly", false, Pivot.SI_DocumentTitleInfo.ReadOnly);
			AssertEquals("SI_IndexInfo.ReadOnly", false, Pivot.SI_IndexInfo.ReadOnly);
			AssertEquals("SI_RT_DocTypeInfo.ReadOnly", false, Pivot.SI_RT_DocTypeInfo.ReadOnly);
			AssertEquals("SI_PrintCopyTypeInfo.ReadOnly", false, Pivot.SI_PrintCopyTypeInfo.ReadOnly);
			AssertEquals("SI_MenuTemplateFilterInfo.ReadOnly", false, Pivot.SI_MenuTemplateFilterInfo.ReadOnly);
			AssertEquals("SI_PrintByDefaultInfo.ReadOnly", false, Pivot.SI_PrintByDefaultInfo.ReadOnly);
			AssertEquals("SI_IsPasswordProtectedInfo.ReadOnly", false, Pivot.SI_IsPasswordProtectedInfo.ReadOnly);
			AssertEquals("SI_IsPasswordProtectedForOpeningInfo.ReadOnly", false, Pivot.SI_IsPasswordProtectedForOpeningInfo.ReadOnly);

			Pivot.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
			AssertEquals("ReadOnly", false, Pivot.ReadOnly);
			AssertEquals("CanDelete", true, Pivot.CanDelete);
			AssertEquals("SI_IsSystemDefinedInfo.ReadOnly", false, Pivot.SI_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SI_IsClientSpecificInfo.ReadOnly", true, Pivot.SI_IsClientSpecificInfo.ReadOnly);
			AssertEquals("SI_DocumentTitleInfo.ReadOnly", false, Pivot.SI_DocumentTitleInfo.ReadOnly);
			AssertEquals("SI_IndexInfo.ReadOnly", false, Pivot.SI_IndexInfo.ReadOnly);
			AssertEquals("SI_RT_DocTypeInfo.ReadOnly", false, Pivot.SI_RT_DocTypeInfo.ReadOnly);
			AssertEquals("SI_PrintCopyTypeInfo.ReadOnly", false, Pivot.SI_PrintCopyTypeInfo.ReadOnly);
			AssertEquals("SI_MenuTemplateFilterInfo.ReadOnly", false, Pivot.SI_MenuTemplateFilterInfo.ReadOnly);
			AssertEquals("SI_PrintByDefaultInfo.ReadOnly", false, Pivot.SI_PrintByDefaultInfo.ReadOnly);
			AssertEquals("SI_IsPasswordProtectedInfo.ReadOnly", false, Pivot.SI_IsPasswordProtectedInfo.ReadOnly);
			AssertEquals("SI_IsPasswordProtectedForOpeningInfo.ReadOnly", false, Pivot.SI_IsPasswordProtectedForOpeningInfo.ReadOnly);

			Pivot.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			AssertEquals("ReadOnly", false, Pivot.ReadOnly);
			AssertEquals("CanDelete", true, Pivot.CanDelete);
			AssertEquals("SI_IsSystemDefinedInfo.ReadOnly", true, Pivot.SI_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SI_IsClientSpecificInfo.ReadOnly", true, Pivot.SI_IsClientSpecificInfo.ReadOnly);
			AssertEquals("SI_DocumentTitleInfo.ReadOnly", false, Pivot.SI_DocumentTitleInfo.ReadOnly);
			AssertEquals("SI_IndexInfo.ReadOnly", false, Pivot.SI_IndexInfo.ReadOnly);
			AssertEquals("SI_RT_DocTypeInfo.ReadOnly", false, Pivot.SI_RT_DocTypeInfo.ReadOnly);
			AssertEquals("SI_PrintCopyTypeInfo.ReadOnly", false, Pivot.SI_PrintCopyTypeInfo.ReadOnly);
			AssertEquals("SI_MenuTemplateFilterInfo.ReadOnly", false, Pivot.SI_MenuTemplateFilterInfo.ReadOnly);
			AssertEquals("SI_PrintByDefaultInfo.ReadOnly", false, Pivot.SI_PrintByDefaultInfo.ReadOnly);
			AssertEquals("SI_IsPasswordProtectedInfo.ReadOnly", false, Pivot.SI_IsPasswordProtectedInfo.ReadOnly);
			AssertEquals("SI_IsPasswordProtectedForOpeningInfo.ReadOnly", false, Pivot.SI_IsPasswordProtectedForOpeningInfo.ReadOnly);

			// Client specific
			Pivot.SI_IsClientSpecific = true;
			Pivot.SI_IsSystemDefined = true;

			Pivot.EditingMode = MenuEditingMode.AllowAll;
			AssertEquals("ReadOnly", false, Pivot.ReadOnly);
			AssertEquals("CanDelete", true, Pivot.CanDelete);
			AssertEquals("SI_IsSystemDefinedInfo.ReadOnly", false, Pivot.SI_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SI_IsClientSpecificInfo.ReadOnly", false, Pivot.SI_IsClientSpecificInfo.ReadOnly);
			AssertEquals("SI_DocumentTitleInfo.ReadOnly", false, Pivot.SI_DocumentTitleInfo.ReadOnly);
			AssertEquals("SI_IndexInfo.ReadOnly", false, Pivot.SI_IndexInfo.ReadOnly);
			AssertEquals("SI_RT_DocTypeInfo.ReadOnly", false, Pivot.SI_RT_DocTypeInfo.ReadOnly);
			AssertEquals("SI_PrintCopyTypeInfo.ReadOnly", false, Pivot.SI_PrintCopyTypeInfo.ReadOnly);
			AssertEquals("SI_MenuTemplateFilterInfo.ReadOnly", false, Pivot.SI_MenuTemplateFilterInfo.ReadOnly);
			AssertEquals("SI_PrintByDefaultInfo.ReadOnly", false, Pivot.SI_PrintByDefaultInfo.ReadOnly);
			AssertEquals("SI_IsPasswordProtectedInfo.ReadOnly", false, Pivot.SI_IsPasswordProtectedInfo.ReadOnly);
			AssertEquals("SI_IsPasswordProtectedForOpeningInfo.ReadOnly", false, Pivot.SI_IsPasswordProtectedForOpeningInfo.ReadOnly);

			Pivot.EditingMode = MenuEditingMode.AllowEditingOfClientSpecificOnly;
			AssertEquals("ReadOnly", false, Pivot.ReadOnly);
			AssertEquals("CanDelete", true, Pivot.CanDelete);
			AssertEquals("SI_IsSystemDefinedInfo.ReadOnly", true, Pivot.SI_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SI_IsClientSpecificInfo.ReadOnly", false, Pivot.SI_IsClientSpecificInfo.ReadOnly);
			AssertEquals("SI_DocumentTitleInfo.ReadOnly", false, Pivot.SI_DocumentTitleInfo.ReadOnly);
			AssertEquals("SI_IndexInfo.ReadOnly", false, Pivot.SI_IndexInfo.ReadOnly);
			AssertEquals("SI_RT_DocTypeInfo.ReadOnly", false, Pivot.SI_RT_DocTypeInfo.ReadOnly);
			AssertEquals("SI_PrintCopyTypeInfo.ReadOnly", false, Pivot.SI_PrintCopyTypeInfo.ReadOnly);
			AssertEquals("SI_MenuTemplateFilterInfo.ReadOnly", false, Pivot.SI_MenuTemplateFilterInfo.ReadOnly);
			AssertEquals("SI_PrintByDefaultInfo.ReadOnly", false, Pivot.SI_PrintByDefaultInfo.ReadOnly);
			AssertEquals("SI_IsPasswordProtectedInfo.ReadOnly", false, Pivot.SI_IsPasswordProtectedInfo.ReadOnly);
			AssertEquals("SI_IsPasswordProtectedForOpeningInfo.ReadOnly", false, Pivot.SI_IsPasswordProtectedForOpeningInfo.ReadOnly);

			Pivot.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
			AssertEquals("ReadOnly", false, Pivot.ReadOnly);
			AssertEquals("CanDelete", false, Pivot.CanDelete);
			AssertEquals("SI_IsSystemDefinedInfo.ReadOnly", true, Pivot.SI_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SI_IsClientSpecificInfo.ReadOnly", true, Pivot.SI_IsClientSpecificInfo.ReadOnly);
			AssertEquals("SI_DocumentTitleInfo.ReadOnly", true, Pivot.SI_DocumentTitleInfo.ReadOnly);
			AssertEquals("SI_IndexInfo.ReadOnly", true, Pivot.SI_IndexInfo.ReadOnly);
			AssertEquals("SI_RT_DocTypeInfo.ReadOnly", true, Pivot.SI_RT_DocTypeInfo.ReadOnly);
			AssertEquals("SI_PrintCopyTypeInfo.ReadOnly", true, Pivot.SI_PrintCopyTypeInfo.ReadOnly);
			AssertEquals("SI_MenuTemplateFilterInfo.ReadOnly", true, Pivot.SI_MenuTemplateFilterInfo.ReadOnly);
			AssertEquals("SI_PrintByDefaultInfo.ReadOnly", false, Pivot.SI_PrintByDefaultInfo.ReadOnly);
			AssertEquals("SI_IsPasswordProtectedInfo.ReadOnly", false, Pivot.SI_IsPasswordProtectedInfo.ReadOnly);
			AssertEquals("SI_IsPasswordProtectedForOpeningInfo.ReadOnly", false, Pivot.SI_IsPasswordProtectedForOpeningInfo.ReadOnly);

			Pivot.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			AssertEquals("ReadOnly", false, Pivot.ReadOnly);
			AssertEquals("CanDelete", false, Pivot.CanDelete);
			AssertEquals("SI_IsSystemDefinedInfo.ReadOnly", true, Pivot.SI_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SI_IsClientSpecificInfo.ReadOnly", true, Pivot.SI_IsClientSpecificInfo.ReadOnly);
			AssertEquals("SI_DocumentTitleInfo.ReadOnly", true, Pivot.SI_DocumentTitleInfo.ReadOnly);
			AssertEquals("SI_IndexInfo.ReadOnly", true, Pivot.SI_IndexInfo.ReadOnly);
			AssertEquals("SI_RT_DocTypeInfo.ReadOnly", true, Pivot.SI_RT_DocTypeInfo.ReadOnly);
			AssertEquals("SI_PrintCopyTypeInfo.ReadOnly", true, Pivot.SI_PrintCopyTypeInfo.ReadOnly);
			AssertEquals("SI_MenuTemplateFilterInfo.ReadOnly", true, Pivot.SI_MenuTemplateFilterInfo.ReadOnly);
			AssertEquals("SI_PrintByDefaultInfo.ReadOnly", false, Pivot.SI_PrintByDefaultInfo.ReadOnly);
			AssertEquals("SI_IsPasswordProtectedInfo.ReadOnly", false, Pivot.SI_IsPasswordProtectedInfo.ReadOnly);
			AssertEquals("SI_IsPasswordProtectedForOpeningInfo.ReadOnly", false, Pivot.SI_IsPasswordProtectedForOpeningInfo.ReadOnly);

			// System Defined
			Pivot.SI_IsClientSpecific = false;
			Pivot.SI_IsSystemDefined = true;

			Pivot.EditingMode = MenuEditingMode.AllowAll;
			AssertEquals("ReadOnly", false, Pivot.ReadOnly);
			AssertEquals("CanDelete", true, Pivot.CanDelete);
			AssertEquals("SI_IsSystemDefinedInfo.ReadOnly", false, Pivot.SI_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SI_IsClientSpecificInfo.ReadOnly", false, Pivot.SI_IsClientSpecificInfo.ReadOnly);
			AssertEquals("SI_DocumentTitleInfo.ReadOnly", false, Pivot.SI_DocumentTitleInfo.ReadOnly);
			AssertEquals("SI_IndexInfo.ReadOnly", false, Pivot.SI_IndexInfo.ReadOnly);
			AssertEquals("SI_RT_DocTypeInfo.ReadOnly", false, Pivot.SI_RT_DocTypeInfo.ReadOnly);
			AssertEquals("SI_PrintCopyTypeInfo.ReadOnly", false, Pivot.SI_PrintCopyTypeInfo.ReadOnly);
			AssertEquals("SI_MenuTemplateFilterInfo.ReadOnly", false, Pivot.SI_MenuTemplateFilterInfo.ReadOnly);
			AssertEquals("SI_PrintByDefaultInfo.ReadOnly", false, Pivot.SI_PrintByDefaultInfo.ReadOnly);
			AssertEquals("SI_IsPasswordProtectedInfo.ReadOnly", false, Pivot.SI_IsPasswordProtectedInfo.ReadOnly);
			AssertEquals("SI_IsPasswordProtectedForOpeningInfo.ReadOnly", false, Pivot.SI_IsPasswordProtectedForOpeningInfo.ReadOnly);

			Pivot.EditingMode = MenuEditingMode.AllowEditingOfClientSpecificOnly;
			AssertEquals("ReadOnly", false, Pivot.ReadOnly);
			AssertEquals("CanDelete", false, Pivot.CanDelete);
			AssertEquals("SI_IsSystemDefinedInfo.ReadOnly", true, Pivot.SI_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SI_IsClientSpecificInfo.ReadOnly", true, Pivot.SI_IsClientSpecificInfo.ReadOnly);
			AssertEquals("SI_DocumentTitleInfo.ReadOnly", true, Pivot.SI_DocumentTitleInfo.ReadOnly);
			AssertEquals("SI_IndexInfo.ReadOnly", true, Pivot.SI_IndexInfo.ReadOnly);
			AssertEquals("SI_RT_DocTypeInfo.ReadOnly", true, Pivot.SI_RT_DocTypeInfo.ReadOnly);
			AssertEquals("SI_PrintCopyTypeInfo.ReadOnly", true, Pivot.SI_PrintCopyTypeInfo.ReadOnly);
			AssertEquals("SI_MenuTemplateFilterInfo.ReadOnly", true, Pivot.SI_MenuTemplateFilterInfo.ReadOnly);
			AssertEquals("SI_PrintByDefaultInfo.ReadOnly", false, Pivot.SI_PrintByDefaultInfo.ReadOnly);
			AssertEquals("SI_IsPasswordProtectedInfo.ReadOnly", false, Pivot.SI_IsPasswordProtectedInfo.ReadOnly);
			AssertEquals("SI_IsPasswordProtectedForOpeningInfo.ReadOnly", false, Pivot.SI_IsPasswordProtectedForOpeningInfo.ReadOnly);

			Pivot.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
			AssertEquals("ReadOnly", false, Pivot.ReadOnly);
			AssertEquals("CanDelete", true, Pivot.CanDelete);
			AssertEquals("SI_IsSystemDefinedInfo.ReadOnly", false, Pivot.SI_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SI_IsClientSpecificInfo.ReadOnly", true, Pivot.SI_IsClientSpecificInfo.ReadOnly);
			AssertEquals("SI_DocumentTitleInfo.ReadOnly", false, Pivot.SI_DocumentTitleInfo.ReadOnly);
			AssertEquals("SI_IndexInfo.ReadOnly", false, Pivot.SI_IndexInfo.ReadOnly);
			AssertEquals("SI_RT_DocTypeInfo.ReadOnly", false, Pivot.SI_RT_DocTypeInfo.ReadOnly);
			AssertEquals("SI_PrintCopyTypeInfo.ReadOnly", false, Pivot.SI_PrintCopyTypeInfo.ReadOnly);
			AssertEquals("SI_MenuTemplateFilterInfo.ReadOnly", false, Pivot.SI_MenuTemplateFilterInfo.ReadOnly);
			AssertEquals("SI_PrintByDefaultInfo.ReadOnly", false, Pivot.SI_PrintByDefaultInfo.ReadOnly);
			AssertEquals("SI_IsPasswordProtectedInfo.ReadOnly", false, Pivot.SI_IsPasswordProtectedInfo.ReadOnly);
			AssertEquals("SI_IsPasswordProtectedForOpeningInfo.ReadOnly", false, Pivot.SI_IsPasswordProtectedForOpeningInfo.ReadOnly);

			Pivot.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			AssertEquals("ReadOnly", false, Pivot.ReadOnly);
			AssertEquals("CanDelete", false, Pivot.CanDelete);
			AssertEquals("SI_IsSystemDefinedInfo.ReadOnly", true, Pivot.SI_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SI_IsClientSpecificInfo.ReadOnly", true, Pivot.SI_IsClientSpecificInfo.ReadOnly);
			AssertEquals("SI_DocumentTitleInfo.ReadOnly", true, Pivot.SI_DocumentTitleInfo.ReadOnly);
			AssertEquals("SI_IndexInfo.ReadOnly", true, Pivot.SI_IndexInfo.ReadOnly);
			AssertEquals("SI_RT_DocTypeInfo.ReadOnly", true, Pivot.SI_RT_DocTypeInfo.ReadOnly);
			AssertEquals("SI_PrintCopyTypeInfo.ReadOnly", true, Pivot.SI_PrintCopyTypeInfo.ReadOnly);
			AssertEquals("SI_MenuTemplateFilterInfo.ReadOnly", true, Pivot.SI_MenuTemplateFilterInfo.ReadOnly);
			AssertEquals("SI_PrintByDefaultInfo.ReadOnly", false, Pivot.SI_PrintByDefaultInfo.ReadOnly);
			AssertEquals("SI_IsPasswordProtectedInfo.ReadOnly", false, Pivot.SI_IsPasswordProtectedInfo.ReadOnly);
			AssertEquals("SI_IsPasswordProtectedForOpeningInfo.ReadOnly", false, Pivot.SI_IsPasswordProtectedForOpeningInfo.ReadOnly);
		}

		public void TestSetIsClientSpecific()
		{
			Pivot.SI_IsSystemDefined = true;
			AssertEquals("SI_IsSystemDefined", true, Pivot.SI_IsSystemDefined);
			AssertEquals("SI_IsClientSpecific", false, Pivot.SI_IsClientSpecific);

			Pivot.SI_IsSystemDefined = false;
			AssertEquals("SI_IsSystemDefined", false, Pivot.SI_IsSystemDefined);
			AssertEquals("SI_IsClientSpecific", false, Pivot.SI_IsClientSpecific);

			Pivot.SI_IsClientSpecific = true;
			AssertEquals("SI_IsSystemDefined", true, Pivot.SI_IsSystemDefined);
			AssertEquals("SI_IsClientSpecific", true, Pivot.SI_IsClientSpecific);

			Pivot.SI_IsClientSpecific = false;
			AssertEquals("SI_IsSystemDefined", false, Pivot.SI_IsSystemDefined);
			AssertEquals("SI_IsClientSpecific", false, Pivot.SI_IsClientSpecific);

			Pivot.SI_IsClientSpecific = true;
			AssertEquals("SI_IsSystemDefined", true, Pivot.SI_IsSystemDefined);
			AssertEquals("SI_IsClientSpecific", true, Pivot.SI_IsClientSpecific);

			Pivot.SI_IsSystemDefined = false;
			AssertEquals("SI_IsSystemDefined", false, Pivot.SI_IsSystemDefined);
			AssertEquals("SI_IsClientSpecific", false, Pivot.SI_IsClientSpecific);

			Pivot.SI_IsSystemDefined = true;
			AssertEquals("SI_IsSystemDefined", true, Pivot.SI_IsSystemDefined);
			AssertEquals("SI_IsClientSpecific", false, Pivot.SI_IsClientSpecific);

			Pivot.SI_IsClientSpecific = true;
			AssertEquals("SI_IsSystemDefined", true, Pivot.SI_IsSystemDefined);
			AssertEquals("SI_IsClientSpecific", true, Pivot.SI_IsClientSpecific);
		}

		public void TestDocTypeList()
		{
			RefDocTypeCollection docTypeCollection = new RefDocTypeCollection(Factory);
			AssertEquals("No DocManagerFilter - Pivot DocType collection has same number of elements as the DocCollection", docTypeCollection.Count, Pivot.DocTypeList.Count);

			RefDocTypeFindboxCollection filteredDocTypeCollection = new RefDocTypeFindboxCollection(Factory, "SHP");

			Pivot.DocManagerFilter = "SHP";
			AssertEquals("DocManagerFilter is SHP. Pivot DocType collection should be same count as SHP filtered DocCollection", filteredDocTypeCollection.Count, Pivot.DocTypeList.Count);

			RefDocType hiddenDocType = Factory.New<RefDocType>();
			hiddenDocType.RT_ReferenceType = "XXX";
			AssertCollectionNotContains(hiddenDocType, Pivot.DocTypeList);

			RefDocType allDocType = Factory.New<RefDocType>();
			allDocType.RT_ReferenceType = "ALL";
			AssertCollectionContains(allDocType, Pivot.DocTypeList);
		}

		public void TestIStmMenuDocumentConfigSourceMembers()
		{
			IStmMenuDocumentConfigSource docConfigSource = Pivot;
			AssertEquals("MenuTemplatePivot", Pivot, docConfigSource.MenuTemplatePivot);

			StmMenuDocumentConfig docConfig = docConfigSource.GetPersistentDocConfig();
			AssertEquals("DocConfigs.Count", 1, Pivot.DocConfigs.Count);
			AssertEquals("GetPersistentDocConfig()", Pivot.DocConfigs[0], docConfig);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			TemporaryStmMenuDocumentConfig tempDocConfig = docConfigSource.GetTemporaryDocConfig(newFactory);
			AssertEquals("GetTemporaryDocConfig().Factory", newFactory, tempDocConfig.Factory);
			AssertEquals("GetTemporaryDocConfig().ConfigItems.Count", 0, tempDocConfig.ConfigItems.Count);
		}
	}
}
