using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentEngine
{
	public interface IDeliverable : IDisposable, IDocument
	{
		/// <summary>
		/// The event to log to the parent BusinessObject when the deliverable is delivered
		/// </summary>
		ZString DocumentDeliveredEventCode { get; }

		/// <summary>
		/// The event to log to the parent BusinessObject for report password information
		/// </summary>
		ZString DocumentPasswordInformationEventCode { get; }

		/// <summary>
		/// File Extension type required when saving this object to a temp file
		/// </summary>
		ZString FileExtension { get; }

		/// <summary>
		/// A description of this Deliverable object e.g. Report Name/title
		/// </summary>
		ZString Name { get; }

		ZPropertyInfo NameInfo { get; }

		ZString NameForBinding { get; }

		/// <summary>
		/// Saves the contents of this object to a temp filename
		/// </summary>
		/// <param name="DeliveryContact">Person to deliver to</param>
		/// <param name="Filename">Filename to save contents of this report/eDoc to</param>
		void Save(DocDeliveryContact deliveryContact, DocDeliveryContact mostOfficialContact, Stream fileContent);

		/// <summary>
		/// Returns DeliveryInfo object to add to the Document Pack
		/// </summary>
		DeliveryInfo GetDeliveryInfo(bool isDraft);

		/// <summary>
		/// Returns the Delivery Mode (Print, Email, Fax etc)
		/// </summary>
		ZString DeliveryMode { get; }

		ZPropertyInfo DeliveryModeInfo { get; }

		ZString AllAvailableDeliveryModes { get; }

		ZString DocumentTypeCode { get; }

		ZPropertyInfo DocumentTypeCodeInfo { get; }

		ZString DocumentTypeDescription { get; }

		ZPropertyInfo DocumentTypeDescriptionInfo { get; }

		DocDeliveryPrintDetails PrinterDetails { get; }

		ZGuid DeliveryGroupID { get; set; }

		ZByte Index { get; set; }

		/// <summary>
		/// Should process this item?
		/// </summary>
		ZBool IncludedInPrint { get; set; }

		ZPropertyInfo IncludedInPrintInfo { get; }

		bool IncludedInPrint_ReadOnly { get; set; }

		bool IsDeliveredByEmail { get; set; }

		/// <summary>
		/// Set the menu item for this individual ideliverable
		/// </summary>
		StmMenuItem MenuItem { get; set; }

		ZBool CoverSheetRequired { get; }

		bool ContainsDataRows { get; }

		ZGuid SourcePivotPK { get; set; }

		ZGuid MenuTemplatePivotPK { get; }

		bool ShouldPrintByDefault { get; set; }

		ZString JobNumber { get; }

		ZString Identifier { get; }

		ZGuid IdentifiablePK { get; }

		#region Testing Only

#if DEBUG
		/// <summary>
		/// This is a test attribute. Unfortunately it is on the interface because I don't have access to
		/// StorageDocs business objects (which are built after this project)
		/// </summary>
		void DeleteTempFilesForTesting();

		/// <summary>
		/// This is a test attribute. Unfortunately it is on the interface because I don't have access to
		/// StorageDocs business objects (which are built after this project)
		/// </summary>
		int RunCountForTesting { get; }
#endif

		#endregion
	}

	static class IDeliverableExtensions
	{
		public static IEnumerable<(BusinessObject bizo, EventValue evnt)> GetEventsToDeliver(this IDeliverable deliverable, BusinessObject parentBusinessObject, IStmMenuItem menuItem, params KeyValuePair<string, string>[] parameters)
		{
			Argument.NotNull(deliverable, nameof(deliverable));
			Argument.NotNull(menuItem, nameof(menuItem));

			if (parentBusinessObject != null)
			{
				if (!string.IsNullOrEmpty(deliverable.DocumentDeliveredEventCode))
				{
					var referenceFreeText = ((ZString)(menuItem.SU_MenuName + "/" + deliverable.Name)).Left(StmALog.Schema.SL_ReferenceMaxLength);
					var reference = StmALog.GenerateEventReferenceToFitInReferenceMaxLength(referenceFreeText, parameters);
					yield return (parentBusinessObject, new EventValue(Events.All[deliverable.DocumentDeliveredEventCode], reference: reference));
				}

				if (deliverable is Report report && !string.IsNullOrEmpty(report.DocumentPasswordInformationEventCode) && !string.IsNullOrEmpty(report.Protector.ExcelProtectedInfoForAddingEvent))
				{
					yield return (parentBusinessObject, new EventValue(Events.All[(ZString)Events.DocumentPasswordInformation.Code], reference: report.Protector.ExcelProtectedInfoForAddingEvent));
				}
			}
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Const")]
	public static class DeliverableConstants
	{
		public const string Index = "Index";
	}

	public interface IDeliverableCollection
	{
	}
}
