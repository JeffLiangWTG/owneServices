using System.Collections.Generic;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Module.Testing;

sealed class EntryHeaderFilterUserControlTest : EU.Module.Testing.EntryHeaderFilterUserControlTest
{
	protected override List<string> FilteredGridColumns
	{
		get
		{
			var list = new List<string>()
			{
				EntryHeaderFilterUserControl.Schema.ControlChannel,
				EntryHeaderFilterUserControl.Schema.RegistrationNumber,
				EntryHeaderFilterUserControl.Schema.ReleaseCode,
				EntryHeaderFilterUserControl.Schema.ExitDate,
				EntryHeaderFilterUserControl.Schema.ExitProcessingDate,
				EntryHeaderFilterUserControl.Schema.IvistoExitOffice,
				EntryHeaderFilterUserControl.Schema.ExitStatus,
				EntryHeaderFilterUserControl.Schema.ArrivalDate,
				EntryHeaderFilterUserControl.Schema.ArrivalOffice,
				EntryHeaderFilterUserControl.Schema.ArrivalStatus,
				EntryHeaderFilterUserControl.Schema.InstructionElectronicDocumentsUploadRequired,
				EntryHeaderFilterUserControl.Schema.DeclarationCTStatus,
			};
			list.AddRange(base.FilteredGridColumns);
			return list;
		}
	}

	protected override List<string> ColumnNamesInSortOrder
	{
		get
		{
			var list = base.ColumnNamesInSortOrder;
			list.AddRange(new List<string>()
			{
				EntryHeaderFilterUserControl.Schema.ControlChannel,
				EntryHeaderFilterUserControl.Schema.RegistrationNumber,
				EntryHeaderFilterUserControl.Schema.ReleaseCode,
				EntryHeaderFilterUserControl.Schema.ExitDate,
				EntryHeaderFilterUserControl.Schema.ExitProcessingDate,
				EntryHeaderFilterUserControl.Schema.IvistoExitOffice,
				EntryHeaderFilterUserControl.Schema.ExitStatus,
				EntryHeaderFilterUserControl.Schema.ArrivalDate,
				EntryHeaderFilterUserControl.Schema.ArrivalOffice,
				EntryHeaderFilterUserControl.Schema.ArrivalStatus,
				EntryHeaderFilterUserControl.Schema.InstructionElectronicDocumentsUploadRequired,
				EntryHeaderFilterUserControl.Schema.DeclarationCTStatus,
			});
			return list;
		}
	}

	protected override Customs.Module.EntryHeaderFilterUserControl GetNewEntryHeaderFilterUserControl()
	{
		var declaration = Factory.New<JobDeclaration>();
		var cusEntryHeaders = new CusEntryHeaderCollection(declaration, Factory);
		var filterBusinessObject = new EntryHeaderFilterBusinessObject();
		return new EntryHeaderFilterUserControl(cusEntryHeaders, filterBusinessObject);
	}
}
