using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5GU;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBR5GUDataProvider
	{
		public GOVCBR5GUMessageData GetMessageData(BusinessObjectFactory factory, TextReader messageTextReader, EDIMessage message)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);

			var result = new GOVCBR5GUMessageData(factory);

			result.ImportDeclarationNumber = response.Declaration.Id.Value;
			if (DateTime.TryParseExact(response.Declaration.IssueDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt1))
			{
				result.CustomsRegistryDate = new ZDate(dt1);
			}
			if (DateTime.TryParseExact(response.Declaration.ExpirationDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt2))
			{
				result.CorrectionOrderDeadline = new ZDate(dt2);
			}
			result.DeclarationOffice = response.Declaration.DeclarationOfficeId.Value;
			result.CustomsPersonName = response.Authenticator.Name.Value;
			result.ComplementNumber = response.Declaration.AdditionalDocument.Id.Value;
			result.CustomsPersonPhoneNumber = response.Authenticator.Communication.Id.Value;

			var entry = MessageLinkedObjectManager.GetLinkedObject(factory, message.Branch.Company, response.Declaration.Id.Value, SharedJobMessageTypeList.Codes.Import);
			var snapShot = entry?.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._929, EntrySnapshotStatus.Lodged);
			var entryLineDictionary = new Dictionary<int, ImportEntryLine>();
			if (snapShot != null)
			{
				using (var textReader = snapShot.GetCES_SnapshotXmlReader())
				{
					var snapShotData = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<ImportEntryHeader>(textReader);

					foreach (var entryLine in snapShotData.EntryLines)
					{
						entryLineDictionary.Add(entryLine.EntryLineNo, entryLine);
					}
				}
			}

			foreach (var violation in response.Declaration.GoodsShipment)
			{
				var correction = result.Corrections.AddNew();
				correction.EntryLineNo = Convert.ToInt32(violation.SequenceNumeric);
				correction.ViolationCode = violation.AdditionalInformation.StatementCode.Value;
				correction.ViolationName = violation.AdditionalInformation.StatementDescription.Value;
				correction.CorrectionMethod = violation.AdditionalInformation.Content.Value;

				ImportEntryLine entryLine;
				if (entryLineDictionary.TryGetValue(correction.EntryLineNo, out entryLine))
				{
					correction.TariffDescription = entryLine.HSDescription;
					correction.Tariff = factory.GetCachedValue<TariffFormatter>().DisplayFormat(entryLine.HSCode);
					correction.CountryOfOrigin = entryLine.CountryOfOrigin;
					correction.CustomsQuantity = entryLine.Quantity;
					correction.CustomsUnitQty = entryLine.QuantityUnit;
					correction.NetWeightInKG = entryLine.NetWeightInKG;
					correction.CustomsValueUSD = entryLine.CustomsValueUSD;
				}
			}

			return result;
		}
	}
}
