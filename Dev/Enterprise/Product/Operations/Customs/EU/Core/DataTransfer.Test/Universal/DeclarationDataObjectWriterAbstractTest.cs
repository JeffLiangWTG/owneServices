using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using NUnit.Framework;

namespace Enterprise.Customs.EU.DataTransfer.Universal.Testing
{
	[TestedType(typeof(DeclarationDataObjectWriter))]
	public abstract class DeclarationDataObjectWriterAbstractTest<TEUJobDeclaration, TDeclarationDataObjectWriter> : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
		where TEUJobDeclaration : JobDeclaration
		where TDeclarationDataObjectWriter : DeclarationDataObjectWriter
	{
		public void TestBothModelViewAndBaseEUAddInfoAreWritten()
		{
			var declaration = Factory.New<TEUJobDeclaration>();
			if (declaration is IAddInfoSchemaProvider schemaProvider)
			{
				var schema = schemaProvider.AddInfoTableSchema;
				var countryAddInfo = schema.All.First(c => !Schema.IsSystemColumn(c.ObjectName));
				var countryAddInfoName = countryAddInfo.Name;
				var countryAddInfoNameString = countryAddInfoName.Substring(countryAddInfoName.IndexOf('_') + 1);
				var countryAddInfoValue = BusinessObjectHelper.GetNonDefaultValueForZType(countryAddInfo.GetEquivalentZType());

				((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator();
				declaration.ZG_VATDeferType = DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14;
				declaration[countryAddInfoName] = countryAddInfoValue;
				var writer = new DeclarationDataObjectWriterForTest(new DataWritingManager(new ActionInfo(null, declaration)));
				var declarationData = writer.GetDataObject(declaration);
				CombineAssertions(() =>
				{
					AssertEquals("VATDeferType", DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14, declarationData.AddInfoCollection.GetZStringValue("VATDeferType"));
					AssertEquals(countryAddInfoNameString, countryAddInfoValue.GetStringRepresentation(), declarationData.AddInfoCollection.GetZStringValue(countryAddInfoNameString));
				});
			}
			else
			{
				Assert("This test is only valid for declarations that implement IAddInfoSchemaProvider", true);
			}
		}
	}
}
