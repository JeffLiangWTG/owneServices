using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.DocumentEngine.GUI.SDF;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.SDF;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu
{
	abstract class DocumentCustomisationMenusMaker<T> : CustomisationMenusMaker<T>
		where T : class
	{
		protected readonly IDocumentSupportable documentSupportable;
		readonly UserControlProviderList userFieldList;

		internal DocumentCustomisationMenusMaker(Form parentForm, IDocumentSupportable documentSupportable, UserControlProviderList userFieldList, ZDocumentsMenuItemHelper<T> helper)
			: base(parentForm, documentSupportable.DocumentSupporter.CustomisationSecurityCheckpoint, helper)
		{
			this.documentSupportable = documentSupportable;
			this.userFieldList = userFieldList;

			var parentBO = documentSupportable as DocumentMenuCustomisation;
			isParentDocumentMenuCustomization = parentBO != null;
			isParentsParentDocumentMenuCustomization = parentBO != null && parentBO.Parent is DocumentMenuCustomisation;
		}

		readonly bool isParentDocumentMenuCustomization;
		readonly bool isParentsParentDocumentMenuCustomization;

		protected override void AddCustomiseMenuItem(List<T> menuItems)
		{
			if (!isParentsParentDocumentMenuCustomization)
			{
				base.AddCustomiseMenuItem(menuItems);
			}
		}

		protected override IMenuCustomisationForm GetCustomisationForm()
		{
			var customisation = DocumentMenuCustomisation.New(documentSupportable, userFieldList);
			return new DocumentCustomisationForm(customisation);
		}

		#region Debug-Only Menus
#if DEBUG

		protected override string CustomisationMenusDescription => documentSupportable is ReportMenuCustomisation ? DocumentMenuCaptions.Reports : DocumentMenuCaptions.Documents;

		protected override void AddDebugOnlyMenuItems(List<T> menuItems)
		{
			if (!isParentsParentDocumentMenuCustomization)
			{
				base.AddDebugOnlyMenuItems(menuItems);
			}
		}

		protected override Action GetCustomisationDebugOnlyAction(Action action)
		{
			return () =>
			{
				action();

				if (documentSupportable is MenuCustomisation menuCustomisation)
				{
					SetMenuEditableEditingMode(menuCustomisation);
				}
			};
		}

		protected override void AddAdditionalDebugOnlyMenuItems(List<T> menuItems)
		{
			base.AddAdditionalDebugOnlyMenuItems(menuItems);
			if (!isParentDocumentMenuCustomization)
			{
				var systemDefinedFieldsMenu = MenuItemHelper.GetNewMenuItem("System Defined Fields", DocumentMenuCaptions.SystemDefinedFields, new EventHandler(OnEditSystemDefinedFields));
				menuItems.Add(systemDefinedFieldsMenu);
			}
		}

		void OnEditSystemDefinedFields(object sender, EventArgs e)
		{
			var manager = new StmSystemDefinedFieldManager(documentSupportable);
			ShowCustomisationForm(new StmSystemDefinedFieldForm(manager));
		}
#endif
		#endregion

		#region Document Field Definitions

		protected override void AddSpecificMenus(List<T> menuItems)
		{
			base.AddSpecificMenus(menuItems);

			if (documentSupportable.DocumentSupporter.GetWrapperTypesForDocumentFieldMenu != null)
			{
				menuItems.Add(MenuItemHelper.GetNewMenuItem("DocumentFieldDefinitions", ResString.GetMultilingualString("8336c4d6-954f-4204-a0a0-15200e208f77", "Document Field Definitions"), new EventHandler(OnDocumentFieldHelp)));
			}

			AddDocBuilderSpecificSuspendMenuItems(menuItems);

			AddSetupJobSpecificRecipientsMenu(menuItems);
		}

		void AddSetupJobSpecificRecipientsMenu(List<T> menuItems)
		{
			if (documentSupportable.DocumentSupporter.BusinessObject is ISupportJobDocumentRecipient)
			{
				menuItems.Add(MenuItemHelper.GetSeparator());
				menuItems.Add(MenuItemHelper.GetNewMenuItem("OnSetupJobSpecificRecipients", SetupJobSpecificRecipientsMenuText, new EventHandler(OnSetupJobSpecificRecipients))); // Menu item name, not menu item text, should not be localized
			}
		}

		protected MultilingualString SetupJobSpecificRecipientsMenuText => ResString.GetMultilingualString("6393da4a-685e-4b17-ab82-5bbe2957819c", "Setup Job Specific Recipients");

		void OnSetupJobSpecificRecipients(object sender, EventArgs e)
		{
			var configuration = JobDocumentRecipientConfiguration.New(documentSupportable);
			var configurationForm = new JobDocumentRecipientConfigurationForm(configuration);
			ShowCustomisationForm(configurationForm);
		}

		void OnDocumentFieldHelp(object sender, EventArgs e)
		{
			var fields = GetDocumentFields();
			var bizO = new DocumentFieldDefinitionFormBizo(fields);
			ShowCustomisationForm(new DocumentFieldHelpForm(bizO));
		}

		void AddDocBuilderSpecificSuspendMenuItems(List<T> menuItems)
		{
			if (isParentDocumentMenuCustomization)
			{
				menuItems.Add(GetSuspendDocBuilderCustomizationsMenuItem());
				menuItems.Add(GetSuspendTemplateCachingMenuItem());
			}
		}

		protected abstract T GetSuspendDocBuilderCustomizationsMenuItem();

		protected abstract T GetSuspendTemplateCachingMenuItem();

		DocumentFieldDefinitionCollection GetDocumentFields()
		{
			var fields = new DocumentFieldDefinitionCollection();
			var previousWrappers = new List<string>();
			var attributeFinder = new DocumentFieldAttributeFinder();

			foreach (var wrapperType in documentSupportable.DocumentSupporter.GetWrapperTypesForDocumentFieldMenu)
			{
				if (!previousWrappers.Contains(wrapperType.Name))
				{
					previousWrappers.Add(wrapperType.Name);
					var docFields = attributeFinder.FindProperties(wrapperType);
					fields.AddRange(docFields);
				}
			}

			fields.Sort("FieldName", ListSortDirection.Ascending);
			return fields;
		}

		#endregion
	}
}
