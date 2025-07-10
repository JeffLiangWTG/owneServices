using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NCTSPrettierSharedFields : IReadOnlyCollection<(string Key, string Value)>
	{
		readonly Dictionary<string, string> sharedFields = new ();

		public NCTSPrettierSharedFields(IEnumerable<(string Key, string Value)> initItems = null)
		{
			if (initItems != null)
			{
				foreach (var valueTuple in initItems)
				{
					sharedFields[valueTuple.Key] = valueTuple.Value;
				}
			}
		}

		public string MessageTypeFieldName { get; } = Res.GetString("79D3ED7D-FEF6-4ED8-BBA6-3AA6FCC2E886", "Message Type");
		public string MessageType
		{
			get => this[MessageTypeFieldName];
			set => this[MessageTypeFieldName] = value;
		}

		public string MessageRecipientFieldName { get; } = Res.GetString("32DA4954-6ED4-4716-A93E-CC7AA000C268", "Message Recipient");
		public string MessageRecipient
		{
			get => this[MessageRecipientFieldName];
			set => this[MessageRecipientFieldName] = value;
		}

		public string DeclarationTypeFieldName { get; } = Res.GetString("230E2559-9141-42A7-BBC9-1008E02DD3F1", "Declaration Type");
		public string DeclarationType
		{
			get => this[DeclarationTypeFieldName];
			set => this[DeclarationTypeFieldName] = value;
		}

		public string AdditionalDeclarationTypeFieldName { get; } = Res.GetString("00FEC1D3-517D-45D6-83E1-2E5D16F7E656", "Additional Declaration Type");
		public string AdditionalDeclarationType
		{
			get => this[AdditionalDeclarationTypeFieldName];
			set => this[AdditionalDeclarationTypeFieldName] = value;
		}

		public string LRNFieldName { get; } = Res.GetString("C65DE22A-B1E9-4FFA-ABBB-5A5753731134", "LRN (Local Reference Number)");
		public string LRN
		{
			get => this[LRNFieldName];
			set => this[LRNFieldName] = value;
		}

		public string MRNFieldName { get; } = Res.GetString("35A47EC8-69E9-4F53-BBB5-10621FDE2040", "MRN (Movement Reference Number)");
		public string MRN
		{
			get => this[MRNFieldName];
			set => this[MRNFieldName] = value;
		}

		public string CustomsOfficeOfDepartureFieldName { get; } = Res.GetString("96B0A396-7E9C-433E-8114-62D4DAE7DA03", "Customs Office (Departure)");
		public string CustomsOfficeOfDeparture
		{
			get => this[CustomsOfficeOfDepartureFieldName];
			set => this[CustomsOfficeOfDepartureFieldName] = value;
		}

		public string CustomsOfficeOfDestinationFieldName { get; } = Res.GetString("515915DF-0DE4-4F77-A498-6EF551278383", "Customs Office (Destination)");
		public string CustomsOfficeOfDestination
		{
			get => this[CustomsOfficeOfDestinationFieldName];
			set => this[CustomsOfficeOfDestinationFieldName] = value;
		}

		public string ReducedDatasetIndicatorFieldName { get; } = Res.GetString("A7D10624-7526-4DB3-9E7E-E257DB5311CC", "Reduced Dataset");
		public string ReducedDatasetIndicator
		{
			get => this[ReducedDatasetIndicatorFieldName];
			set => this[ReducedDatasetIndicatorFieldName] = value;
		}

		public string SimplifiedProcedureFieldName { get; } = Res.GetString("1FB8AAC6-0303-4D01-AD05-A9FDECEC77BF", "Simplified Procedure");
		public string SimplifiedProcedure
		{
			get => this[SimplifiedProcedureFieldName];
			set => this[SimplifiedProcedureFieldName] = value;
		}

		public string SecurityFieldName { get; } = Res.GetString("9FCB5322-2ED1-4520-87FD-3D2C18225109", "Security");
		public string Security
		{
			get => this[SecurityFieldName];
			set => this[SecurityFieldName] = value;
		}

		public string BindingItineraryFieldName { get; } = Res.GetString("81636EE1-FF23-4413-A4E8-58B540CDE34C", "Binding Itinerary");
		public string BindingItinerary
		{
			get => this[BindingItineraryFieldName];
			set => this[BindingItineraryFieldName] = value;
		}

		public string PrincipleEORIFieldName { get; } = Res.GetString("7C5E79B4-645A-4842-AA35-1A8CAECC0909", "Principle EORI");
		public string PrincipleEORI
		{
			get => this[PrincipleEORIFieldName];
			set => this[PrincipleEORIFieldName] = value;
		}

		public string RepresentativeEORIFieldName { get; } = Res.GetString("D7105D21-A073-4B46-A9A5-D4B0C061C826", "Representative EORI");
		public string RepresentativeEORI
		{
			get => this[RepresentativeEORIFieldName];
			set => this[RepresentativeEORIFieldName] = value;
		}

		public string ConsigneeEORIFieldName { get; } = Res.GetString("FD19E9AC-ECE3-49BB-90AA-99F7323F4FB2", "Consignee EORI");
		public string ConsigneeEORI
		{
			get => this[ConsigneeEORIFieldName];
			set => this[ConsigneeEORIFieldName] = value;
		}

		public string ConsignorEORIFieldName { get; } = Res.GetString("23145543-FD20-4DF2-A66B-ECD6EC51CE01", "Consignor EORI");
		public string ConsignorEORI
		{
			get => this[ConsignorEORIFieldName];
			set => this[ConsignorEORIFieldName] = value;
		}

		public string this[string key]
		{
			get => sharedFields.TryGetValue(key, out var result) ? result : string.Empty;
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					sharedFields[key] = value;
				}
				else if (sharedFields.ContainsKey(key))
				{
					sharedFields.Remove(key);
				}
			}
		}

		public IEnumerator<(string Key, string Value)> GetEnumerator() => sharedFields.Select(x => (Key: x.Key, Value: x.Value)).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public int Count => sharedFields.Count;
	}
}
