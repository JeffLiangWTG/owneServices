using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public static class LocalExportAmendmentDetailsDecorator
	{
		public static void Decorate(this LocalExportAmendmentDetails details, CusEntryHeader entry, ZShort versionNumber, ZString outgoingMessageType)
		{
			if (entry != null)
			{
				var localExportDeclarationSnapshots = entry.Snapshots.Cast<CusEntrySnapshot>().Where(x => x.CES_MessageType == outgoingMessageType);
				var matchedSnapshot = GetSnapshotData(localExportDeclarationSnapshots, versionNumber, TargetSnapshotType.Matched);

				if (matchedSnapshot != null)
				{
					if (matchedSnapshot.Supplier != null)
					{
						details.Supplier.CompanyName = matchedSnapshot.Supplier.CompanyName;
						details.Supplier.AddressDetails = matchedSnapshot.Supplier.GetAddressDetails();
						details.Supplier.RepresentativeName = matchedSnapshot.Supplier.RepresentativeName;
					}
				}
				else
				{
					if (entry.RandomHeader.Supplier != null)
					{
						details.Supplier.CompanyName = entry.RandomHeader.Supplier.OH_FullName;
						details.Supplier.AddressDetails = entry.RandomHeader.Supplier.MainAddress.GetAddressDetails();
						details.Supplier.RepresentativeName = entry.RandomHeader.Supplier.GetRepresentativeName();
					}
				}
			}
		}

		enum TargetSnapshotType { Matched, PreviouslyLodged }

		static LocalExportEntryHeader GetSnapshotData(IEnumerable<CusEntrySnapshot> snapshots, ZShort targetVersionNumber, TargetSnapshotType targetSnapshotType)
		{
			LocalExportEntryHeader result = null;
			var snapshot = targetSnapshotType == TargetSnapshotType.PreviouslyLodged ?
				snapshots.Where(x => x.CES_Status == EntrySnapshotStatus.Lodged && x.CES_VersionNumber < targetVersionNumber).OrderBy(x => x.CES_VersionNumber).LastOrDefault()
				: snapshots.FirstOrDefault(x => x.CES_VersionNumber == targetVersionNumber);
			if (snapshot != null)
			{
				using (var textReader = snapshot.GetCES_SnapshotXmlReader())
				{
					result = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<LocalExportEntryHeader>(textReader);
				}
			}

			return result;
		}
	}
}
