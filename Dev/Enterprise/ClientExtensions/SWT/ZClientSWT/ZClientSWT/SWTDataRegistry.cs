using System;
using CargoWise.Types;
using Enterprise.ClientSharedComponents.ServiceTasks;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.SWT
{
	public sealed class SWTDataRegistry : RegistryItemSet
	{
		#region Instance

		public static SWTDataRegistry Instance
		{
			get { return instance ?? (instance = new SWTDataRegistry()); }
		}
		[ThreadStatic]
		static SWTDataRegistry instance;

		#endregion

		public override bool IsForProductivityWise => false;

		#region Not Yet Arrived Report

		#region ConsignorsList

		internal GuidArrayRegistryItem ConsignorsList
		{
			get
			{
				return GetItem("SWTConsignorsList", delegate
				{
					GuidArrayRegistryItem result = new GuidArrayRegistryItem(
						"SWTConsignorsList",
						(NoResString)NotYetArrivedReportCategory,
						(NoResString)"Consignor Organisations",
						(NoResString)"List of Consignors that will receive the 'Not Yet Arrived' report if applicable.",
						RegistryStorageFlags.System);

					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.OrgHeaderCodeListEdit);
					return result;
				});
			}
		}

		#endregion

		#region NotYetArrivedAutomaticSettings

		internal AutomaticProcessRegistryItem NotYetArrivedAutomaticSettings
		{
			get
			{
				return GetItem("NotYetArrivedAutomaticSettings", delegate
					{
						return new AutomaticProcessRegistryItem(
						"NotYetArrivedAutomaticSettings",
						(NoResString)NotYetArrivedReportCategory,
						(NoResString)"Automatic Report Settings",
						(NoResString)"Settings for the batch processor that runs the 'Not Yer Arrived' report",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						ServiceTaskOptionsInitialiser.MinimumIntervalType,
						ServiceTaskOptionsInitialiser.MinimumInterval);
					});
			}
		}

		#endregion

		#region Opening Text

		public ZString NotYetArrivedReportOpeningText
		{
			get
			{
				return NotYetArrivedReportOpeningTextItem.Value;
			}
		}

		internal DocumentOpenCloseTextRegistryItem NotYetArrivedReportOpeningTextItem
		{
			get
			{
				return GetItem("NotYetArrivedReportOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem(
						"NotYetArrivedReportOpeningText",
						(NoResString)NotYetArrivedReportCategory,
						(NoResString)"Opening Text",
						(NoResString)"\"Not Yet Arrived\" Opening Text.",
						RegistryStorageFlags.Company);
				});
			}
		}

		#endregion

		#region Closing Text

		public ZString NotYetArrivedReportClosingText
		{
			get
			{
				return NotYetArrivedReportClosingTextItem.Value;
			}
		}

		internal DocumentOpenCloseTextRegistryItem NotYetArrivedReportClosingTextItem
		{
			get
			{
				return GetItem("NotYetArrivedReportClosingText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem(
						"NotYetArrivedReportClosingText",
						(NoResString)NotYetArrivedReportCategory,
						(NoResString)"Closing Text",
						(NoResString)"\"Not Yet Arrived\" Closing Text."
						, RegistryStorageFlags.Company);
				});
			}
		}
		#endregion
		#endregion

		const string ClientCategory = "SWT Client Extensions";
		const string DocumentsCategory = ClientCategory + @"/Documents";
		const string NotYetArrivedReportCategory = DocumentsCategory + @"/Not Yet Arrived Report";
	}
}
