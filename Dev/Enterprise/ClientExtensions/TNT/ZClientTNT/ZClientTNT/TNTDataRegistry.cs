using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ClientSharedComponents.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.TNT
{
	public sealed class TNTDataRegistry : RegistryItemSet
	{
		TNTDataRegistry()
		{
		}

		#region Instance

		public static TNTDataRegistry Instance
		{
			get { return instance ?? (instance = new TNTDataRegistry()); }
		}
		[ThreadStatic]
		static TNTDataRegistry instance;

		#endregion

		public override bool IsForProductivityWise => false;

		#region Declaration CustomsResponse

		public ZString DeclarationCustomsResponseExportDirectory
		{
			get { return new ZString(DeclarationCustomsResponseExportDirectoryRaw.Value); }
		}

		internal StringRegistryItem DeclarationCustomsResponseExportDirectoryRaw
		{
			get
			{
				return GetItem("TNTDeclarationQuantumCustomsResponseDirectory", delegate
				{
					StringRegistryItem result = new StringRegistryItem("TNTDeclarationQuantumCustomsResponseDirectory",
						(NoResString)DeclarationCustomsResponseCategory,
						(NoResString)"Directory for Declaration Customs Response Files",
						null,
						RegistryStorageFlags.Company,
						RegistryOptions.NotCached);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		internal EventsRegistryItem DeclarationEventsForCustomsResponseItem
		{
			get
			{
				return GetItem("TNTDeclarationEventsForQuantumCustomsResponseItem", delegate
				{
					return new EventsRegistryItem("TNTDeclarationEventsForQuantumCustomsResponseItem",
						(NoResString)DeclarationCustomsResponseCategory,
						(NoResString)"Events List",
						(NoResString)"Events on Declaration that trigger an automatic Customs Response to be generated",
						RegistryStorageFlags.Company);
				});
			}
		}

		#endregion

		#region QuantumFile

		#region QuantumFileSourceDirectory

		public ZString QuantumFileSourceDirectory
		{
			get { return new ZString(QuantumFileSourceDirectoryRaw.Value); }
			set { QuantumFileSourceDirectoryRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		internal StringRegistryItem QuantumFileSourceDirectoryRaw
		{
			get
			{
				return GetItem("QuantumFileSourceDirectory", delegate
				{
					StringRegistryItem result = new StringRegistryItem(NewAUStringRegistryItem("QuantumFileSourceDirectory", (NoResString)"Source Directory",
						(NoResString)"Source directory for automatic import of Quantum files from " + TNTConstants.QuantumFileImportSrvTaskCode + " service task", (NoResString)QuantumFileAutoCategory));
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		#endregion

		#region QuantumFileProcessedDirectory

		public ZString QuantumFileProcessedDirectory
		{
			get { return new ZString(QuantumFileProcessedDirectoryRaw.Value); }
			set { QuantumFileProcessedDirectoryRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		internal StringRegistryItem QuantumFileProcessedDirectoryRaw
		{
			get
			{
				return GetItem("QuantumFileProcessedDirectory", delegate
				{
					StringRegistryItem result = new StringRegistryItem(NewAUStringRegistryItem("QuantumFileProcessedDirectory", (NoResString)"Processed Directory",
						(NoResString)"Processed directory for automatic import of Quantum files from " + TNTConstants.QuantumFileImportSrvTaskCode + " service task", (NoResString)QuantumFileAutoCategory));
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		#endregion

		#region QuantumFileSourceDirectoryForManualImport

		public ZString QuantumFileSourceDirectoryForManualImport
		{
			get { return new ZString(QuantumFileSourceDirectoryForManualImportRaw.Value); }
			set { QuantumFileSourceDirectoryForManualImportRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		internal StringRegistryItem QuantumFileSourceDirectoryForManualImportRaw
		{
			get
			{
				return GetItem("QuantumFileSourceDirectoryForManualImport", delegate
				{
					StringRegistryItem result = new StringRegistryItem("QuantumFileSourceDirectoryForManualImport", (NoResString)QuantumFileManualCategory, (NoResString)"Source Directory",
						(NoResString)"Source directory for manual import of Quantum files", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.NotCached, null);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		#endregion

		#region QuantumFileProcessedDirectoryForManualImport

		public ZString QuantumFileProcessedDirectoryForManualImport
		{
			get { return new ZString(QuantumFileProcessedDirectoryForManualImportRaw.Value); }
			set { QuantumFileProcessedDirectoryForManualImportRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		internal StringRegistryItem QuantumFileProcessedDirectoryForManualImportRaw
		{
			get
			{
				return GetItem("QuantumFileProcessedDirectoryForManualImport", delegate
				{
					StringRegistryItem result = new StringRegistryItem("QuantumFileProcessedDirectoryForManualImport", (NoResString)QuantumFileManualCategory, (NoResString)"Processed Directory",
						 (NoResString)"Processed directory for manual import of Quantum files", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.NotCached, null);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		#endregion

		#endregion

		#region NZ specific

		public ZDecimal INDFileImportConsignmentValueThreshold
		{
			get { return new ZDecimal(INDFileImportConsignmentValueThresholdItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)); }
			set { INDFileImportConsignmentValueThresholdItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, (Decimal)value); }
		}

		internal NZRegistryItem INDFileImportConsignmentValueThresholdItem
		=> GetItem("INDFileImportConsignmentValueThreshold", () =>
			new NZRegistryItem(
				"INDFileImportConsignmentValueThreshold",
				(NoResString)QuantumFileManualCategory,
				(NoResString)"Consignment Value Threshold",
				(NoResString)@"If consignment value in NZD cannot be determined or is above the threshold, declaration's entry style will be set to SIM (Simplified).",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				new NumericRegistryEditorInfo(2),
				400.00m));
		#endregion

		#region NAD

		#region NADFileSourceDirectory
		public ZString NADFileSourceDirectory
		{
			get
			{
				ThrowExceptionIfNotInAustralia();
				return new ZString(NADFileSourceDirectoryRaw.Value);
			}
			set
			{
				ThrowExceptionIfNotInAustralia();
				NADFileSourceDirectoryRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString());
			}
		}

		internal StringRegistryItem NADFileSourceDirectoryRaw
		{
			get
			{
				return GetItem("NADFileSourceDirectory", delegate
				{
					StringRegistryItem result = new StringRegistryItem(NewAUStringRegistryItem("NADFileSourceDirectory", "Source Directory for NAD Files", NADFileCategory));
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}
		#endregion

		#region NADFileProcessedDirectory
		public ZString NADFileProcessedDirectory
		{
			get
			{
				ThrowExceptionIfNotInAustralia();
				return new ZString(NADFileProcessedDirectoryRaw.Value);
			}
			set
			{
				ThrowExceptionIfNotInAustralia();
				NADFileProcessedDirectoryRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString());
			}
		}

		internal StringRegistryItem NADFileProcessedDirectoryRaw
		{
			get
			{
				return GetItem("NADFileProcessedDirectory", delegate
				{
					StringRegistryItem result = new StringRegistryItem(NewAUStringRegistryItem("NADFileProcessedDirectory", "Processed Directory for NAD Files", NADFileCategory));
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}
		#endregion

		#region NADFileExtension
		public ZString NADFileExtension
		{
			get
			{
				ThrowExceptionIfNotInAustralia();
				return new ZString(NADFileExtensionRaw.Value);
			}
			set
			{
				ThrowExceptionIfNotInAustralia();
				NADFileExtensionRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString());
			}
		}

		internal StringRegistryItem NADFileExtensionRaw
		{
			get
			{
				return GetItem("NADFileExtension", delegate
				{
					return new StringRegistryItem(NewAUStringRegistryItem("NADFileExtension", "Extension name for NAD interface Files", NADFileCategory));
				});
			}
		}
		#endregion

		#endregion

		#region AirCargo Customs Response

		#region TNTReplyDirectory
		public ZString TNTReplyDirectory
		{
			get
			{
				ThrowExceptionIfNotInAustralia();
				return new ZString(TNTReplyDirectoryRaw.Value);
			}
			set
			{
				ThrowExceptionIfNotInAustralia();
				TNTReplyDirectoryRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString());
			}
		}

		internal StringRegistryItem TNTReplyDirectoryRaw
		{
			get
			{
				return GetItem("TNTReplyDirectory", delegate
				{
					StringRegistryItem result = new StringRegistryItem(NewAUStringRegistryItem("TNTReplyDirectory", "Registry for TNT Reply Directory", AirCargoCustomsResponseCategory));
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}
		#endregion

		#region CustomsStatusCode

		public ReadOnlyCodeDescriptionPairList CustomsStatusCode
		{
			get
			{
				ThrowExceptionIfNotInAustralia();
				return CustomsStatusCodeRaw.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			}
			set
			{
				ThrowExceptionIfNotInAustralia();
				CustomsStatusCodeRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
			}
		}

		internal CodeDescriptionPairListRegistryItem CustomsStatusCodeRaw
		{
			get
			{
				return GetItem("CustomsStatusCode", delegate
				{
					return new CodeDescriptionPairListRegistryItem(new AUCodeDescriptionPairListRegistryItemImpl("CustomsStatusCode", AirCargoCustomsResponseCategory, "Customs Status Code", "Status Code From Customs to Search For", 3), false, new ReadOnlyCodeDescriptionPairList());
				});
			}
		}
		#endregion

		#endregion

		#region Air Cargo Response File Export

		public ZDateTime AirCargoResponseExportHighWaterMark
		{
			get { return new ZDateTime(AirCargoResponseExportHighWaterMarkItem.Value); }
			set { AirCargoResponseExportHighWaterMarkItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, (value.IsValidSmallDateTime) ? value.ToDateTime() : ZDateTime.Empty.ToDateTime()); }
		}

		internal
 DateTimeRegistryItem AirCargoResponseExportHighWaterMarkItem
		{
			get
			{
				return GetItem("TNTAirCargoRepExportHighWaterMark", delegate
				{
					DateTimeRegistryItem result = new DateTimeRegistryItem(NewAURegistryItem("TNTAirCargoRepExportHighWaterMark",
							AirCargoCustomsResponseCategory,
							"High Water Mark",
							"This is the Date & Time which will be used as the start Date & Time from which events will be read for generating the export file",
							RegistryDataTypes.DateTimeType,
							SystemDataRegistry.Instance.SystemLogBatchProcessHighWaterMark.Value));
					result.EditorInfo = new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Long);
					return result;
				});
			}
		}

		#endregion

		#region IQDown

		#region IQ Down Customs Response Export High Water Mark

		public ZDateTime LastEDNReturnTime
		{
			get
			{
				ThrowExceptionIfNotInAustralia();
				return new ZDateTime(LastEDNReturnTimeRaw.Value);
			}
			set
			{
				ThrowExceptionIfNotInAustralia();
				LastEDNReturnTimeRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToDateTime());
			}
		}

		DateTimeRegistryItem LastEDNReturnTimeRaw
		{
			get
			{
				return GetItem("LastEDNReturnTimeRaw", delegate
				{
					DateTimeRegistryItem result = new DateTimeRegistryItem(
							NewAURegistryItem("LastEDNReturnTimeRaw",
							QuantumFileCategory,
							"Last Response Date Time",
							"Last Export Time of Customs Declaration Response Files",
							RegistryDataTypes.DateTimeType,
							new DateTime(1900, 01, 01)
							)
						);
					result.EditorInfo = new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Long);

					return result;
				});
			}
		}

		#endregion

		#region IQDownFileSourceDirectory

		public ZString IQDownFileSourceDirectory
		{
			get
			{
				ThrowExceptionIfNotInAustralia();
				return new ZString(IQDownFileSourceDirectoryRaw.Value);
			}
			set
			{
				ThrowExceptionIfNotInAustralia();
				IQDownFileSourceDirectoryRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString());
			}
		}

		internal StringRegistryItem IQDownFileSourceDirectoryRaw
		{
			get
			{
				return GetItem("IQDownFileSourceDirectory", delegate
				{
					StringRegistryItem result = new StringRegistryItem(NewAUStringRegistryItem("IQDownFileSourceDirectory", "Source Directory for IQDown Files", IQDownFileCategory));
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}
		#endregion

		#region IQDownFileProcessedDirectory

		public ZString IQDownFileProcessedDirectory
		{
			get
			{
				ThrowExceptionIfNotInAustralia();
				return new ZString(IQDownFileProcessedDirectoryRaw.Value);
			}
			set
			{
				ThrowExceptionIfNotInAustralia();
				IQDownFileProcessedDirectoryRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString());
			}
		}
		internal StringRegistryItem IQDownFileProcessedDirectoryRaw
		{
			get
			{
				return GetItem("IQDownFileProcessedDirectory", delegate
				{
					StringRegistryItem result = new StringRegistryItem(NewAUStringRegistryItem("IQDownFileProcessedDirectory", "Processed Directory for IQDown Files", IQDownFileCategory));
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		#endregion

		#endregion

		#region OutTurn

		public ZString OutTurnFileSourceDirectory
		{
			get
			{
				ThrowExceptionIfNotInAustralia();
				return new ZString(OutTurnFileSourceDirectoryRaw.Value);
			}
			set
			{
				ThrowExceptionIfNotInAustralia();
				OutTurnFileSourceDirectoryRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString());
			}
		}

		public ZString OutTurnFileProcessedDirectory
		{
			get
			{
				ThrowExceptionIfNotInAustralia();
				return new ZString(OutTurnFileProcessedDirectoryRaw.Value);
			}
			set
			{
				ThrowExceptionIfNotInAustralia();
				OutTurnFileProcessedDirectoryRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString());
			}
		}

		internal StringRegistryItem OutTurnFileSourceDirectoryRaw
		{
			get
			{
				return GetItem("OutTurn Import Directory", delegate
				{
					StringRegistryItem result = new StringRegistryItem(NewAUStringRegistryItem("OutTurn Import Directory", "Source Directory for OutTurn Files", "Specify Directory to place OutTurn files for Import", OutturnFileCategory));
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		internal StringRegistryItem OutTurnFileProcessedDirectoryRaw
		{
			get
			{
				return GetItem("OutTurnProcessedDirectory", delegate
				{
					StringRegistryItem result = new StringRegistryItem(NewAUStringRegistryItem("OutTurnProcessedDirectory", "Processed Directory for OutTurn Files", "This is the directory that processed OutTurn files will be moved to", OutturnFileCategory));
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		#endregion

		#region XXX File

		#region XXXFileSourceDirectory

		public ZString XXXFileSourceDirectory
		{
			get
			{
				ThrowExceptionIfNotInAustralia();
				return new ZString(XXXFileSourceDirectoryRaw.Value);
			}
			set
			{
				ThrowExceptionIfNotInAustralia();
				XXXFileSourceDirectoryRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString());
			}
		}

		internal StringRegistryItem XXXFileSourceDirectoryRaw
		{
			get
			{
				return GetItem("XXXFileSourceDirectory", delegate
				{
					StringRegistryItem result = new StringRegistryItem(NewAUStringRegistryItem("XXXFileSourceDirectory", "Source Directory for XXX Files", XXXFileCategory));
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		#endregion

		#region XXXFileProcessedDirectory

		public ZString XXXFileProcessedDirectory
		{
			get
			{
				ThrowExceptionIfNotInAustralia();
				return new ZString(XXXFileProcessedDirectoryRaw.Value);
			}
			set
			{
				ThrowExceptionIfNotInAustralia();
				XXXFileProcessedDirectoryRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString());
			}
		}

		internal StringRegistryItem XXXFileProcessedDirectoryRaw
		{
			get
			{
				return GetItem("XXXFileProcessedDirectory", delegate
				{
					StringRegistryItem result = new StringRegistryItem(NewAUStringRegistryItem("XXXFileProcessedDirectory", "Processed Directory for XXX Files", XXXFileCategory));
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		#endregion

		#endregion

		#region Implementation

		#region NZ Registry

		internal class NZRegistryItem : RegistryItemImpl
		{
			public NZRegistryItem(string name, string category, string caption, string hint, RegistryStorageFlags storage, RegistryOptions options, NumericRegistryEditorInfo editorInfo, decimal defaultValue)
				: base((NoResString)name, (NoResString)category, (NoResString)caption, (NoResString)hint, new DecimalRegistryDataType(), editorInfo, storage, options, defaultValue)
			{ }

			public override bool IsVisible(Guid companyPk, Guid branchPk, Guid departmentPk, IGlbDepartment department, IRegistryItemVisibility registryItemVisibility)
			{
				return base.IsVisible(companyPk, branchPk, departmentPk, department, registryItemVisibility) && CurrentCountryIsNewZealand;
			}

			public override IEnumerable<Guid> CountryFilterPKs { get => RegistryItemSet.CountryFilterPKs.NewZealand; set => base.CountryFilterPKs = value; }
		}

		#endregion

		#region AU Registry Items

		class AURegistryItem : RegistryItemImpl
		{
			public AURegistryItem(string name, string category, string caption, string hint, IRegistryDataType dataType, RegistryStorageFlags storage, RegistryOptions options, object defaultValue)
				: base(name, (NoResString)category, (NoResString)caption, (NoResString)hint, dataType, null, storage, options, defaultValue)
			{
			}

			public override bool IsVisible(Guid companyPK, Guid branchPK, Guid departmentPK, IGlbDepartment department, IRegistryItemVisibility registryItemVisibility)
			{
				return (base.IsVisible(companyPK, branchPK, departmentPK, department, registryItemVisibility) && CurrentCountryIsAustralia);
			}

			public override IEnumerable<Guid> CountryFilterPKs
			{
				get { return RegistryItemSet.CountryFilterPKs.Australia; }
				set { base.CountryFilterPKs = value; }
			}
		}

		AURegistryItem NewAUStringRegistryItem(string name, string caption, string category)
		{
			return NewAUStringRegistryItem(name, caption, "", category);
		}

		AURegistryItem NewAUStringRegistryItem(string name, string caption, string hint, string category)
		{
			return NewAURegistryItem(name, category, caption, hint, RegistryDataTypes.StringType, null);
		}

		AURegistryItem NewAURegistryItem(string name, string category, string caption, string hint, IRegistryDataType dataType, object defaultValue)
		{
			return new AURegistryItem(name, category, caption, hint, dataType, RegistryStorageFlags.System, RegistryOptions.NotCached, defaultValue);
		}

		#region AUCodeDescriptionPairList

		class AUCodeDescriptionPairListRegistryItemImpl : RegistryItemImpl
		{
			public AUCodeDescriptionPairListRegistryItemImpl(string name, string category, string caption, string hint, int codeMaxLength)
				: base(name, (NoResString)caption, (NoResString)hint, new CodeDescriptionPairListRegistryDataType(codeMaxLength), new CodeDescriptionPairListEditorInfo(), RegistryStorageFlags.System, RegistryOptions.NotCached, new ReadOnlyCodeDescriptionPairList(), false, (NoResString)category)
			{
			}

			public override IEnumerable<Guid> CountryFilterPKs
			{
				get { return RegistryItemSet.CountryFilterPKs.Australia; }
				set { base.CountryFilterPKs = value; }
			}

			public override bool IsVisible(Guid companyPK, Guid branchPK, Guid departmentPK, IGlbDepartment department, IRegistryItemVisibility registryItemVisibility)
			{
				return (base.IsVisible(companyPK, branchPK, departmentPK, department, registryItemVisibility) && CurrentCountryIsAustralia);
			}
		}

		#endregion

		static bool CurrentCountryIsAustralia
		{
			get { return GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Australia; }
		}

		static bool CurrentCountryIsNewZealand
		{
			get { return GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.NewZealand; }
		}

		#endregion

		void ThrowExceptionIfNotInAustralia()
		{
			if (!CurrentCountryIsAustralia)
			{
				throw new DeveloperNotificationException("Should not use an Australian Registry Item for New Zealand");
			}
		}

		#endregion

		const string Category = "TNT Client Extensions";
		const string QuantumFileCategory = Category + "/" + "Quantum Files";
		const string QuantumFileAutoCategory = QuantumFileCategory + "/Automatic Import";
		const string QuantumFileManualCategory = QuantumFileCategory + "/Manual Import";
		const string NADFileCategory = Category + "/" + "NAD Files";
		const string OutturnFileCategory = Category + "/" + "Outturn Files";
		const string XXXFileCategory = Category + "/" + "XXX Files";
		const string IQDownFileCategory = Category + "/" + "IQDown Files";
		const string AirCargoCustomsResponseCategory = Category + "/" + "Air Cargo Customs Response File";
		const string DeclarationCustomsResponseCategory = Category + "/" + "Declaration Customs Response File";
	}
}
