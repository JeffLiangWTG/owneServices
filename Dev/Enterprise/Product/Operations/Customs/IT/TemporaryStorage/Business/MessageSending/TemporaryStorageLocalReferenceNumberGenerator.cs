using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

public class TemporaryStorageLocalReferenceNumberGenerator : ILocalReferenceNumberGenerator
{
	public TemporaryStorageLocalReferenceNumberGenerator(TemporaryStorageHeader header, ILocalReferenceNumberGenerator localReferenceNumberGenerator)
	{
		this.header = Argument.NotNull(header, nameof(header));
		this.localReferenceNumberGenerator = Argument.NotNull(localReferenceNumberGenerator, nameof(localReferenceNumberGenerator));
	}

	string ILocalReferenceNumberGenerator.Generate()
	{
		string firstGeneratedLrn = null;

		foreach (var bill in header.Bills.Where(x => x.Mrn.IsEmpty))
		{
			var localReferenceNumber = localReferenceNumberGenerator.Generate();

			if (firstGeneratedLrn == null)
			{
				firstGeneratedLrn = localReferenceNumber;
			}

			var entryNumber = CusEntryNumber.LoadOrCreate(
				bill,
				CusEntryNumberTypes.Standard.LocalReferenceNumber,
				Core.Constants.CountryCodes.Italy);

			entryNumber.CE_EntryNum = localReferenceNumber;
		}

		return firstGeneratedLrn;
	}

	readonly TemporaryStorageHeader header;
	readonly ILocalReferenceNumberGenerator localReferenceNumberGenerator;
}
