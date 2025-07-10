using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Messaging;

public class NbSadFieldDescriptionProvider : CustomsFieldDescriptionsProviderWithSequenceNumber<NbSadFieldDescriptionList>
{
	public NbSadFieldDescriptionProvider(BusinessObjectFactory factory) : base(factory)
	{
	}

	protected virtual int FieldStartingIndex => 12;

	protected override IEnumerable<(string, string)> GetCorrelations()
	{
		var correlations = new List<CodeDescriptionPair>();
		correlations.AddRange(GetCorrelationsForNotRepeatedFields());
		correlations.AddRange(GetCorrelationsForRepeatedFields());
		return correlations.Select(x => (x.Code, x.Description));
	}

	protected virtual IEnumerable<CodeDescriptionPair> GetCorrelationsForNotRepeatedFields()
	{
		var correlations = new List<CodeDescriptionPair>();
		correlations.Add(new CodeDescriptionPair(ConvertToString(7), NbSadFieldDescriptionList.Codes.RecordType));
		correlations.Add(new CodeDescriptionPair(ConvertToString(8), NbSadFieldDescriptionList.Codes.AnnualSequenceNumber));
		correlations.Add(new CodeDescriptionPair(ConvertToString(11), NbSadFieldDescriptionList.Codes.ItemNumber5));
		return correlations;
	}

	protected IEnumerable<CodeDescriptionPair> GetCorrelationsForRepeatedFields()
	{
		var correlations = new List<CodeDescriptionPair>();
		for (int i = 0; i < 4; i++)
		{
			var startingIndexForIteration = FieldStartingIndex + (i * 20);

			correlations.Add(new CodeDescriptionPair(ConvertToString(startingIndexForIteration), NbSadFieldDescriptionList.Codes.RegisterCodePA1));
			correlations.Add(new CodeDescriptionPair(ConvertToString(startingIndexForIteration + 1), NbSadFieldDescriptionList.Codes.NumberPA2));
			correlations.Add(new CodeDescriptionPair(ConvertToString(startingIndexForIteration + 2), NbSadFieldDescriptionList.Codes.CinPA3));
			correlations.Add(new CodeDescriptionPair(ConvertToString(startingIndexForIteration + 3), NbSadFieldDescriptionList.Codes.DatePA4));
			correlations.Add(new CodeDescriptionPair(ConvertToString(startingIndexForIteration + 4), NbSadFieldDescriptionList.Codes.SeriesPA5));
			correlations.Add(new CodeDescriptionPair(ConvertToString(startingIndexForIteration + 5), NbSadFieldDescriptionList.Codes.ItemNumberPA6));
			correlations.Add(new CodeDescriptionPair(ConvertToString(startingIndexForIteration + 6), NbSadFieldDescriptionList.Codes.CustomsOfficePA7));
			correlations.Add(new CodeDescriptionPair(ConvertToString(startingIndexForIteration + 7), NbSadFieldDescriptionList.Codes.Mrn));

			correlations.Add(new CodeDescriptionPair(ConvertToString(startingIndexForIteration + 8), NbSadFieldDescriptionList.Codes.RegisterCodeRP1));
			correlations.Add(new CodeDescriptionPair(ConvertToString(startingIndexForIteration + 9), NbSadFieldDescriptionList.Codes.NumberRP2));
			correlations.Add(new CodeDescriptionPair(ConvertToString(startingIndexForIteration + 10), NbSadFieldDescriptionList.Codes.CinRP3));
			correlations.Add(new CodeDescriptionPair(ConvertToString(startingIndexForIteration + 11), NbSadFieldDescriptionList.Codes.DateRP4));
			correlations.Add(new CodeDescriptionPair(ConvertToString(startingIndexForIteration + 12), NbSadFieldDescriptionList.Codes.SeriesRP5));
			correlations.Add(new CodeDescriptionPair(ConvertToString(startingIndexForIteration + 13), NbSadFieldDescriptionList.Codes.ItemNumberRP6));
			correlations.Add(new CodeDescriptionPair(ConvertToString(startingIndexForIteration + 14), NbSadFieldDescriptionList.Codes.CustomsOfficeRP7));

			correlations.Add(new CodeDescriptionPair(ConvertToString(startingIndexForIteration + 15), NbSadFieldDescriptionList.Codes.NumberOfPackages));
			correlations.Add(new CodeDescriptionPair(ConvertToString(startingIndexForIteration + 16), NbSadFieldDescriptionList.Codes.GrossMass));
			correlations.Add(new CodeDescriptionPair(ConvertToString(startingIndexForIteration + 17), NbSadFieldDescriptionList.Codes.Tariff));
			correlations.Add(new CodeDescriptionPair(ConvertToString(startingIndexForIteration + 18), NbSadFieldDescriptionList.Codes.NetMass));
			correlations.Add(new CodeDescriptionPair(ConvertToString(startingIndexForIteration + 19), NbSadFieldDescriptionList.Codes.SupplementaryUnit));
		}
		return correlations;
	}

	string ConvertToString(int value) => value.ToString(CultureInfo.InvariantCulture);
}
