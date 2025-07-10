using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Customs.Shared.WorldCustomsOrganisation.MessageDefinitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestsSubclassesOf(typeof(FriendlyCodeWithPointers))]
	public abstract class PointerParserAbstractTest<T, TMetaData> : TestCaseWithFactory
		where T : FriendlyCodeWithPointers
		where TMetaData : class
	{
		protected abstract TMetaData GetMetaData { get; }
		protected abstract T GetFriendlyCodeWithPointers(ICodeWithPointers c);
		protected abstract IPointerParser Parser { get; }

		public virtual IEnumerable<ICodeWithPointers> GetResponses(TMetaData metaData) => Enumerable.Empty<ICodeWithPointers>();

		public virtual void TestGetErrorWithPointers()
		{
			var metaData = GetMetaData;

			var errorWithPointers = GetResponses(metaData);

			var expectedResult = new[]
			{
				"DMS10001|42A/67A[1]/68A[4]/23A/65A/128[1]",
				"DMS12056|42A/67A[1]/68A[1]/02A/D031[1]",
				"DMS10002|90B/20W[3]",
				"CDS10001|42A/67A/68A[1]/114[1]",
				"CDS12056|42A/67A/68A/02A/D006[1]",
				"CDS10020|42A/67A[1]/68A[1]/02A[4]/D005[1]",
				"CDS12070|42A/02A[4]/D031[1]"
			};

			var actualResult = errorWithPointers.Select(c => c.Code + "|" + c.GetPointers()).ToArray();
			AssertContainsExactElementsInAnyOrder(expectedResult, actualResult);

			expectedResult = new[]
			{
				"Declaration/GoodsShipment/GovernmentAgencyGoodsItem[4]/Commodity/GoodsMeasure/NetNetWeightMeasure",
				"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/AdditionalDocument/CategoryCode",
				"absent/absent[3]",
				"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/StatisticalValueAmount",
				"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/AdditionalDocument[4]/ID",
				"Declaration/GoodsShipment/GovernmentAgencyGoodsItem/AdditionalDocument/TypeCode",
				"Declaration/AdditionalDocument[4]/CategoryCode"
			};

			var friendlyCodeWithPointers = errorWithPointers.Select(c => GetFriendlyCodeWithPointers(c));

			actualResult = friendlyCodeWithPointers.Select(c => c.PseudoXpath).ToArray();
			AssertContainsExactElementsInAnyOrder(expectedResult, actualResult);

			ZString messageWithoutErrorXmlContent = @"
<_2:MetaData xmlns:_2=""urn:wco:datamodel:WCO:DocumentMetaData-DMS:2"">
	<_2:WCODataModelVersionCode>3.6</_2:WCODataModelVersionCode>
	<_2:WCOTypeName>RES</_2:WCOTypeName>
	<_2:ResponsibleCountryCode/>
	<_2:ResponsibleAgencyName/>
	<_2:AgencyAssignedCustomizationCode/>
	<_2:AgencyAssignedCustomizationVersionCode/>
	<_2_1:Response xmlns:_2_1=""urn:wco:datamodel:WCO:RES-DMS:2"">
		<_2_1:FunctionCode>03</_2_1:FunctionCode>
		<_2_1:FunctionalReferenceID>c41cb7554783489c94e24939cb1ccf51</_2_1:FunctionalReferenceID>
		<_2_1:Declaration>
			<_2_1:FunctionalReferenceID>Import_Obligation_REJ</_2_1:FunctionalReferenceID>
			<_2_1:ID>18GBJCUDI9ADRHWD54</_2_1:ID>
			<_2_1:VersionID>1</_2_1:VersionID>
		</_2_1:Declaration>
	</_2_1:Response>
</_2:MetaData>";

			metaData = XmlObjectSerializer.Deserialize<TMetaData>(messageWithoutErrorXmlContent);

			errorWithPointers = GetResponses(metaData);

			AssertNoExceptionThrown(() => errorWithPointers.Select(c => c.Code + "|" + c.GetPointers()).ToArray());
		}
	}
}
