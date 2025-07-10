using System.Text.RegularExpressions;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(BillCustomisationRegistryDataType))]
	sealed class BillCustomisationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<BillCustomisationRegistryDataType>
	{
		public void TestDefaultCategory()
		{
			AssertEquals(NumberCustomisationElementCategories.Default, DataType.DefaultValue.Categories);

			DataType.Categories = NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.LinerAgency;
			AssertEquals(NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.LinerAgency, DataType.DefaultValue.Categories);
		}

		public void TestDefaultPrefixLength()
		{
			AssertEquals(1, DataType.DefaultValue.PrefixLength);

			DataType.PrefixLength = 2;
			AssertEquals(2, DataType.DefaultValue.PrefixLength);
		}

		public void TestDefaultAllowNonAlphanumericCharacters()
		{
			AssertEquals(false, DataType.AllowNonAlphanumericCharacters);

			DataType.AllowNonAlphanumericCharacters = true;
			AssertEquals(true, DataType.DefaultValue.AllowNonAlphanumericCharacters);
		}

		public void TestDefaultEnableMacroInsertion()
		{
			AssertEquals(false, DataType.EnableMacroInsertion);

			DataType.EnableMacroInsertion = true;
			AssertEquals(true, DataType.DefaultValue.EnableMacroInsertion);
		}

		public void TestMacroType()
		{
			AssertNull("Macro Type should be null for default.", DataType.MacroType);
		}

		public void TestDeserialiseCategory()
		{
			ValidSampleAndBinaryValueInDB sample = GetValidSamples()[0];

			AssertEquals(NumberCustomisationElementCategories.Default, DataType.Deserialise(sample.BinaryValue).Categories);

			DataType.Categories = NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.LinerAgency;
			AssertEquals(NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.LinerAgency, DataType.Deserialise(sample.BinaryValue).Categories);
		}

		public void TestDeserialisePrefixLength()
		{
			ValidSampleAndBinaryValueInDB sample = GetValidSamples()[0];

			AssertEquals(1, DataType.Deserialise(sample.BinaryValue).PrefixLength);

			DataType.PrefixLength = 2;
			AssertEquals(2, DataType.Deserialise(sample.BinaryValue).PrefixLength);
		}

		public void TestDeserialiseAllowNonAlphanumericCharactersAndMaxAllowedLength()
		{
			ValidSampleAndBinaryValueInDB sample = GetValidSamples()[0];
			DataType.MaxLength = 6;
			AssertEquals(false, DataType.Deserialise(sample.BinaryValue).AllowNonAlphanumericCharacters);
			AssertEquals(false, DataType.Deserialise(sample.BinaryValue).EnableMacroInsertion);
			AssertEquals(6, DataType.Deserialise(sample.BinaryValue).MaxAllowedLength);

			DataType.AllowNonAlphanumericCharacters = true;
			DataType.EnableMacroInsertion = true;
			AssertEquals(true, DataType.Deserialise(sample.BinaryValue).AllowNonAlphanumericCharacters);
			AssertEquals(true, DataType.Deserialise(sample.BinaryValue).EnableMacroInsertion);
		}

		public void TestIBillCustomisationRegistryDataType()
		{
			var dataType = new BillCustomisationRegistryDataType
			{
				FountainPrefix = "TEST1",
				GeneratedNumberName = (NoResString)"TEST2",
				SequenceNumberName = (NoResString)"TEST3",
				MaxLength = 42,
				Categories = NumberCustomisationElementCategories.WarehouseJob,
				AllowNonAlphanumericCharacters = true,
				EnableMacroInsertion = true,
				MacroType = typeof(string),
			};

			CombineAssertions(() =>
			{
				var dataTypeInterface = dataType as IBillCustomisationRegistryDataType;
				AssertNotNull("Should implement IBillCustomisationRegistryDataType.", dataTypeInterface);

				AssertEquals(nameof(dataTypeInterface.FountainPrefix), dataType.FountainPrefix, dataTypeInterface.FountainPrefix);
				AssertEquals(nameof(dataTypeInterface.GeneratedNumberName), dataType.GeneratedNumberName, dataTypeInterface.GeneratedNumberName);
				AssertEquals(nameof(dataTypeInterface.SequenceNumberName), dataType.SequenceNumberName, dataTypeInterface.SequenceNumberName);
				AssertEquals(nameof(dataTypeInterface.MaxLength), dataType.MaxLength, dataTypeInterface.MaxLength);
				AssertEquals(nameof(dataTypeInterface.Categories), dataType.Categories, dataTypeInterface.Categories);
				AssertEquals(nameof(dataTypeInterface.AllowNonAlphanumericCharacters), dataType.AllowNonAlphanumericCharacters, dataTypeInterface.AllowNonAlphanumericCharacters);
				AssertEquals(nameof(dataTypeInterface.EnableMacroInsertion), dataType.EnableMacroInsertion, dataTypeInterface.EnableMacroInsertion);
				AssertEquals(nameof(dataTypeInterface.MacroType), dataType.MacroType, dataTypeInterface.MacroType);
			});
		}

		#region Implementation

		protected override BillCustomisationRegistryDataType GetNewDataType()
		{
			return new BillCustomisationRegistryDataType(new BillOfLadingNumberCustomisation());
		}

		protected override string ExpectedEditorName
		{
			get { return "BillOfLadingNumberCustomisationRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var customisation = new BillOfLadingNumberCustomisation();

			customisation.RemoveFountainPrefix = true;
			customisation.UseShipmentSequenceNumber = false;
			customisation.CheckDigitAlgorithm = CheckDigitAlgorithmList.Codes.None;

			customisation.UnFilteredElements.Sort(BillOfLadingNumberCustomisationElement.Schema.Key);
			for (int i = 0; i < customisation.UnFilteredElements.Count; i++)
			{
				var element = customisation.UnFilteredElements[i];
				element.Include = true;
				element.Order = (byte)i;

				switch (element.Key)
				{
					case BillOfLadingNumberCustomisationElement.Keys.ClientCoded1:
						element.Detail = new string('A', element.DetailInfo.MaxLength);
						break;

					case BillOfLadingNumberCustomisationElement.Keys.SequenceNumber:
						element.Detail = "8";
						break;

					case BillOfLadingNumberCustomisationElement.Keys.YearAsDigit:
						element.Detail = "1";
						break;

					default:
						element.Detail = "";
						break;
				}
			}

			var xml = @"<?xml version=""1.0"" encoding=""utf-16""?>
<BillOfLadingNumberCustomisation>
	<ServiceLevel />
	<RemoveFountainPrefix>Y</RemoveFountainPrefix>
	<AutoAllocateMasterBillNumbersToConsols>N</AutoAllocateMasterBillNumbersToConsols>
	<CheckDigitAlgorithm>NON</CheckDigitAlgorithm>
	<UseShipmentSequenceNumber>N</UseShipmentSequenceNumber>
	<Elements>
		<Element key=""BranchCode"">
			<Order>0</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
		<Element key=""CarrierPrincipalCode"">
			<Order>1</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
		<Element key=""ClientCoded"">
			<Order>2</Order>
			<CheckDigit>Y</CheckDigit>
			<Detail>AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA</Detail>
		</Element>
		<Element key=""ClientCoded2"">
			<Order>3</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
		<Element key=""ClientCoded3"">
			<Order>4</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
		<Element key=""ClientOrganisation"">
			<Order>5</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
		<Element key=""CompanyCode"">
			<Order>6</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
		<Element key=""ContainerTranshipmentIndicator"">
			<Order>7</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
		<Element key=""DestinationIATA"">
			<Order>8</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
		<Element key=""DestinationUNLOCO"">
			<Order>9</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
		<Element key=""Direction"">
			<Order>10</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
		<Element key=""DischargeIATA"">
			<Order>11</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
		<Element key=""DischargeUNLOCO"">
			<Order>12</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
		<Element key=""EnterpriseCode"">
			<Order>13</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
		<Element key=""FirstLoadIATA"">
			<Order>14</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
		<Element key=""FirstLoadUNLOCO"">
			<Order>15</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
		<Element key=""GlobalOrLocal"">
			<Order>16</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
		<Element key=""JobNo"">
			<Order>17</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
		<Element key=""LastDischargeIATA"">
			<Order>18</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
		<Element key=""LastDischargeUNLOCO"">
			<Order>19</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
		<Element key=""LoadIATA"">
			<Order>20</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
		<Element key=""LoadUNLOCO"">
			<Order>21</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
		<Element key=""MonthAs2Digits"">
			<Order>22</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
		<Element key=""MonthAsLetter"">
			<Order>23</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
		<Element key=""OriginIATA"">
			<Order>24</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
		<Element key=""OriginUNLOCO"">
			<Order>25</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
		<Element key=""Quarter"">
			<Order>26</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
		<Element key=""SequenceNumber"">
			<Order>27</Order>
			<CheckDigit>Y</CheckDigit>
			<Detail>8</Detail>
		</Element>
		<Element key=""ServerCode"">
			<Order>28</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
		<Element key=""ServiceLevel"">
			<Order>29</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
		<Element key=""SundryChargesActivity"">
			<Order>30</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
		<Element key=""SundryChargesMode"">
			<Order>31</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
		<Element key=""SundryChargesType"">
			<Order>32</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
		<Element key=""TransportMode"">
			<Order>33</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
		<Element key=""UniversalOfficeCode"">
			<Order>34</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
		<Element key=""WarehouseCode"">
			<Order>35</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
		<Element key=""WarehouseClientCode"">
			<Order>36</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
		<Element key=""WarehouseSalesChannelCode"">
			<Order>37</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
		<Element key=""WarehouseSubType"">
			<Order>38</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
		<Element key=""WarehouseSupplierCode"">
			<Order>39</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
		<Element key=""WarehouseReceiveCategoryCode"">
			<Order>40</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
		<Element key=""YearAsDigit"">
			<Order>41</Order>
			<CheckDigit>Y</CheckDigit>
			<Detail>1</Detail>
		</Element>
		<Element key=""YearAsLetter"">
			<Order>42</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
	</Elements>
</BillOfLadingNumberCustomisation>";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(customisation, Regex.Replace(xml, @"\t|\n|\r", ""))
			};
		}

		#endregion
	}
}
