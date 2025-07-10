using System;
using CargoWise.EntityFramework;

namespace Enterprise.Integration
{
	public interface IZFilterGridModule : IZFilterModule
	{
		BusinessObject[] GetSelectedBusinessObjects();
		bool HasImportNativeXmlMenuItem { get; }
		bool HasExportNativeXmlMenuItem { get; }
		bool AllowAddCopyMenuItem { get; }
		bool AllowTemplateRecords { get; }
		bool AllowLoadTemplateRecords { get; set; }
		bool AllowSendEmailAction { get; }
		bool HasErrors { get; }
		int ExactRowCount { get; }
		ZArchitecture.IModuleFilterCollection ModuleFilters { get; }
		void AddInterfaceConnectorMenuItems();
		void RunPreSaveValidation();
		Type TypeOfTopLevelBusinessObject { get; }
	}
}
