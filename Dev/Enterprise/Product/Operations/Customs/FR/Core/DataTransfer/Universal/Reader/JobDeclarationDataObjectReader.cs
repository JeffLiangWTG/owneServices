using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Registry;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using CusEntryHeader = Enterprise.Customs.FR.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.FR.DataTransfer.Universal;

public class JobDeclarationDataObjectReader : EU.DataTransfer.Universal.JobDeclarationDataObjectReader, ISnapshotReader
{
	public JobDeclarationDataObjectReader(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, Freight.Forwarding.Business.ForwardingShipment forwardingShipment = null) : base(declarationDataObject, logger, factory, forwardingShipment)
	{
	}

	protected override bool CanPopulateDeclarationWhenMessageSent => IsUsedForSnapshotReverting || base.CanPopulateDeclarationWhenMessageSent;

	protected override bool ShouldConsiderDeclarationReason => !IsUsedForSnapshotReverting && base.ShouldConsiderDeclarationReason;

	public IDisposable TemporarilyMarkJobDeclarationReaderIsBeingUsedForSnapshotReverting(CusEntryHeader entryHeader, SnapshotRevertingStrategy strategy, IXmlImportLogger snapshotReaderLogger)
	{
		EntryHeaderToRevert = entryHeader;
		Strategy = strategy;
		PackingLinksFromInvoiceLinesOfTheEntryHeaderToBeReverted = new Collection<ZInt>();
		SnapshotReaderLogger = snapshotReaderLogger;
		var matchingKeyOfInvoiceLinesToBeReverted = EntryHeaderToRevert.InvoiceLines.Select(line => line.JI_MatchingKey);
		dataObject.CommercialInfo.CommercialInvoiceCollection.SelectMany(invoice => invoice.CommercialInvoiceLineCollection).Select(line => (line.DataImportMatchingKey, line.Link)).ForEach(keyLinkMap =>
		{
			if (matchingKeyOfInvoiceLinesToBeReverted.Contains(keyLinkMap.DataImportMatchingKey.GetValueOrDefault()) && keyLinkMap.Link is ZInt link)
			{
				PackingLinksFromInvoiceLinesOfTheEntryHeaderToBeReverted.Add(link);
			}
		});

		return new DisposableAction(() =>
		{
			EntryHeaderToRevert = null;
			strategy = default;
			PackingLinksFromInvoiceLinesOfTheEntryHeaderToBeReverted = null;
		});
	}

	internal SnapshotRevertingStrategy Strategy { get; private set; }
	internal CusEntryHeader EntryHeaderToRevert { get; private set; }
	internal bool IsUsedForSnapshotReverting => EntryHeaderToRevert is not null;
	internal Collection<ZInt> PackingLinksFromInvoiceLinesOfTheEntryHeaderToBeReverted { get; private set; }
	internal IXmlImportLogger SnapshotReaderLogger;

	protected override void PopulateDeclaration(EU.Business.Declaration.JobDeclaration declaration, Shipment dataObject, IAddInfoManager addInfoManager, out Customs.DataTransfer.Universal.BillDetail primaryMasterBillDetail, out Customs.DataTransfer.Universal.BillDetail primaryHouseBillDetail, out Dictionary<string, ValueSetter> xmlPersistentSetters)
	{
		if (IsUsedForSnapshotReverting)
		{
			PreventDeclarationPropertiesFromUpdate();
		}

		if (!IsUsedForSnapshotReverting || Strategy.Equals(SnapshotRevertingStrategy.Override))
		{
			base.PopulateDeclaration(declaration, dataObject, addInfoManager, out primaryMasterBillDetail, out primaryHouseBillDetail, out xmlPersistentSetters);
		}
		else
		{
			primaryMasterBillDetail = GetPrimaryMasterBillDetail();
			primaryHouseBillDetail = GetPrimaryHouseBillDetail();
			xmlPersistentSetters = new Dictionary<string, ValueSetter>();
		}

		void PreventDeclarationPropertiesFromUpdate()
		{
			dataObject.MessageType = null;
			dataObject.CustomsProfileIdentifier = null;
		}
	}

