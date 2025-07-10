using System;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP.DF_MSG10000_ImportDeclaration;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP.NS3;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class ImportDeclarationResponseAdaptorTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("importDeclarationResponse is must", () => new ImportDeclarationResponseAdaptor(null));
		}

		public void TestProperties()
		{
			var importDeclarationResponse = new DfNg2754Msg10004ImportDeclarationResponse();
			importDeclarationResponse.Response = new Response()
			{
				Declaration = new Declaration()
				{
					DmExtensions = new DeclarationDmExtensions()
					{
						ExternalDeclarationId = new ExternalDeclarationIdType()
						{
							Value = "42430006402024"
						},
						VersionId = new DeclarationDmExtensionsVersionId()
						{
							Value = "0.3"
						}
					},
					Id = new DeclarationIdentificationIdType()
					{
						Value = "24013304469780"
					},
					DutyTaxFee = new System.Collections.ObjectModel.Collection<DeclarationDutyTaxFee>
					{
						new DeclarationDutyTaxFee { TypeCode = new DutyTaxFeeTypeCodeType { Value = "A" } },
						new DeclarationDutyTaxFee { TypeCode = new DutyTaxFeeTypeCodeType { Value = "B" } }
					},
					GoodsShipment = new System.Collections.ObjectModel.Collection<DeclarationGoodsShipment>
					{
						new DeclarationGoodsShipment { }
					}
				},
				Status = new ResponseStatus()
				{
					NameCode = new StatusNameCodeType()
					{
						Value = "13"
					}
				},
			};
			var importDeclarationResponseAdaptor = new ImportDeclarationResponseAdaptor(importDeclarationResponse);

			CombineAssertions("Test mapping properties", () =>
			{
				AssertEquals("ExternalDeclarationID", "42430006402024", importDeclarationResponseAdaptor.ExternalDeclarationID);
				AssertEquals("CustomsStatusNameCode", "13", importDeclarationResponseAdaptor.CustomsStatusNameCode);
				AssertEquals("DeclarationVersionID", "0.3", importDeclarationResponseAdaptor.DeclarationVersionID);
				AssertEquals("CustomsDeclarationNumber", "24013304469780", importDeclarationResponseAdaptor.CustomsDeclarationNumber);
				AssertEquals("GoodsShipmentList", 1, importDeclarationResponseAdaptor.GoodsShipmentList.Count);
				AssertEquals("DeclarationDutyTaxFeeList", 2, importDeclarationResponseAdaptor.DeclarationDutyTaxFeeList.Count);
				Assert("IsResponsePresent", importDeclarationResponseAdaptor.IsResponsePresent);
			});
		}

		public void TestProperties_NullGuard()
		{
			var importDeclarationResponseAdaptorEmpty = new ImportDeclarationResponseAdaptor(new DfNg2754Msg10004ImportDeclarationResponse() { });
			CombineAssertions("Test Null Guard", () =>
			{
				AssertNullOrEmpty("ExternalDeclarationID", importDeclarationResponseAdaptorEmpty.ExternalDeclarationID);
				AssertNullOrEmpty("CustomsStatusNameCode", importDeclarationResponseAdaptorEmpty.CustomsStatusNameCode);
				AssertNullOrEmpty("DeclarationVersionID", importDeclarationResponseAdaptorEmpty.DeclarationVersionID);
				AssertNullOrEmpty("CustomsDeclarationNumber", importDeclarationResponseAdaptorEmpty.CustomsDeclarationNumber);
				AssertNull("GoodsShipmentList", importDeclarationResponseAdaptorEmpty.GoodsShipmentList);
				AssertNull("DeclarationDutyTaxFeeList", importDeclarationResponseAdaptorEmpty.DeclarationDutyTaxFeeList);
				Assert("IsResponsePresent", !importDeclarationResponseAdaptorEmpty.IsResponsePresent);
			});
		}
	}
}
