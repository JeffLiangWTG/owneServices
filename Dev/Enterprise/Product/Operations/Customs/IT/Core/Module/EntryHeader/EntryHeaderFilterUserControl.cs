using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IT.Module;

public partial class EntryHeaderFilterUserControl : EU.Module.EntryHeaderFilterUserControl
{
	public new static class Schema
	{
		public const string ControlChannel = "CustomsChannel";
		public const string RegistrationNumber = "RegistrationNumber";
		public const string ReleaseCode = "EntryNumbersProvider+ReleaseInfo+CE_EntryNum";
		public const string ReleaseDate = "CH_EntryReleaseDate";
		public const string ExitDate = "EntryNumbersProvider+IvistoWrapper+Date";
		public const string ExitProcessingDate = "EntryNumbersProvider+Ivisto+CE_SystemCreateTimeUtc";
		public const string IvistoExitOffice = "EntryNumbersProvider+IvistoWrapper+Office";
		public const string ExitStatus = "EntryNumbersProvider+IvistoWrapper+Status";
		public const string ArrivalDate = "EntryNumbersProvider+IrildesWrapper+Date";
		public const string ArrivalOffice = "EntryNumbersProvider+IrildesWrapper+Office";
		public const string ArrivalStatus = "EntryNumbersProvider+IrildesWrapper+Status";
		public const string InstructionElectronicDocumentsUploadRequired = "EntryInstruction+ElectronicDocuments";
		public const string DeclarationCTStatus = "Declaration+ZG_CTStatusID";
	}

	[Obsolete("Use the constructor that takes a collection and/or business object, this constructor is just for the designer")]
	public EntryHeaderFilterUserControl()
	{
		InitializeComponent();
	}

	public EntryHeaderFilterUserControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
		: base(gridCollection, filterBusinessObject)
	{
		InitializeComponent();
	}

	protected override void InitialiseGridCore()
	{
		base.InitialiseGridCore();

		grid.ColumnStyles.AddRange(new Core.Forms.ZGridColumnInfo[]
		{
			new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Enterprise.Customs.IT.Module.Res.GetData("C784B808-EC0E-4BEE-8A6C-15EF426E4383", "Control Channel"),
				ColumnName = Schema.ControlChannel,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			},
			new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Enterprise.Customs.IT.Module.Res.GetData("071D74D2-3539-4618-9BAB-116C63C7DD8B", "Registration Number"),
				ColumnName = Schema.RegistrationNumber,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			},
			new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Enterprise.Customs.IT.Module.Res.GetData("4F1F9DF7-FCDF-489D-B02F-5123FBC7ED76", "Release Code"),
				ColumnName = Schema.ReleaseCode,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			},
			new ZArchitecture.ZDateEditColumnStyleInfo
			{
				CaptionResourceString = Enterprise.Customs.IT.Module.Res.GetData("E7965461-2983-4F37-8CEC-1AA2F5C097AB", "Exit Date"),
				ColumnName = Schema.ExitDate,
				DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			},
			new ZArchitecture.ZDateEditColumnStyleInfo
			{
				CaptionResourceString = Enterprise.Customs.IT.Module.Res.GetData("C757D06D-F24D-4635-A518-18A1FBE0AACB", "Exit Processing Date"),
				ColumnName = Schema.ExitProcessingDate,
				DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110)
			},
			new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Enterprise.Customs.IT.Module.Res.GetData("AB6762DB-9D63-461C-B3F3-B2338C07B1B7", "IVISTO Exit Office"),
				ColumnName = Schema.IvistoExitOffice,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			},
			new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Enterprise.Customs.IT.Module.Res.GetData("0AA3A800-9F62-4FFE-A875-50A62CBB3196", "Exit Status"),
				ColumnName = Schema.ExitStatus,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			},
			new ZArchitecture.ZDateEditColumnStyleInfo
			{
				CaptionResourceString = Enterprise.Customs.IT.Module.Res.GetData("E73963B1-65A1-4FA2-83B1-38CC4FA89DA4", "Arrival Date"),
				ColumnName = Schema.ArrivalDate,
				DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			},
			new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Enterprise.Customs.IT.Module.Res.GetData("F5B89C53-44C1-48D5-BCF5-83D6CA123BBF", "Arrival Office"),
				ColumnName = Schema.ArrivalOffice,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			},
			new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Enterprise.Customs.IT.Module.Res.GetData("9DB3250C-58ED-407A-9B63-ECA3CB40213A", "Arrival Status"),
				ColumnName = Schema.ArrivalStatus,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			},
			new ZArchitecture.ZCheckBoxColumnStyleInfo
			{
				CaptionResourceString = Enterprise.Customs.IT.Module.Res.GetData("18A56EDC-38DE-4A1C-9C39-F359B3215B22", "Instruction Electronic Upload Required"),
				ColumnName = Schema.InstructionElectronicDocumentsUploadRequired,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			},
			new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Enterprise.Customs.IT.Module.Res.GetData("E51A7971-94A6-4141-AC55-A451C70FBC70", "CT Status"),
				ColumnName = Schema.DeclarationCTStatus,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			},
		});
	}
}