	protected override OrgAddress FillSupplier(List<OrganizationAddress> organizationAddressCollection, EU.Business.Declaration.JobDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
	{
		OrgAddress result = null;
		if (!IsUsedForSnapshotReverting)
		{
			result = base.FillSupplier(organizationAddressCollection, declaration, delaySetters);
		}
		return result;
	}

	protected override OrgAddress FillImporter(List<OrganizationAddress> organizationAddressCollection, EU.Business.Declaration.JobDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
	{
		OrgAddress result = null;
		if (!IsUsedForSnapshotReverting)
		{
			result = base.FillImporter(organizationAddressCollection, declaration, delaySetters);
		}
		return result;
	}

	protected override void FillDeclarant(List<OrganizationAddress> organizationAddressCollection, EU.Business.Declaration.JobDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
	{
		if (!IsUsedForSnapshotReverting)
		{
			base.FillDeclarant(organizationAddressCollection, declaration, delaySetters);
		}
	}

	protected override void FillCollections(EU.Business.Declaration.JobDeclaration declaration, Shipment dataObject, bool isStandalone)
	{
		if (IsUsedForSnapshotReverting)
		{
			FillPackingLines(declaration, dataObject);
			var landedCostDataReader = GetLandedCostDataReader(declaration);
			FillCommercialInfo(declaration, dataObject, landedCostDataReader, null);
		}
		else
		{
			base.FillCollections(declaration, dataObject, isStandalone);
		}
	}

	protected override Customs.DataTransfer.Universal.CommercialInvoiceHeaderDataObjectReader<EU.Business.Declaration.JobComInvoiceGroupHeader> CreateNewCommercialInvoiceHeaderDataObjectReader(EU.Business.Declaration.JobComInvoiceGroupHeader groupHeader, CommercialInvoiceHeader invoiceData, Shipment dataObject, ILandedCostDataReader landedCostDataReader)
	{
		return new CommercialInvoiceHeaderDataObjectReader(invoiceData, logger, base.Helper, groupHeader, dataObject, landedCostDataReader, parentDeclarationReader: IsUsedForSnapshotReverting ? this : null);
	}

	protected override Customs.DataTransfer.Universal.CustomsPackingLineDataObjectReader CreateNewCustomsPackingLineDataObjectReader(PackingLine packingLineDataObject, BaseJobDeclaration declaration, IColumnIndexer billRow, bool supportsParentPackage)
	{
		return new CustomsPackingLineDataObjectReader(packingLineDataObject, logger, Helper, declaration, billRow, supportsParentPackage, parentDeclarationReader: IsUsedForSnapshotReverting ? this : null);
	}

	protected override Customs.DataTransfer.Universal.CustomsSupportingInformationCollectionDataObjectReader CreateNewCustomsSupportingInformationCollectionDataObjectReader() => new CustomsSupportingInformationCollectionDataObjectReader(logger, Helper, parentDeclarationReader: IsUsedForSnapshotReverting ? this : null);

	protected override void RemoveUnwantedPackingGroupOrChildrenPackage(BaseDeclarationLevelPackingGroupCollection packingGroups)
	{
		if (!IsUsedForSnapshotReverting || Strategy == SnapshotRevertingStrategy.Override)
		{
			base.RemoveUnwantedPackingGroupOrChildrenPackage(packingGroups);
		}
		else
		{
			var entryHeaderToRevert = EntryHeaderToRevert;
			var packingGroupsToDelete = new List<BasePackingGroup>();
			foreach (var packingGroup in packingGroups.Cast<BasePackingGroup>())
			{
				if (packingGroup.Packages.Cast<BasePackage>().All(package => package.InvoiceLinePivotCollection.Cast<InvoiceLinePackagePivot>().All(pivot => entryHeaderToRevert.InvoiceLines.Select(line => line.PK).Contains(pivot.InvoiceLine.PK))))
				{
					packingGroupsToDelete.Add(packingGroup);
				}
				else
				{
					var packagesToDelete = new List<BasePackage>();
					foreach (var package in packingGroup.Packages.Cast<BasePackage>())
					{
						if (package.InvoiceLinePivotCollection.Cast<InvoiceLinePackagePivot>().All(pivot => entryHeaderToRevert.InvoiceLines.Select(line => line.PK).Contains(pivot.InvoiceLine.PK)))
						{
							packagesToDelete.Add(package);
						}
					}
					packingGroup.LoadChildrenForDeletionIncludingCusAddInfoAndCusCodeData(packagesToDelete.ToArray());
					foreach (var package in packagesToDelete)
					{
						packingGroup.Packages.RemoveAndDelete(package);
					}
				}
			}
			foreach (var packingGroup in packingGroupsToDelete)
			{
				packingGroups.RemoveAndDelete(packingGroup);
			}
		}
	}

	protected override void AddOrExecuteSetter(Dictionary<string, ValueSetter> delaySetters, ValueSetter setter)
	{
		if (IsUsedForSnapshotReverting && setter is IColumnValueSetterInfo columnValueSetter)
		{
			var originalValue = columnValueSetter.Row[columnValueSetter.Column.Name];
			base.AddOrExecuteSetter(delaySetters, setter);
			LogSetterMessage(setter, originalValue);
		}
		else
		{
			base.AddOrExecuteSetter(delaySetters, setter);
		}
	}

	internal void LogSetterMessage(ValueSetter setter, object originalValue)
	{
		var columnValueSetter = ((IColumnValueSetterInfo)setter);
		var newValue = columnValueSetter.Value;
		if ((newValue is IZType value && !value.Equals(originalValue)) || (newValue is ICodeDataObject codeValue && !codeValue.Code.Equals(originalValue)))
		{
			SnapshotReaderLogger?.Log(LogType.Information, $"{columnValueSetter.Column.Name} is updated from `{originalValue}` to `{((newValue is IZType) ? newValue : ((ICodeDataObject)newValue).Code)}`");
		}
	}

	protected override bool IsValidApplicationCode(EU.Business.Declaration.JobDeclaration declaration, ZString applicationCode)
	{
		var frDeclaration = (JobDeclaration)declaration;
		var isDeltaIE = (MessageTypeCode == JobMessageTypeList.Codes.Import && FRCustomsDataRegistry.Instance.EnableDeltaIEForImports.Value)
		|| (MessageTypeCode == JobMessageTypeList.Codes.Export && FRCustomsDataRegistry.Instance.EnableDeltaIEForExports.Value);

		return frDeclaration.Lookups.GetApplicationCodeList(isDeltaIE).ContainsCode(applicationCode);
	}
}
