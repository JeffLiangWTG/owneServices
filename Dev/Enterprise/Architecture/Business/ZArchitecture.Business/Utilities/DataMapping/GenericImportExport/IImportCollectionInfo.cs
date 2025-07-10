using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.DataMapping
{
	public interface IImportCollectionInfo
	{
		IBusinessObjectCollection Collection { get; }
		IEnumerable<IImportPropertyInfo> Properties { get; }

		bool ValidateAndSave { get; }
		void OnImportStarted();
		void OnImportCompleted(bool success);
	}

	public interface IImportPropertyInfo
	{
		string HeaderText { get; }
		string MappingName { get; }
		Type PropertyType { get; }
		Type ComponentType { get; }
		int ColumnWidth { get; }
		[SuppressWeaklyTypedCollectionMessage]
		IList GetBindToList(BusinessObject bizObj);
		ModuleIdentifier GetModuleID(BusinessObject bizObj);
		bool IsMultiControl { get; }
		Type GetExpectedTypeForMultiControl(BusinessObject bizObj);
		bool IsReadOnly { get; }
		bool IsMandatory { get; }
		ZCharacterCasing CharacterCasing { get; }
		string FieldTypeColumnName { get; }
	}

	public interface IImportCollectionInfoProvider
	{
		IImportCollectionInfo ImportCollectionInfo { get; }
		string ContextKey { get; }
	}
}
