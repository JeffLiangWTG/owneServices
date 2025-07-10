using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngineCore;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class DummyDeliverable : NonPersistentBusinessObject, IDeliverable
	{
		public ZString DocumentDeliveredEventCode { get; set; }
		public ZString DocumentPasswordInformationEventCode { get; set; }
		public ZString FileExtension { get; set; }
		public ZString Name { get; set; }

		public ZPropertyInfo NameInfo
		{
			get { return GetZPropertyInfo(nameof(Name)); }
		}

		public Action<DocDeliveryContact, DocDeliveryContact, Stream> SaveStrategy { get; set; }
		public void Save(DocDeliveryContact deliveryContact, DocDeliveryContact mostOfficialContact, Stream fileContent)
		{
			if (SaveStrategy != null)
			{
				SaveStrategy(deliveryContact, mostOfficialContact, fileContent);
			}
		}

		public DeliveryInfo GetDeliveryInfo(bool isDraft)
		{
			var result = new DeliveryInfo(Enterprise.DocumentEngine.DeliveryMethods.DeliveryInfo.DeliveryFormats.Document);
			result.ShowDraftWatermark = isDraft;
			return result;
		}

		public ZString DeliveryMode { get; set; }

		public ZPropertyInfo DeliveryModeInfo
		{
			get { return GetZPropertyInfo(nameof(DeliveryMode)); }
		}

		public ZString AllAvailableDeliveryModes
		{
			get { return nameof(PrintCopyType.ALL); }
		}

		public ZString DocumentTypeCode
		{
			get { return ""; }
		}

		public ZPropertyInfo DocumentTypeCodeInfo
		{
			get { return GetZPropertyInfo(nameof(DocumentTypeCode)); }
		}

		public ZString DocumentTypeDescription
		{
			get { return ""; }
		}

		public ZPropertyInfo DocumentTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(DocumentTypeDescription)); }
		}

		public ZGuid DeliveryGroupID { get; set; }

		public ZBool IncludedInPrint { get; set; }

		public ZPropertyInfo IncludedInPrintInfo
		{
			get { return GetZPropertyInfo(nameof(IncludedInPrint)); }
		}

		public bool IncludedInPrint_ReadOnly { get; set; }

		public bool IsDeliveredByEmail { get; set; }

		public StmMenuItem MenuItem { get; set; }

		public ZBool CoverSheetRequired { get; set; }

		public bool ContainsDataRows { get; set; }

		public void DeleteTempFilesForTesting()
		{
		}

		public int RunCountForTesting { get; set; }

		public void Dispose()
		{
		}

		public bool SupportsDeliveryMethod(string deliveryMethod)
		{
			return true;
		}

		public IEnumerable<string> GetSupportedDeliveryMethods()
		{
			return DeliveryMethodHelper.GetSupportedDelvieryMethodsFor(PrintCopyType.ALL);
		}

		public string DocumentDeliveryMethod { get; set; }

		public string DocumentName { get; set; }

		public bool IncludeInPrint { get; set; }

		public bool CanIncludeInPrint { get; set; }

		public IEnumerable<string> GetSupportedDeliveryMethodDespiteOfPrintCopyType()
		{
			return DeliveryMethodHelper.GetSupportedDelvieryMethodsFor(PrintCopyType.PRN);
		}

		public ZGuid SourcePivotPK { get; set; }

		public ZGuid MenuTemplatePivotPK { get; }

		public ZGuid IdentifiablePK { get; }

		public bool ShouldPrintByDefault { get; set; }

		public DocDeliveryPrintDetails PrinterDetails => throw new NotImplementedException();

		public ZString JobNumber { get; set; }

		public ZString NameForBinding { get; }

		public ZByte Index { get; set; }

		public ZPropertyInfo IndexInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(Index));
			}
		}

		public ZString Identifier => "";
	}
}
