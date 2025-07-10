using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public static class ExportAmendmentDetailsDecorator
	{
		public static void Decorate(this ExportAmendmentDetails details, CusEntryHeader entry, ZShort versionNumber)
		{
			if (entry != null)
			{
				var exportDeclarationSnapshots = entry.Snapshots.Cast<CusEntrySnapshot>().Where(x => x.CES_MessageType == ElectronicDocumentTypeList.Codes._830);
				var matchedSnapshot = GetSnapshotData(exportDeclarationSnapshots, versionNumber, TargetSnapshotType.Matched);

				details.DeclarationDate = entry.CusEntryNumber?.CE_IssueDate ?? ZDateTime.Empty;
				details.ReleaseDate = entry.CH_EntryReleaseDate;

				if (details.AmendmentType == _5ASAmendmentType.Codes.Amendment)
				{
					details.BeforeTotalCustomsValue = GetSnapshotData(exportDeclarationSnapshots, versionNumber, TargetSnapshotType.PreviouslyLodged)?.TotalCustomsValue ?? ZDecimal.Zero;
					details.AfterTotalCustomsValue = matchedSnapshot?.TotalCustomsValue ?? ZDecimal.Zero;
					details.BeforeTotalCustomsValueUSD = GetCustomsValueUSD(entry, details.BeforeTotalCustomsValue);
					details.AfterTotalCustomsValueUSD = GetCustomsValueUSD(entry, details.AfterTotalCustomsValue);
				}

				if (matchedSnapshot != null)
				{
					if (matchedSnapshot.Declarant != null)
					{
						details.BrokerCompanyName = matchedSnapshot.Declarant.CompanyName;
						details.BrokerCompanyRepresentative = matchedSnapshot.Declarant.RepresentativeName;
					}
					if (matchedSnapshot.Supplier != null)
					{
						details.SupplierCompanyName = matchedSnapshot.Supplier.CompanyName;
						details.SupplierAddress = matchedSnapshot.Supplier.GetAddressDetails();
						details.SupplierCompanyID = matchedSnapshot.Supplier.UnipassIDForOrganization;
					}
				}
				else
				{
					if (entry.Declaration.BrokerAddress != null)
					{
						details.BrokerCompanyName = entry.Declaration.BrokerAddress.CompanyName;
						details.BrokerCompanyRepresentative = entry.Declaration.BrokerAddress.Header.GetRepresentativeName();
					}

					var supplier = entry.Declaration.SupplierAddress;
					if (supplier != null)
					{
						details.SupplierCompanyName = supplier.CompanyName;
						details.SupplierAddress = supplier.Address1AndAddress2;
						details.SupplierCompanyID = supplier.Header.GetRegistrationIDNumber(IdentificationType.UnipassIDForOrganization)?.Number ?? ZString.Empty;
					}
				}
			}
		}

		enum TargetSnapshotType { Matched, PreviouslyLodged }

		static ExportEntryHeader GetSnapshotData(IEnumerable<CusEntrySnapshot> snapshots, ZShort targetVersionNumber, TargetSnapshotType targetSnapshotType)
		{
			ExportEntryHeader result = null;
			var snapshot = targetSnapshotType == TargetSnapshotType.PreviouslyLodged ?
				snapshots.Where(x => x.CES_Status == EntrySnapshotStatus.Lodged && x.CES_VersionNumber < targetVersionNumber).OrderBy(x => x.CES_VersionNumber).LastOrDefault()
				: snapshots.FirstOrDefault(x => x.CES_VersionNumber == targetVersionNumber);
			if (snapshot != null)
			{
				using (var textReader = snapshot.GetCES_SnapshotXmlReader())
				{
					result = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<ExportEntryHeader>(textReader);
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1068:DoNotUseMathRound", Justification = "Baseline")]
		static ZDecimal GetCustomsValueUSD(CusEntryHeader entry, ZDecimal customsValue)
		{
			var result = ZDecimal.Zero;

			if (customsValue != ZDecimal.Zero)
			{
				var krwCurrency = entry.Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.KoreaRepublicOf);
				var usdCurrency = entry.Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);

				result = Math.Round(entry.CurrencyConverter.ConvertExact(new Money(customsValue, krwCurrency), usdCurrency).Amount);
			}

			return result;
		}
	}
}
