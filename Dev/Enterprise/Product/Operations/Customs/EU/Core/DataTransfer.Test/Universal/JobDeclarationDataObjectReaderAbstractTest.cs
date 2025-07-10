using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using UniversalAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.EU.DataTransfer.Universal.Testing
{
	[TestedType(typeof(JobDeclarationDataObjectReader))]
	public abstract class JobDeclarationDataObjectReaderAbstractTest<TEUJobDeclaration, TJobDeclarationDataObjectReader> : Customs.DataTransfer.Universal.Testing.DataObjectReaderTest
		where TEUJobDeclaration : JobDeclaration
		where TJobDeclarationDataObjectReader : JobDeclarationDataObjectReader
	{
		public void TestBothModelViewAndBaseEUAddInfoAreRead_DefaultDisabled() => AssertBothModelViewAndBaseEUAddInfoAreRead(false);
		public void TestBothModelViewAndBaseEUAddInfoAreRead_DefaultEnabled() => AssertBothModelViewAndBaseEUAddInfoAreRead(true);
		void AssertBothModelViewAndBaseEUAddInfoAreRead(bool enableDefaulting)
		{
			var declaration = Factory.New<TEUJobDeclaration>();
			if (declaration is IAddInfoSchemaProvider schemaProvider)
			{
				var schema = schemaProvider.AddInfoTableSchema;
				var countryAddInfo = schema.All.First(c => !Schema.IsSystemColumn(c.ObjectName));
				var countryAddInfoName = countryAddInfo.Name;
				var countryAddInfoNameString = countryAddInfoName.Substring(countryAddInfoName.IndexOf('_') + 1);
				var countryAddInfoValue = BusinessObjectHelper.GetNonDefaultValueForZType(countryAddInfo.GetEquivalentZType());

				using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableDefaulting))
				{
					var declarationDataObject = SetupDeclaration(null, "MB2343", new WayBillType() { Code = WayBillTypeList.Codes.Master });

					declarationDataObject.SetAddInfoCollection(() => new List<UniversalAddInfo>
					{
						new UniversalAddInfo { Key = EUAddInfoSchema.Constants.ZG_VATDeferType.Substring(3), Value = DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14 },
						new UniversalAddInfo { Key = countryAddInfoNameString, Value = countryAddInfoValue.GetStringRepresentation() },
					});

					var reader = (TJobDeclarationDataObjectReader)Activator.CreateInstance(typeof(TJobDeclarationDataObjectReader), BindingFlags.Default, null, [declarationDataObject, logger, Factory, null], null);
					var declarationBO = reader.ReadIntoBusinessObject();

					CombineAssertions(() =>
					{
						AssertEquals("VATDeferType", DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14, declarationBO.ZG_VATDeferType);
						AssertEquals(countryAddInfoNameString, countryAddInfoValue, declarationBO[countryAddInfoName]);
					});
				}
			}
			else
			{
				Assert("This test is only valid for declarations that implement IAddInfoSchemaProvider", true);
			}
		}
	}

	class JobDeclarationDataObjectReaderWithCountryAddInfoTest : JobDeclarationDataObjectReaderAbstractTest<JobDeclarationWithCountryAddInfo, JobDeclarationWithCountryAddInfoDataObjectReader>
	{
	}

	class JobDeclarationWithCountryAddInfoDataObjectReader : JobDeclarationDataObjectReader
	{
		public JobDeclarationWithCountryAddInfoDataObjectReader(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment forwardingShipment = null)
			: base(declarationDataObject, logger, factory, forwardingShipment)
		{
		}

		protected override JobDeclaration GetNewBusinessObjectCore()
		{
			return factory.New<JobDeclarationWithCountryAddInfo>();
		}
	}

	class JobDeclarationWithCountryAddInfo : JobDeclaration, IAddInfoManagerWithSchema, IAddInfoSchemaProvider
	{
		public JobDeclarationWithCountryAddInfo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override void OnSaving()
		{
			base.OnSaving();
			JE_AddInfo = AddInfoParser.Serialise(ZZDummyBizoSchema.Constants.Z0_AddInfoBool, Z0_AddInfoBool.GetStringRepresentation());
		}

		public virtual ZBool Z0_AddInfoBool
		{
			get => Z0_AddInfoBoolData.Value;
			set
			{
				SetNonPersistentPropertyValue(Z0_AddInfoBoolInfo, ref Z0_AddInfoBoolData.Value, value);
			}
		}
		public ZPropertyInfo Z0_AddInfoBoolInfo => GetZPropertyInfo(ZZDummyBizoSchema.Constants.Z0_AddInfoBool);
		AddInfoPropertyData<ZBool> Z0_AddInfoBoolData => addInfoBoolData ?? (addInfoBoolData = new AddInfoPropertyData<ZBool>(ZZDummyBizoSchema.Constants.Z0_AddInfoBool));
		AddInfoPropertyData<ZBool> addInfoBoolData;

		ITableSchema IAddInfoManagerWithSchema.AddInfoSchema => new CombinedAddInfoSchema(JobDeclarationSchema.Instance, new ITableSchema[] { ZZDummyBizoSchema.Instance, EUAddInfoSchema.Instance });

		ITableSchema IAddInfoSchemaProvider.AddInfoTableSchema => ZZDummyBizoSchema.Instance;

		protected override IDictionary<string, IAddInfoPropertyData> GetAddInfoNamesMapping() => new Dictionary<string, IAddInfoPropertyData>
		{
			{ "AddInfoBool", Z0_AddInfoBoolData }
		}.Union(base.GetAddInfoNamesMapping()).ToDictionary(x => x.Key, x => x.Value);
	}
}
